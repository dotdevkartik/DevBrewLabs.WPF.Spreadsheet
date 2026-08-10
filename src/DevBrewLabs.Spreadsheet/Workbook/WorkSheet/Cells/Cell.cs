using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class Cell : IRange, IDisposable
    {
        private Cells _parentRange;
        private WorkSheet _workSheet;

        private ColumnData ColData => _workSheet.GetColumnData(Column, true);
        private ColumnData ColDataReadOnly => _workSheet.GetColumnData(Column, false);

        public IFormatter Formatter
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return null;
                }
                return ColDataReadOnly.GetFormatter(Row);
            }
            set
            {
                var oldValue = Formatter;

                if (oldValue == value)
                {
                    return;
                }

                ColData.SetFormatter(Row, value);
            }
        }

        public object Value
        {
            get
            {
                return _workSheet.DataStore.GetValue(Row, Column);
            }
            set
            {
                var oldValue = Value;

                if (oldValue == value)
                {
                    return;
                }

                if (HasFormula && value != null)
                    Formula = null;

                _workSheet.DataStore.SetValue(Row, Column, value);

                _workSheet.OnCellChanged(new CellChangedEventArgs(
                       SheetRegion.Cells,
                       Row,
                       Column,
                       oldValue,
                       value,
                       CellChangeType.Value));
            }
        }

        public string Formula
        {
            get
            {
                return _workSheet.GetCellFormula(Row, Column);
            }
            set
            {
                var oldValue = Formula;

                if (oldValue == value)
                {
                    return;
                }

                if (value != null && Value != null)
                    Value = null;

                _workSheet.SetCellFormula(Row, Column, value);

                _workSheet.OnCellChanged(new CellChangedEventArgs(
                      SheetRegion.Cells,
                      Row,
                      Column,
                      oldValue,
                      value,
                      CellChangeType.Formula));
            }
        }

        public string StyleName
        {
            get
            {
                if (ColDataReadOnly != null)
                    return ColDataReadOnly.GetStyleName(Row);

                return null;
            }
            set
            {
                var oldStyleName = StyleName;

                if (oldStyleName == value)
                {
                    return;
                }

                ColData.SetStyleName(Row, value);
                _workSheet.OnCellChanged(new CellChangedEventArgs(
                    SheetRegion.Cells,
                    Row,
                    Column,
                    oldStyleName,
                    value,
                    CellChangeType.Style));
            }
        }

        public IStyle Style
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return null;
                }

                ushort styleId = ColDataReadOnly.GetStyleId(Row);
                return _workSheet.GetStyle(styleId);
            }
            set
            {
                var oldStyle = Style;

                if (oldStyle == value)
                {
                    return;
                }

                ushort styleId = _workSheet.GetOrAddStyle(value);
                ColData.SetStyleId(Row, styleId);

                _workSheet.OnCellChanged(new CellChangedEventArgs(
                       SheetRegion.Cells,
                       Row,
                       Column,
                       oldStyle,
                       value,
                       CellChangeType.Style));
            }
        }

        public DataMap DataMap
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return null;
                }

                return ColDataReadOnly.GetDataMap(Row) as DataMap;
            }
            set
            {
                ColData.SetDataMap(Row, value);
            }
        }

        public ICellType CellType
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return null;
                }

                return ColDataReadOnly.GetCellType(Row);
            }
            set
            {
                ColData.SetCellType(Row, value);
            }
        }

        public bool Locked
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return false;
                }

                return ColDataReadOnly.GetLocked(Row);
            }
            set
            {
                ColData.SetLocked(Row, value);
            }
        }

        public int RowSpan
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return 0;
                }
                return ColDataReadOnly.GetRowSpan(Row);
            }
            set
            {
                var oldValue = RowSpan;

                if (oldValue == value)
                {
                    return;
                }

                ColData.SetRowSpan(Row, value);
            }
        }

        public int ColumnSpan
        {
            get
            {
                if (ColDataReadOnly == null)
                {
                    return 0;
                }
                return ColDataReadOnly.GetColumnSpan(Row);
            }
            set
            {
                var oldValue = ColumnSpan;

                if (oldValue == value)
                {
                    return;
                }

                ColData.SetColumnSpan(Row, value);
            }
        }

        public int RowCount => 1;
        public int ColumnCount => 1;
        internal object MetaData { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool HasFormula => !string.IsNullOrEmpty(Formula);
        public IRange ParentRange => _parentRange;
        public bool IsVisible { get; internal set; }

        public IRange this[int row, int column, int rowCount, int columnCount] => this;
        public IRange this[int row, int column] => this;
        public IRange this[string name] => this;

        internal Cell(Cells parent)
        {
            _parentRange = parent;
            _workSheet = parent.WorkSheet;
            IsVisible = true;
        }

        public void Dispose()
        {
            Value = null;
            Formula = null;
            Formatter = null;
            MetaData = null;
            DataMap = null;
            _parentRange = null;
            CellType = null;
            StyleName = null;
            Style = null;
        }
    }
}
