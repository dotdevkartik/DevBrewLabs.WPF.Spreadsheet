using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class Cell : ICell, IDisposable
    {
        private Cells _parentRange;
        private WorkSheet _workSheet;

        public IFormatter Formatter
        {
            get
            {
                return _workSheet.GetFormatter(Row, Column);
            }
            set
            {
                _workSheet.SetFormatter(Row, Column, value);
            }
        }

        public object Value
        {
            get
            {
                return _workSheet.GetValue(Row, Column);
            }
            set
            {
                _workSheet.SetValue(Row, Column, value);
            }
        }

        public string Formula
        {
            get
            {
                return _workSheet.GetFormula(Row, Column);
            }
            set
            {
                _workSheet.SetFormula(Row, Column, value);
            }
        }

        public string StyleName
        {
            get
            {
                return _workSheet.GetStyleName(Row, Column);
            }
            set
            {
                _workSheet.SetStyleName(Row, Column, value);
            }
        }

        public IStyle Style
        {
            get
            {
                return _workSheet.GetStyle(Row, Column);
            }
            set
            {
                _workSheet.SetStyle(Row, Column, value);
            }
        }

        public IDataMap DataMap
        {
            get
            {
                return _workSheet.GetDataMap(Row, Column);
            }
            set
            {
                _workSheet.SetDataMap(Row, Column, value);
            }
        }

        public ICellType CellType
        {
            get
            {
                return _workSheet.GetCellType(Row, Column);
            }
            set
            {
                _workSheet.SetCellType(Row, Column, value);
            }
        }

        public bool Locked
        {
            get
            {
                return _workSheet.GetLocked(Row, Column);
            }
            set
            {
                _workSheet.SetLocked(Row, Column, value);
            }
        }

        public int RowSpan
        {
            get
            {
                return _workSheet.GetRowSpan(Row, Column);
            }
            set
            {
                _workSheet.SetRowSpan(Row, Column, value);
            }
        }

        public int ColumnSpan
        {
            get
            {
                 return _workSheet.GetColumnSpan(Row, Column);
            }
            set
            {
                _workSheet.SetColumnSpan(Row, Column, value);
            }
        }

        public int Row { get; set; }
        public int Column { get; set; }
        public IRange ParentRange => _parentRange;

        internal Cell(Cells parent)
        {
            _parentRange = parent;
            _workSheet = parent.WorkSheet;
        }

        public void Dispose()
        {
            Value = null;
            Formula = null;
            Formatter = null;

            DataMap = null;
            _parentRange = null;
            CellType = null;
            StyleName = null;
            Style = null;
        }
    }
}
