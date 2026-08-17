using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet
{
    internal class WorkBook : IWorkBook
    {
        private WorkbookAdapter _dataProvider;
        private IChangeListener _changeListener;
        private Dictionary<string, IStyle> _namedStyles;

        public string Name { get; set; }
        public IWorkSheets WorkSheets { get; private set; }
        public ICalcEngine CalcEngine { get; private set; }
        public IStylePalette StylePalette { get; private set; }
        internal IChangeListener ChangeListener => _changeListener;

        public WorkBook(string name)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            WorkSheets = new WorkSheets(this);
            _namedStyles = new Dictionary<string, IStyle>();
            _dataProvider = new WorkbookAdapter(this);
            CalcEngine = new SheetCalcEngine(_dataProvider);
            StylePalette = new StylePalette();
            AddDefaultStyles();
        }

        private void AddDefaultStyles()
        {
            var rowHeaderStyle = new CellStyle
            {
                FontSize = 14,
                HorizontalAlignment = CellHorizontalAlignment.Center,
                BackColor = Drawing.CellColor.Gray
            };

            AddNamedStyle(StyleKeys.DefaultRowHeaderStyleKey, rowHeaderStyle);

            var columnHeaderStyle = new CellStyle
            {
                FontSize = 14,
                HorizontalAlignment = CellHorizontalAlignment.Center,
                BackColor = Drawing.CellColor.Gray
            };

            AddNamedStyle(StyleKeys.DefaultColumnHeaderStyleKey, columnHeaderStyle);

            var sheetStyle = new CellStyle
            {
                BackColor = Drawing.CellColor.White,
                AllowMultiLineText = true,
            };

            AddNamedStyle(StyleKeys.DefaultSheetStyleKey, sheetStyle);

            var topLeftStyle = new CellStyle
            {
                ForeColor = Drawing.CellColor.LightGray
            };
            AddNamedStyle(StyleKeys.DefaultTopLeftStyleKey, topLeftStyle);

            rowHeaderStyle.BackColor = topLeftStyle.BackColor = columnHeaderStyle.BackColor = Drawing.CellColor.FromArgb(255, 240, 240, 240);
        }

        internal WorkBook(string name, IChangeListener updateProvider) : this(name)
        {
            if(updateProvider == null)
                throw new ArgumentNullException(nameof(updateProvider));

            _changeListener = updateProvider;
        }

        public void AddNamedStyle(string styleName, CellStyle style)
        {
            if (_namedStyles.ContainsKey(styleName))
                throw new ArgumentException($"A style is already registered with the name '{styleName}'");

            _namedStyles.Add(styleName, style);
        }

        public IStyle GetNamedStyle(string styleName)
        {
            if(_namedStyles.TryGetValue(styleName, out IStyle style))
                return style;

            return null;
        }

        public void Dispose()
        {
            WorkSheets.Dispose();
            _namedStyles.Clear();
            StylePalette?.Clear();
            StylePalette = null;
            WorkSheets = null;
            CalcEngine = null;
            _namedStyles = null;
            _dataProvider = null;
        }

        #region adapter
        internal void RaiseValueChanged(ValueChangedEventArgs args)
        {
           _dataProvider.RaiseValueChanged(args);
        }

        internal void RaiseFormulaChanged(FormulaChangedEventArgs args)
        {
            _dataProvider.RaiseFormulaChanged(args);
        }

        private class WorkbookAdapter : IDataAdapter, IDisposable
        {
            private WorkBook _workBook;

            public event ValueChangedEventHandler ValueChanged;
            public event FormulaChangedEventHandler FormulaChanged;

            public WorkbookAdapter(WorkBook workBook)
            {
                _workBook = workBook;
            }

            public object[,] GetRangeValue(string sheetName, int rowIndex, int columnIndex, int rowCount, int columnCount)
            {
                var worksheet = _workBook.WorkSheets.GetSheet(sheetName);
                return worksheet.GetData(rowIndex, columnIndex, rowCount, columnCount);
            }

            public object GetValue(string sheetName, int rowIndex, int columnIndex)
            {
                var worksheet = _workBook.WorkSheets.GetSheet(sheetName);
                return worksheet.GetValue(rowIndex, columnIndex);
            }

            public void SetMetadata(string sheetName, int row, int column, object data)
            {
                var worksheet = (WorkSheet)_workBook.WorkSheets.GetSheet(sheetName);
                worksheet.SetMetadata(row, column, data);
            }

            public object GetMetadata(string sheetName, int row, int column)
            {
                var worksheet = (WorkSheet)_workBook.WorkSheets.GetSheet(sheetName);
                return worksheet.GetMetadata(row, column);
            }

            public string GetFormula(string sheetName, int row, int column)
            {
                var worksheet = _workBook.WorkSheets.GetSheet(sheetName);
                return worksheet.GetFormula(row, column);
            }

            internal void RaiseValueChanged(ValueChangedEventArgs args)
            {
                ValueChanged?.Invoke(args);
            }

            internal void RaiseFormulaChanged(FormulaChangedEventArgs args)
            {
                FormulaChanged?.Invoke(args);
            }

            public void Dispose()
            {
                _workBook = null;
            }
        }
        #endregion
    }
}
