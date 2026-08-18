using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet
{
    internal class Workbook : IWorkbook
    {
        private WorkbookAdapter _dataProvider;
        private IChangeListener _changeListener;
        private Dictionary<string, IStyle> _namedStyles;

        public string Name { get; set; }
        public IWorksheets WorkSheets { get; private set; }
        public ICalcEngine CalcEngine { get; private set; }
        public IStylePalette StylePalette { get; private set; }
        internal IChangeListener ChangeListener => _changeListener;

        public Workbook(string name)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            WorkSheets = new Worksheets(this);
            _namedStyles = new Dictionary<string, IStyle>();
            _dataProvider = new WorkbookAdapter(this);
            CalcEngine = new SheetCalcEngine(_dataProvider);
            StylePalette = new StylePalette();
            AddDefaultStyles();
        }

        private void AddDefaultStyles()
        {
            var headerColor = Drawing.CellColor.FromArgb(255, 240, 240, 240);

            var rowHeaderStyle = new CellStyle
            {
                HorizontalAlignment = CellHorizontalAlignment.Center,
                VerticalAlignment = CellVerticalAlignment.Center,
                BackColor = headerColor
            };
            AddNamedStyle(StyleKeys.DefaultRowHeaderStyleKey, rowHeaderStyle);

            var columnHeaderStyle = new CellStyle
            {
                HorizontalAlignment = CellHorizontalAlignment.Center,
                VerticalAlignment = CellVerticalAlignment.Center,
                BackColor = headerColor
            };
            AddNamedStyle(StyleKeys.DefaultColumnHeaderStyleKey, columnHeaderStyle);

            var sheetStyle = new CellStyle
            {
                // Relying on CellStyle constructor defaults: Calibri 14pt, Black on White, NoWrap.
            };
            AddNamedStyle(StyleKeys.DefaultSheetStyleKey, sheetStyle);

            var topLeftStyle = new CellStyle
            {
                BackColor = headerColor,
                ForeColor = Drawing.CellColor.LightGray
            };
            AddNamedStyle(StyleKeys.DefaultTopLeftStyleKey, topLeftStyle);
        }

        internal Workbook(string name, IChangeListener updateProvider) : this(name)
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
            private Workbook _workBook;

            public event ValueChangedEventHandler ValueChanged;
            public event FormulaChangedEventHandler FormulaChanged;

            public WorkbookAdapter(Workbook workBook)
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
                var worksheet = (Worksheet)_workBook.WorkSheets.GetSheet(sheetName);
                worksheet.SetMetadata(row, column, data);
            }

            public object GetMetadata(string sheetName, int row, int column)
            {
                var worksheet = (Worksheet)_workBook.WorkSheets.GetSheet(sheetName);
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
