using DevBrewLabs.Spreadsheet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.CalcEngine.Utils;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    /// <summary>
    /// Enterprise-grade clipboard manager handling copy, paste, Win32 clipboard locking retries,
    /// internal data objects, TSV/CSV format parsing, formula insertion, and undo/redo integration.
    /// </summary>
    internal class ClipboardManager
    {
        private static IDataObject _lastCopiedDataObject;
        private readonly Spread _spread;
        private const int MaxClipboardRetries = 1;
        private const int ClipboardRetryDelayMs = 30;

        public ClipboardManager(Spread spread)
        {
            _spread = spread ?? throw new ArgumentNullException(nameof(spread));
        }

        #region Public Methods

        public void Cut(ISheetView sheetView)
        {
            if (sheetView == null)
                throw new ArgumentNullException(nameof(sheetView));

            Cut(sheetView, sheetView.Selection);
        }

        public void Cut(ISheetView sheetView, CellRange range)
        {
            if (sheetView == null || range == default || sheetView.WorkSheet == null)
                return;

            if (range.RowCount <= 0 || range.ColumnCount <= 0)
                return;

            var concreteSheetView = sheetView as SheetView;
            if (concreteSheetView == null)
                return;

            // 1. Copy to clipboard
            Copy(sheetView, range);

            // 2. Clear cells and record undo action
            _spread.SuspendUpdates = true;
            try
            {
                var ws = concreteSheetView.WorkSheet;
                var rows = ws.Rows;
                var columns = ws.Columns;
                var colCache = new IColumn[range.ColumnCount];
                for (int c = 0; c < range.ColumnCount; c++)
                {
                    colCache[c] = columns.GetItem(range.LeftColumn + c);
                }

                string[,] oldFormulas = new string[range.RowCount, range.ColumnCount];
                string[,] newFormulas = new string[range.RowCount, range.ColumnCount];
                object[,] oldValues = ws.GetData(range.TopRow, range.LeftColumn, range.RowCount, range.ColumnCount);
                object[,] newValues = new object[range.RowCount, range.ColumnCount];
                bool anyCleared = false;

                for (int r = 0; r < range.RowCount; r++)
                {
                    int actualRow = range.TopRow + r;
                    var sheetRow = rows.GetItem(actualRow);

                    for (int c = 0; c < range.ColumnCount; c++)
                    {
                        int actualCol = range.LeftColumn + c;
                        string oldFormula = ws.GetFormula(actualRow, actualCol);
                        oldFormulas[r, c] = oldFormula;

                        if (ws.IsCellLocked(actualRow, actualCol, sheetRow, colCache[c]))
                        {
                            newValues[r, c] = oldValues[r, c];
                            newFormulas[r, c] = oldFormula;
                            continue;
                        }

                        if (oldValues[r, c] != null || !string.IsNullOrEmpty(oldFormula))
                        {
                            anyCleared = true;
                        }

                        ws.SetValue(actualRow, actualCol, null);
                        ws.SetFormula(actualRow, actualCol, null);
                    }
                }

                if (anyCleared)
                {
                    var pasteAction = new ClipboardPasteAction { SheetView = concreteSheetView };
                    pasteAction.OldState.Value = oldValues;
                    pasteAction.OldState.Formulas = oldFormulas;
                    pasteAction.OldState.Row = range.TopRow;
                    pasteAction.OldState.Column = range.LeftColumn;
                    pasteAction.OldState.Selection = concreteSheetView.Selection.Clone();

                    pasteAction.NewState.Value = newValues;
                    pasteAction.NewState.Formulas = newFormulas;
                    pasteAction.NewState.Row = range.TopRow;
                    pasteAction.NewState.Column = range.LeftColumn;
                    pasteAction.NewState.Selection = concreteSheetView.Selection.Clone();

                    _spread.UndoRedoManager.AddAction(pasteAction);
                }
            }
            finally
            {
                _spread.SuspendUpdates = false;
            }

            if (concreteSheetView.AutoSizeRows)
            {
                for (int r = range.TopRow; r <= range.BottomRow; r++)
                {
                    concreteSheetView.AutoSizeRow(r);
                }
            }

            _spread.Refresh();
        }

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

            var formulas = new string[range.RowCount, range.ColumnCount];
            for (int r = 0; r < range.RowCount; r++)
            {
                for (int c = 0; c < range.ColumnCount; c++)
                {
                    if (!sheetView.WorkSheet.IsCovered(range.TopRow + r, range.LeftColumn + c))
                    {
                        formulas[r, c] = sheetView.WorkSheet.GetFormula(range.TopRow + r, range.LeftColumn + c);
                    }
                }
            }

            var dataObject = new DataObject();
            var textContent = stringBuilder.ToString();
            dataObject.SetData(DataFormats.UnicodeText, textContent);
            dataObject.SetData(DataFormats.Text, textContent);
            dataObject.SetData("InternalDataObject", data);
            dataObject.SetData("InternalFormulasObject", formulas);
            dataObject.SetData("InternalSourceOrigin", new int[] { range.TopRow, range.LeftColumn });

            _lastCopiedDataObject = dataObject;
            ExecuteWithRetry(() => Clipboard.SetDataObject(dataObject));
        }

        public void Paste(ISheetView sheetView)
        {
            if (sheetView == null || sheetView.WorkSheet == null)
                return;

            var dataObject = ExecuteWithRetry(() => Clipboard.GetDataObject()) ?? _lastCopiedDataObject;
            if (dataObject == null)
                return;

            object[,] data = ExtractClipboardData(dataObject);
            if (data == null && !ReferenceEquals(dataObject, _lastCopiedDataObject) && _lastCopiedDataObject != null)
            {
                dataObject = _lastCopiedDataObject;
                data = ExtractClipboardData(dataObject);
            }
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
                int pasteRows = data.GetLength(0);
                int pasteCols = data.GetLength(1);
                string[,] oldFormulas = new string[pasteRows, pasteCols];
                for (int r = 0; r < pasteRows; r++)
                {
                    for (int c = 0; c < pasteCols; c++)
                    {
                        oldFormulas[r, c] = concreteSheetView.WorkSheet.GetFormula(activeRow + r, activeColumn + c);
                    }
                }

                var pasteAction = new ClipboardPasteAction() { SheetView = concreteSheetView };
                pasteAction.OldState.Value = concreteSheetView.WorkSheet.GetData(activeRow, activeColumn, pasteRows, pasteCols);
                pasteAction.OldState.Formulas = oldFormulas;
                pasteAction.OldState.Row = activeRow;
                pasteAction.OldState.Column = activeColumn;
                pasteAction.OldState.Selection = concreteSheetView.Selection.Clone();

                var internalFormulas = dataObject.GetDataPresent("InternalFormulasObject")
                    ? dataObject.GetData("InternalFormulasObject") as string[,]
                    : null;
                var sourceOrigin = dataObject.GetDataPresent("InternalSourceOrigin")
                    ? dataObject.GetData("InternalSourceOrigin") as int[]
                    : null;
                int rowDelta = sourceOrigin != null && sourceOrigin.Length == 2 ? activeRow - sourceOrigin[0] : 0;
                int colDelta = sourceOrigin != null && sourceOrigin.Length == 2 ? activeColumn - sourceOrigin[1] : 0;

                var rows = workSheet.Rows;
                var columns = workSheet.Columns;
                var colCache = new IColumn[pasteCols];
                for (int c = 0; c < pasteCols; c++)
                {
                    colCache[c] = columns.GetItem(activeColumn + c);
                }

                bool anyModified = false;

                for (int row = 0; row < pasteRows; row++)
                {
                    int targetRow = activeRow + row;
                    var sheetRow = rows.GetItem(targetRow);

                    for (int column = 0; column < pasteCols; column++)
                    {
                        int targetCol = activeColumn + column;
                        if (workSheet.IsCovered(targetRow, targetCol) ||
                            workSheet.IsCellLocked(targetRow, targetCol, sheetRow, colCache[column]))
                        {
                            continue;
                        }

                        anyModified = true;

                        if (internalFormulas != null &&
                            row < internalFormulas.GetLength(0) &&
                            column < internalFormulas.GetLength(1) &&
                            !string.IsNullOrEmpty(internalFormulas[row, column]))
                        {
                            string shiftedFormula = FormulaRemapper.ShiftFormula(internalFormulas[row, column], rowDelta, colDelta);
                            workSheet.SetFormula(targetRow, targetCol, shiftedFormula);
                            continue;
                        }

                        var value = data[row, column];
                        if (value is string strVal)
                        {
                            try
                            {
                                workSheet.SetRawValue(targetRow, targetCol, strVal);
                            }
                            catch
                            {
                                workSheet.SetValue(targetRow, targetCol, strVal);
                            }
                        }
                        else
                        {
                            workSheet.SetValue(targetRow, targetCol, value);
                        }
                    }
                }

                _spread.SelectionManager.SelectRange(sheetView, activeRow, activeColumn, data.GetLength(0), data.GetLength(1));

                if (anyModified)
                {
                    string[,] newFormulas = new string[pasteRows, pasteCols];
                    for (int r = 0; r < pasteRows; r++)
                    {
                        for (int c = 0; c < pasteCols; c++)
                        {
                            newFormulas[r, c] = concreteSheetView.WorkSheet.GetFormula(activeRow + r, activeColumn + c);
                        }
                    }

                    pasteAction.NewState.Value = concreteSheetView.WorkSheet.GetData(activeRow, activeColumn, pasteRows, pasteCols);
                    pasteAction.NewState.Formulas = newFormulas;
                    pasteAction.NewState.Row = activeRow;
                    pasteAction.NewState.Column = activeColumn;
                    pasteAction.NewState.Selection = concreteSheetView.Selection.Clone();

                    _spread.UndoRedoManager.AddAction(pasteAction);
                }
            }
            finally
            {
                _spread.SuspendUpdates = false;
            }
        }

        public bool CanCut(ISheetView sheetView)
        {
            return sheetView != null && sheetView.Selection.RowCount > 0 && sheetView.Selection.ColumnCount > 0;
        }

        public bool CanCopy(ISheetView sheetView)
        {
            return sheetView != null && sheetView.Selection.RowCount > 0 && sheetView.Selection.ColumnCount > 0;
        }

        public bool CanPaste(ISheetView sheetView)
        {
            var dataObject = ExecuteWithRetry(() => Clipboard.GetDataObject()) ?? _lastCopiedDataObject;
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
                catch (Exception ex) when (ex is COMException || ex is ExternalException)
                {
                    if (i < MaxClipboardRetries - 1)
                    {
                        Thread.Sleep(ClipboardRetryDelayMs);
                    }
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
                catch (Exception ex) when (ex is COMException || ex is ExternalException)
                {
                    if (i < MaxClipboardRetries - 1)
                    {
                        Thread.Sleep(ClipboardRetryDelayMs);
                    }
                }
            }
        }

        #endregion
    }
}
