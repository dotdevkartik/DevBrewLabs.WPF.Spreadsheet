using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet
{
    internal class WorkBook : IWorkBook
    {
        private WorkBookDataProvider _dataProvider;
        private IUpdateProvider _updateProvider;
        private Dictionary<string, CellStyle> _namedStyles;

        public string Name { get; set; }
        public IWorkSheets WorkSheets { get; private set; }
        public ICalcEngine CalcEngine { get; private set; }
        public IStylePalette StylePalette { get; private set; }
        internal IUpdateProvider UpdateProvider => _updateProvider;
        internal WorkBookDataProvider DataProvider => _dataProvider;

        public WorkBook(string name)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            WorkSheets = new WorkSheets(this);
            _namedStyles = new Dictionary<string, CellStyle>();
            _dataProvider = new WorkBookDataProvider(this);
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

        internal WorkBook(string name, IUpdateProvider updateProvider) : this(name)
        {
            if(updateProvider == null)
                throw new ArgumentNullException(nameof(updateProvider));

            _updateProvider = updateProvider;
        }

        public void AddNamedStyle(string styleName, CellStyle style)
        {
            if (_namedStyles.ContainsKey(styleName))
                throw new ArgumentException($"A style is already registered with the name '{styleName}'");

            _namedStyles.Add(styleName, style);
        }

        public CellStyle GetNamedStyle(string styleName)
        {
            if(_namedStyles.TryGetValue(styleName, out CellStyle style))
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
    }
}
