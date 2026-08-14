using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal class Cells : IRange, IDisposable
    {
        private int _rowCount;
        private int _columnCount;
        private WorkSheet _workSheet;
        private CellRange _cellRange;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Dictionary<long, Cell> _cellCollection;

        public IRange this[string name]
        {
            get
            {
                if(name.Contains(":"))
                {
                    var rangeRef = new CellRangeRef(name);
                    return GetRange(rangeRef.TopRow, rangeRef.LeftColumn, rangeRef.RowCount, rangeRef.ColumnCount);
                }
                else
                {
                    var cell = new CellRef(name);
                    return GetRange(cell.Row, cell.Column, 1, 1);
                }
            }
        }

        public ICell this[int row, int column]
        {
            get
            {
                return GetCell(row, column);
            }
        }

        public IRange this[int row, int column, int rowCount, int columnCount]
        {
            get
            {
                return GetRange(row, column, rowCount, columnCount);
            }
        }

        public int Row { get; }

        public int Column { get; }

        public int RowCount
        {
            get
            {
                if (_rowCount == -1)
                    return _workSheet.RowCount;

                return _rowCount;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (_columnCount == -1)
                    return _workSheet.ColumnCount;

                return _columnCount;
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
                _workSheet.ExecuteSupressed(() =>
                {
                    ApplyToRange((row, column) => _workSheet.SetValue(row, column, value));
                    OnRangeChanged(RangeChangeType.Value);
                });
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
                _workSheet.ExecuteSupressed(() =>
                {
                    ApplyToRange((row, column) => _workSheet.SetFormula(row, column, value));
                    OnRangeChanged(RangeChangeType.Formula);
                });
            }
        }

        public IFormatter Formatter
        {
            get
            {
                return _workSheet.GetFormatter(Row, Column);
            }
            set
            {
                ApplyToRange((row, column) => _workSheet.SetFormatter(row, column, value));
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
                _workSheet.ExecuteSupressed(() =>
                {
                    ApplyToRange((row, column) => _workSheet.SetStyleName(row, column, value));
                    OnRangeChanged(RangeChangeType.Style);
                });
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
                _workSheet.ExecuteSupressed(() =>
                {
                    ApplyToRange((row, column) => _workSheet.SetStyle(row, column, value));
                    OnRangeChanged(RangeChangeType.Style);
                });
            }
        }

        public IRange ParentRange { get; private set; }

        public IDataMap DataMap
        {
            get
            {
                return _workSheet.GetDataMap(Row, Column);
            }
            set
            {
                ApplyToRange((row, column) => _workSheet.SetDataMap(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetCellType(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetLocked(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetRowSpan(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetColumnSpan(row, column, value));
            }
        }

        public WorkSheet WorkSheet => _workSheet;

        public bool HasSpans { get; }

        internal Cells(WorkSheet parent)
        {
            _workSheet = parent;
            Row = Column = 0;
            _rowCount = _columnCount = -1;
            _cellCollection = new Dictionary<long, Cell>();
        }

        internal Cells(Cells parentRange, int row, int column, int rowCount, int columnCount)
        {
            this.ValidateIndexes(row, column, rowCount, columnCount);
            _workSheet = parentRange._workSheet;
            ParentRange = parentRange;
            Row = row;
            Column = column;
            _rowCount = rowCount;
            _columnCount = columnCount;
            _cellCollection = parentRange._cellCollection;
            _workSheet = parentRange._workSheet;
        }

        private Cell GetCell(int row, int column)
        {
            this.ValidateIndexes(row, column, 1, 1);
            long key = CellUtils.MakeKey(row, column);

            if (_cellCollection.TryGetValue(key, out var existingCell))
            {
                existingCell.Row = row;
                existingCell.Column = column;
                return existingCell;
            }

            Cell cell = new Cell(this)
            {
                Row = row,
                Column = column
            };

            _cellCollection[key] = cell;
            return cell;
        }

        internal IEnumerable<KeyValuePair<int, object>> GetCellValues(int column)
        {
            for (int row = Row; row < Row + RowCount; row++)
            {
                var val = _workSheet.GetValue(row, column);
                if (val != null)
                    yield return new KeyValuePair<int, object>(row, val);
            }
        }

        private Cells GetRange(int row, int column, int rowCount, int columnCount)
        {
            return new Cells(this, row, column, rowCount, columnCount);
        }

        private void ApplyToRange(Action<int, int> action)
        {
            for (int row = Row; row < Row + RowCount; row++)
            {
                for (int column = Column; column < Column + ColumnCount; column++)
                {
                    action(row, column);
                }
            }
        }

        public IDataMap GetDataMap(int row, int column)
        {
            return _workSheet.GetDataMap(row, column);
        }

        public void SetDataMap(int row, int column, IDataMap dataMap)
        {
            _workSheet.SetDataMap(row, column, dataMap);
        }

        public IStyle GetStyle(int row, int column)
        {
            return _workSheet.GetStyle(row, column);
        }

        public void SetStyle(int row, int column, IStyle style)
        {
            _workSheet.SetStyle(row, column, style);
        }

        public string GetStyleName(int row, int column)
        {
            return _workSheet.GetStyleName(row, column);
        }

        public void SetStyleName(int row, int column, string styleName)
        {
            _workSheet.SetStyleName(row, column, styleName);
        }

        public object GetValue(int row, int column)
        {
            return _workSheet.GetValue(row, column);
        }

        public void SetValue(int row, int column, object value)
        {
            _workSheet.SetValue(row, column, value);
        }

        public bool HasFormula(int row, int column)
        {
            return _workSheet.HasFormula(row, column);
        }

        public string GetFormula(int row, int column)
        {
            return _workSheet.GetFormula(row, column);
        }

        public void SetFormula(int row, int column, string formula)
        {
            _workSheet.SetFormula(row, column, formula);
        }

        public IFormatter GetFormatter(int row, int column)
        {
            return _workSheet.GetFormatter(row, column);
        }

        public void SetFormatter(int row, int column, IFormatter formatter)
        {
            _workSheet.SetFormatter(row, column, formatter);
        }

        public bool GetLocked(int row, int column)
        {
            return _workSheet.GetLocked(row, column);
        }

        public void SetLocked(int row, int column, bool locked)
        {
            _workSheet.SetLocked(row, column, locked);
        }

        public ICellType GetCellType(int row, int column)
        {
            return _workSheet.GetCellType(row, column);
        }

        public void SetCellType(int row, int column, ICellType cellType)
        {
            _workSheet.SetCellType(row, column, cellType);
        }

        public int GetRowSpan(int row, int column)
        {
            return _workSheet.GetRowSpan(row, column);
        }

        public void SetRowSpan(int row, int column, int rowSpan)
        {
            _workSheet.SetRowSpan(row, column, rowSpan);
        }

        public int GetColumnSpan(int row, int column)
        {
            return _workSheet.GetColumnSpan(row, column);
        }

        public void SetColumnSpan(int row, int column, int columnSpan)
        {
            _workSheet.SetColumnSpan(row, column, columnSpan);
        }

        public void AddSpan(int row, int column, int rowCount, int columnCount)
        {
            _workSheet.AddSpan(row, column, rowCount, columnCount);
        }

        public void RemoveSpan(int row, int column)
        {
            _workSheet.RemoveSpan(row, column);
        }

        public void SetRawValue(int row, int column, string value)
        {
            _workSheet.SetRawValue(row, column, value);
        }

        public CellRange GetSpanCellRange(int row, int column)
        {
            return _workSheet.GetSpanCellRange(row, column);
        }

        public CellRange ExpandSpanRange(CellRange range)
        {
            return _workSheet.ExpandSpanRange(range);
        }

        public bool IsCovered(int row, int column)
        {
            return _workSheet.IsCovered(row, column);
        }

        public void Dispose()
        {
            _cellCollection.Clear();
        }

        private void OnRangeChanged(RangeChangeType changeType)
        {
            _workSheet.OnRangeChanged(new RangeChangedEventArgs(SheetRegion.Cells, 
                new CellRange(Row, Column, RowCount, ColumnCount),
                changeType));
        }
    }
}
