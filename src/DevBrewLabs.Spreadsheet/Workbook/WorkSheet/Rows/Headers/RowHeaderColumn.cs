using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class RowHeaderColumn : IColumn, IDisposable
    {
        private int _width;
        private RowHeaderColumns _parent;
        private string _styleName;
        private IStyle _style;

        public int Width
        {
            get
            {
                if (_width < 0)
                {
                    return _parent.RowHeaders.DefaultColumnWidth;
                }

                return _width;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Column width can't be negative.");

                int oldWidth = Width;
                if (oldWidth == value)
                {
                    return;
                }

                _width = value;

                _parent.RowHeaders.WorkSheet.OnColumnChanged(new ColumnChangedEventArgs(
                    SheetRegion.RowHeader,
                    _parent.RowHeaders.WorkSheet,
                    Index,
                    oldWidth,
                    value,
                    ColumnChangeType.Width));
            }
        }

        public string StyleName
        {
            get
            {
                return _styleName;
            }
            set
            {
                var oldValue = _styleName;

                if (value == oldValue)
                {
                    return;
                }

                _styleName = value;

                _parent.RowHeaders.WorkSheet.OnColumnChanged(new ColumnChangedEventArgs(
                      SheetRegion.RowHeader,
                      _parent.RowHeaders.WorkSheet,
                      Index,
                      oldValue,
                      value,
                      ColumnChangeType.StyleName));
            }
        }

        public IStyle Style
        {
            get
            {
                return _style;
            }
            set
            {
                var oldValue = _style;
                if (value == oldValue)
                {
                    return;
                }

                _style = value;

                _parent.RowHeaders.WorkSheet.OnColumnChanged(new ColumnChangedEventArgs(
                       SheetRegion.RowHeader,
                       _parent.RowHeaders.WorkSheet,
                       Index,
                       oldValue,
                       value,
                       ColumnChangeType.Style));

            }
        }

        public IColumns Parent => _parent;

        public bool Visible => Width > 0;
        public bool Locked { get; set; }
        public DataMap DataMap { get; set; }
        public ICellType CellType { get; set; }
        public IFormatter Formatter { get; set; }
        public int Index { get; internal set; }

        internal RowHeaderColumn(RowHeaderColumns parent)
        {
            _parent = parent;
            _width = -1;
        }

        public void Dispose()
        {
            StyleName = null;
            _parent = null;
        }
    }
}
