using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevBrewLabs.Spreadsheet
{
    internal class ColumnHeaderCells : IRange, IDisposable
    {
        private int _rowCount;
        private int _columnCount;
        private WorkSheet _workSheet;
        private ColumnHeaders _columnHeaders;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Dictionary<long, ColumnHeaderCell> _activeCellInstances;

        public IRange this[string name]
        {
            get
            {
                throw new NotSupportedException();
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
                    return _columnHeaders.RowCount;

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
            get { return _columnHeaders.GetValue(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetValue(r, c, value)); }
        }

        public string Formula
        {
            get { return _columnHeaders.GetFormula(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetFormula(r, c, value)); }
        }

        public IFormatter Formatter
        {
            get { return _columnHeaders.GetFormatter(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetFormatter(r, c, value)); }
        }

        public string StyleName
        {
            get { return _columnHeaders.GetStyleName(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetStyleName(r, c, value)); }
        }

        public IStyle Style
        {
            get { return _columnHeaders.GetStyle(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetStyle(r, c, value)); }
        }

        public IRange ParentRange { get; private set; }

        public IDataMap DataMap
        {
            get { return _columnHeaders.GetDataMap(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetDataMap(r, c, value)); }
        }

        public ICellType CellType
        {
            get { return _columnHeaders.GetCellType(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetCellType(r, c, value)); }
        }

        public bool Locked
        {
            get { return _columnHeaders.GetLocked(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetLocked(r, c, value)); }
        }

        public int RowSpan
        {
            get { return _columnHeaders.GetRowSpan(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetRowSpan(r, c, value)); }
        }

        public int ColumnSpan
        {
            get { return _columnHeaders.GetColumnSpan(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetColumnSpan(r, c, value)); }
        }

        public WorkSheet WorkSheet => _workSheet;
        public ColumnHeaders ColumnHeaders => _columnHeaders;

        public bool HasSpans { get; }

        internal ColumnHeaderCells(ColumnHeaders parent)
        {
            _columnHeaders = parent;
            _workSheet = parent.WorkSheet;
            Row = Column = 0;
            _rowCount = _columnCount = -1;
            _activeCellInstances = new Dictionary<long, ColumnHeaderCell>();
        }

        internal ColumnHeaderCells(ColumnHeaderCells parentRange, int row, int column, int rowCount, int columnCount)
        {
            this.ValidateIndexes(row, column, rowCount, columnCount);
            _workSheet = parentRange._workSheet;
            ParentRange = parentRange;
            Row = row;
            Column = column;
            _rowCount = rowCount;
            _columnCount = columnCount;
            _activeCellInstances = parentRange._activeCellInstances;
            _workSheet = parentRange._workSheet;
        }

        private ColumnHeaderCell GetCell(int row, int column)
        {
            this.ValidateIndexes(row, column, 1, 1);
            long key = CellUtils.MakeKey(row, column);

            if (_activeCellInstances.TryGetValue(key, out var existingCell))
            {
                existingCell.Row = row;
                existingCell.Column = column;
                return existingCell;
            }

            ColumnHeaderCell cell = new ColumnHeaderCell(this)
            {
                Row = row,
                Column = column
            };

            _activeCellInstances[key] = cell;
            return cell;
        }

        private ColumnHeaderCells GetRange(int row, int column, int rowCount, int columnCount)
        {
            return new ColumnHeaderCells(this, row, column, rowCount, columnCount);
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

        public void Dispose()
        {
            _activeCellInstances.Clear();
        }

        public IDataMap GetDataMap(int row, int column)
        {
            return _columnHeaders.GetDataMap(row, column);
        }

        public void SetDataMap(int row, int column, IDataMap dataMap)
        {
            _columnHeaders.SetDataMap(row, column, dataMap);
        }

        public IStyle GetStyle(int row, int column)
        {
            return _columnHeaders.GetStyle(row, column);
        }

        public void SetStyle(int row, int column, IStyle style)
        {
            _columnHeaders.SetStyle(row, column, style);
        }

        public string GetStyleName(int row, int column)
        {
            return _columnHeaders.GetStyleName(row, column);
        }

        public void SetStyleName(int row, int column, string styleName)
        {
            _columnHeaders.SetStyleName(row, column, styleName);
        }

        public object GetValue(int row, int column)
        {
            return _columnHeaders.GetValue(row, column);
        }

        public void SetValue(int row, int column, object value)
        {
            _columnHeaders.SetValue(row, column, value);
        }

        public bool HasFormula(int row, int column)
        {
            return _columnHeaders.HasFormula(row, column);
        }

        public string GetFormula(int row, int column)
        {
            return _columnHeaders.GetFormula(row, column);
        }

        public void SetFormula(int row, int column, string formula)
        {
            _columnHeaders.SetFormula(row, column, formula);
        }

        public IFormatter GetFormatter(int row, int column)
        {
             return _columnHeaders.GetFormatter(row, column);
        }

        public void SetFormatter(int row, int column, IFormatter formatter)
        {
            _columnHeaders.SetFormatter(row, column, formatter);
        }

        public bool GetLocked(int row, int column)
        {
            return _columnHeaders.GetLocked(row, column);
        }

        public void SetLocked(int row, int column, bool locked)
        {
            _columnHeaders.SetLocked(row, column, locked);
        }

        public ICellType GetCellType(int row, int column)
        {
            return _columnHeaders.GetCellType(row, column);
        }

        public void SetCellType(int row, int column, ICellType cellType)
        {
            _columnHeaders.SetCellType(row, column, cellType);
        }

        public int GetRowSpan(int row, int column)
        {
            return _columnHeaders.GetRowSpan(row, column);
        }

        public void SetRowSpan(int row, int column, int rowSpan)
        {
            _columnHeaders.SetRowSpan(row, column, rowSpan);
        }

        public int GetColumnSpan(int row, int column)
        {
            return _columnHeaders.GetColumnSpan(row, column);
        }

        public void SetColumnSpan(int row, int column, int columnSpan)
        {
            _columnHeaders.SetColumnSpan(row, column, columnSpan);
        }

        public void AddSpan(int row, int column, int rowCount, int columnCount)
        {
            _columnHeaders.AddSpan(row, column, rowCount, columnCount);
        }

        public void RemoveSpan(int row, int column)
        {
            _columnHeaders.RemoveSpan(row, column);
        }

        public void SetRawValue(int row, int column, string value)
        {
            _columnHeaders.SetRawValue(row, column, value);
        }

        public CellRange GetSpanCellRange(int row, int column)
        {
            return _columnHeaders.GetSpanCellRange(row, column);
        }

        public CellRange ExpandSpanRange(CellRange range)
        {
            return _columnHeaders.ExpandSpanRange(range);
        }

        public bool IsCovered(int row, int column)
        {
            return _columnHeaders.IsCovered(row, column);
        }
    }
}
