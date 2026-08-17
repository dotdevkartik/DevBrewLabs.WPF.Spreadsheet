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

                var oldHeight = Height;

                if (oldHeight == value)
                {
                    return;
                }

                _height = value;

                _parent.WorkSheet.OnRowChanged(new RowChangedEventArgs(
                    SheetRegion.Cells,
                    _parent.WorkSheet,
                    Index,
                    oldHeight,
                    value,
                    RowChangeType.Height));
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
                string oldValue = _styleName;
                if (oldValue == value)
                {
                    return;
                }

                _styleName = value;

                _parent.WorkSheet.OnRowChanged(new RowChangedEventArgs(
                        SheetRegion.Cells,
                        _parent.WorkSheet,
                        Index,
                        oldValue,
                        value, RowChangeType.StyleName));
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

                _parent.WorkSheet.OnRowChanged(new RowChangedEventArgs(
                        SheetRegion.Cells,
                        _parent.WorkSheet,
                        Index,
                        oldValue,
                        value, RowChangeType.Style));
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
