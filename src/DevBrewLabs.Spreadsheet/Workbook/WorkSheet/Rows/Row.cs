using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class Row : IRow, IDisposable
    {
        private int _height;
        private string _styleName;
        private Rows _parent;
        private IStyle _style;

        public IFormatter Formatter { get; set; }

        public int Height
        {
            get
            {
                if (_height < 0)
                {
                    return _parent.WorkSheet.DefaultRowHeight;
                }

                return _height;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Row height can't be negative.");

                double oldHeight = Height;

                if (oldHeight == value)
                {
                    return;
                }

                _height = value;
                _parent.UpdateLocation(Index + 1, value - oldHeight);

                _parent.WorkSheet.OnRowsChanged(new RowChangedEventArgs(
                    SheetRegion.Cells,
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
                if (_styleName == value)
                {
                    return;
                }

                _styleName = value;

                _parent.WorkSheet.OnRowsChanged(new RowChangedEventArgs(
                        SheetRegion.Cells,
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
                    _parent.WorkSheet.OnRowsChanged(new RowChangedEventArgs(
                       SheetRegion.Cells,
                       Index,
                        1,
                        RowChangeType.Style));
                }

                _style = value;
            }
        }

        public bool Visible => Height > 0;
        public int Index { get; internal set; }
        public bool Locked { get; set; }

        internal Row(Rows parent)
        {
            _parent = parent;
            _height = -1;
        }

        public void Dispose()
        {
            Formatter = null;
            StyleName = null;
            _parent = null;
        }
    }
}
