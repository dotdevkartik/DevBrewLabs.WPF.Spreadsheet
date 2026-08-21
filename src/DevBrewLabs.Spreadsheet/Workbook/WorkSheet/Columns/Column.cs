using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class Column : IColumn, IDisposable
    {
        private int _width;
        private DataMap _dataMap;
        private Columns _parent;
        private string _styleName;
        private IStyle _style;

        public IFormatter Formatter { get; set; }

        public int Width
        {
            get
            {
                if (_width < 0)
                {
                    return _parent.WorkSheet.DefaultColumnWidth;
                }

                return _width;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Column width can't be negative.");

                int oldWidth = Width;
                if (value == oldWidth)
                {
                    return;
                }

                _width = value;

                _parent.WorkSheet?.OnColumnChanged(new ColumnChangedEventArgs(
                    SheetRegion.Cells,
                    _parent.WorkSheet,
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
                string oldStyleName = _styleName;

                if (value == oldStyleName)
                {
                    return;
                }

                _styleName = value;

                _parent.WorkSheet?.OnColumnChanged(new ColumnChangedEventArgs(
                    SheetRegion.Cells,
                    _parent.WorkSheet,
                    Index,
                    oldStyleName,
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
                var oldStyle = _style;

                if (oldStyle == _style)
                {
                    return;
                }

                if (_style != value)
                {
                    _parent.WorkSheet?.OnColumnChanged(new ColumnChangedEventArgs(
                       SheetRegion.Cells,
                       _parent.WorkSheet,
                       Index,
                       oldStyle,
                       value,
                       ColumnChangeType.Style));
                }

                _style = value;
            }
        }

        public IColumns Parent => _parent;
        public DataMap DataMap
        {
            get
            {
                return _dataMap;
            }
            set
            {
                _dataMap = value;
                OnDataMapChanged();
            }
        }
        public ICellType CellType { get; set; }
        public bool Locked { get; set; }
        public bool AllowFiltering { get; set; } = true;
        public bool Visible => Width > 0;
        public int Index { get; internal set; }

        internal Column(Columns parent)
        {
            _parent = parent;
            _width = -1;
            Locked = false;
        }

        private void OnDataMapChanged()
        {
            
        }

        public void Dispose()
        {
            StyleName = null;
            CellType = null;
            DataMap = null;
            Formatter = null;
            _parent = null;
        }
    }
}
