using DevBrewLabs.Spreadsheet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using DevBrewLabs.Spreadsheet.Core;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    /// <summary>
    /// Enterprise-grade clipboard manager handling copy, paste, Win32 clipboard locking retries,
    /// internal data objects, TSV/CSV format parsing, formula insertion, and undo/redo integration.
    /// </summary>
    internal class ClipboardManager
    {
        private readonly Spread _spread;
        private const int MaxClipboardRetries = 5;
        private const int ClipboardRetryDelayMs = 30;

        public ClipboardManager(Spread spread)
        {
            _spread = spread ?? throw new ArgumentNullException(nameof(spread));
        }

        #region Public Methods

        public void Copy(ISheetView sheetView)
        {
            if (sheetView == null)
                throw new ArgumentNullException(nameof(sheetView));

            Copy(sheetView, sheetView.Selection);
        }

        public void Copy(ISheetView sheetView, CellRange range)
        {
            if (sheetView == null || range == default || sheetView.WorkSheet == null)
                return;

            if (range.RowCount <= 0 || range.ColumnCount <= 0)
                return;

            var stringBuilder = new StringBuilder();
            var data = sheetView.WorkSheet.GetData(
                range.TopRow,
                range.LeftColumn,
                range.RowCount,
                range.ColumnCount);

            for (int row = 0; row < data.GetLength(0); row++)
            {
                for (int column = 0; column < data.GetLength(1); column++)
                {
                    var val = data[row, column];
                    if (sheetView.WorkSheet.IsCovered(range.TopRow + row, range.LeftColumn + column))
                    {
                        val = null;
                        data[row, column] = null;
                    }

                    var strVal = val != null ? val.ToString() : string.Empty;
                    stringBuilder.Append(SpreadsheetDataParser.FormatTsvCell(strVal));

                    if (column < range.ColumnCount - 1)
                        stringBuilder.Append(SheetUtils.Tab);
                }

                if (row < range.RowCount - 1)
                    stringBuilder.Append(SheetUtils.NextLine);
            }

            var dataObject = new DataObject();
            var textContent = stringBuilder.ToString();
            dataObject.SetData(DataFormats.UnicodeText, textContent);
            dataObject.SetData(DataFormats.Text, textContent);
            dataObject.SetData("InternalDataObject", data);

            ExecuteWithRetry(() => Clipboard.SetDataObject(dataObject));
        }

        public void Paste(ISheetView sheetView)
        {
            if (sheetView == null || sheetView.WorkSheet == null)
                return;

            var dataObject = ExecuteWithRetry(() => Clipboard.GetDataObject());
            if (dataObject == null)
                return;

            object[,] data = ExtractClipboardData(dataObject);
            if (data == null)
                return;

            var concreteSheetView = sheetView as SheetView;
            if (concreteSheetView == null)
                return;

            int activeRow = concreteSheetView.ActiveRow;
            int activeColumn = concreteSheetView.ActiveColumn;
            var workSheet = concreteSheetView.WorkSheet;

            _spread.SuspendUpdates = true;

            try
            {
                var pasteAction = new ClipboardPasteAction() { SheetView = concreteSheetView };
                pasteAction.OldState.Value = concreteSheetView.WorkSheet.GetData(activeRow, activeColumn, data.GetLength(0), data.GetLength(1));
                pasteAction.OldState.Row = activeRow;
                pasteAction.OldState.Column = activeColumn;
                pasteAction.OldState.Selection = concreteSheetView.Selection.Clone();

                for (int row = 0; row < data.GetLength(0); row++)
                {
                    for (int column = 0; column < data.GetLength(1); column++)
                    {
                        if (workSheet.IsCovered(activeRow + row, activeColumn + column))
                        {
                            continue;
                        }

                        var value = data[row, column];
                        if (value is string strVal)
                        {
                            try
                            {
                                workSheet.SetRawValue(activeRow + row, activeColumn + column, strVal);
                            }
                            catch
                            {
                                workSheet.SetValue(activeRow + row, activeColumn + column,  strVal);
                            }
                        }
                        else
                        {
                            workSheet.SetValue(activeRow + row, activeColumn + column, value);
                        }
                    }
                }

                _spread.SelectionManager.SelectRange(sheetView, activeRow, activeColumn, data.GetLength(0), data.GetLength(1));

                pasteAction.NewState.Value = data;
                pasteAction.NewState.Row = activeRow;
                pasteAction.NewState.Column = activeColumn;
                pasteAction.NewState.Selection = concreteSheetView.Selection.Clone();

                _spread.UndoRedoManager.AddAction(pasteAction);
            }
            finally
            {
                _spread.SuspendUpdates = false;
            }
        }

        public bool CanCopy(ISheetView sheetView)
        {
            return sheetView.Selection.RowCount > 0 && sheetView.Selection.ColumnCount > 0;
        }

        public bool CanPaste(ISheetView sheetView)
        {
            var dataObject = ExecuteWithRetry(() => Clipboard.GetDataObject());
            if (dataObject == null)
                return false;

            return dataObject.GetDataPresent("InternalDataObject") ||
                   dataObject.GetDataPresent(DataFormats.UnicodeText) ||
                   dataObject.GetDataPresent(DataFormats.Text) ||
                   dataObject.GetDataPresent(DataFormats.StringFormat) ||
                   dataObject.GetDataPresent(DataFormats.CommaSeparatedValue);
        }

        #endregion

        #region Private Helper Methods

        private object[,] ExtractClipboardData(IDataObject dataObject)
        {
            if (dataObject.GetDataPresent("InternalDataObject"))
            {
                var internalData = dataObject.GetData("InternalDataObject") as object[,];
                if (internalData != null)
                    return internalData;
            }

            string text = null;
            if (dataObject.GetDataPresent(DataFormats.UnicodeText))
                text = dataObject.GetData(DataFormats.UnicodeText) as string;
            else if (dataObject.GetDataPresent(DataFormats.Text))
                text = dataObject.GetData(DataFormats.Text) as string;
            else if (dataObject.GetDataPresent(DataFormats.StringFormat))
                text = dataObject.GetData(DataFormats.StringFormat) as string;

            if (!string.IsNullOrEmpty(text))
            {
                return SpreadsheetDataParser.ParseTextData(text, '\t');
            }

            if (dataObject.GetDataPresent(DataFormats.CommaSeparatedValue))
            {
                var csvObj = dataObject.GetData(DataFormats.CommaSeparatedValue);
                if (csvObj is string csvText)
                    return SpreadsheetDataParser.ParseTextData(csvText, ',');
                if (csvObj is Stream csvStream)
                {
                    using (var reader = new StreamReader(csvStream, Encoding.UTF8))
                    {
                        return SpreadsheetDataParser.ParseTextData(reader.ReadToEnd(), ',');
                    }
                }
            }

            return null;
        }
        // Removed ParseTextData and FormatTsvCell as they are now in Core
        private static T ExecuteWithRetry<T>(Func<T> action)
        {
            for (int i = 0; i < MaxClipboardRetries; i++)
            {
                try
                {
                    return action();
                }
                catch (COMException) when (i < MaxClipboardRetries - 1)
                {
                    Thread.Sleep(ClipboardRetryDelayMs);
                }
                catch (ExternalException) when (i < MaxClipboardRetries - 1)
                {
                    Thread.Sleep(ClipboardRetryDelayMs);
                }
            }
            return default(T);
        }

        private static void ExecuteWithRetry(Action action)
        {
            for (int i = 0; i < MaxClipboardRetries; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (COMException) when (i < MaxClipboardRetries - 1)
                {
                    Thread.Sleep(ClipboardRetryDelayMs);
                }
                catch (ExternalException) when (i < MaxClipboardRetries - 1)
                {
                    Thread.Sleep(ClipboardRetryDelayMs);
                }
            }
        }

        #endregion
    }
}
