using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class ColumnHeaderRow : IRow, IDisposable
    {
        private int _height;
        private string _styleName;
        private ColumnHeaderRows _parent;
        private IStyle _style;

        public int Height
        {
            get
            {
                if (_height < 0)
                {
                    return _parent.ColumnHeaders.DefaultRowHeight;
                }

                return _height;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Row height can't be negative.");

                int oldHeight = Height;
                if (value == oldHeight)
                {
                    return;
                }

                _height = value;
                _parent.UpdateLocation(Index + 1, value - oldHeight);

                _parent.ColumnHeaders.WorkSheet.OnRowsChanged(new RowChangedEventArgs(
                    SheetRegion.ColumnHeader,
                    Index,
                    1, RowChangeType.Height));
            }
        }

        public IRows Parent => _parent;

        public string StyleName
        {
            get
            {
                return _styleName;
            }
            set
            {
                if(_styleName == value)
                {
                    return;
                }

                _styleName = value;

                _parent.ColumnHeaders.WorkSheet.OnRowsChanged(new RowChangedEventArgs(
                        SheetRegion.ColumnHeader,
                        Index,
                        1, RowChangeType.Style));
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
                    _parent.ColumnHeaders.WorkSheet.OnRowsChanged(new RowChangedEventArgs(
                       SheetRegion.ColumnHeader,
                       Index,
                        1,
                        RowChangeType.Style));
                }

                _style = value;
            }
        }

        public IFormatter Formatter { get; set; }

        public bool Visible => Height > 0;
        public int Index { get; set; }
        public bool Locked { get; set; }

        internal ColumnHeaderRow(ColumnHeaderRows parent)
        {
            _parent = parent;
            _height = -1;
        }

        public void Dispose()
        {
            StyleName = null;
            _parent = null;
        }
    }
}
