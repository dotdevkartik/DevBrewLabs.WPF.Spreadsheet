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
                _parent.UpdateLocation(Index + 1, value - oldWidth);

                _parent.WorkSheet?.OnColumnsChanged(new ColumnChangedEventArgs(
                    SheetRegion.Cells,
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

                _styleName = value;

                _parent.WorkSheet?.OnColumnsChanged(new ColumnChangedEventArgs(
                    SheetRegion.Cells,
                    Index,
                    1,
                    ColumnChangeType.Style));
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
        public bool Visible => Width > 0;
        internal Column(Columns parent)
        {
            _parent = parent;
            _width = -1;
            Locked = false;
        }
        internal int Index { get; set; }

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
