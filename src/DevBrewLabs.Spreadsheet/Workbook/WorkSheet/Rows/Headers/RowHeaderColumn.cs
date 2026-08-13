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
                _parent.UpdateLocation(Index + 1, value - oldWidth);

                _parent.RowHeaders.WorkSheet.OnColumnsChanged(new ColumnChangedEventArgs(
                    SheetRegion.RowHeader,
                    Index,
                    1,
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
                if (value == _styleName)
                {
                    return;
                }

                if (_styleName != value)
                {
                    _parent.RowHeaders.WorkSheet.OnColumnsChanged(new ColumnChangedEventArgs(
                       SheetRegion.RowHeader,
                       Index,
                        1,
                        ColumnChangeType.Style));
                }

                _styleName = value;
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
                if (value == _style)
                {
                    return;
                }

                if (_style != value)
                {
                    _parent.RowHeaders.WorkSheet.OnColumnsChanged(new ColumnChangedEventArgs(
                       SheetRegion.RowHeader,
                       Index,
                        1,
                        ColumnChangeType.Style));
                }

                _style = value;
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
