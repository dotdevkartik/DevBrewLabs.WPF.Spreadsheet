using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        public IRange this[int row, int column]
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

        public bool HasFormula => _columnHeaders.HasFormula(Row, Column);

        public bool Locked
        {
            get { return _columnHeaders.GetLocked(Row, Column); }
            set { ApplyToRange((r, c) => _columnHeaders.SetLocked(r, c, value)); }
        }

        public bool IsVisible
        {
            get { return GetCell(Row, Column)?.IsVisible ?? true; }
            internal set { ApplyToRange((r, c) => ((ColumnHeaderCell)GetCell(r, c)).IsVisible = value); }
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
            ValidateIndexes(row, column, 1, 1);
            long key = MakeKey(row, column);

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

        internal void ClearCellStore()
        {
            _activeCellInstances.Clear();
        }

        private ColumnHeaderCells GetRange(int row, int column, int rowCount, int columnCount)
        {
            ValidateIndexes(row, column, rowCount, columnCount);
            return new ColumnHeaderCells(this, row, column, rowCount, columnCount);
        }

        private void ValidateIndexes(int row, int column, int rowCount, int columnCount)
        {
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
            ClearCellStore();
        }

        private static long MakeKey(int row, int column)
        {
            return ((long)row << 32) | (uint)column;
        }

        private static int GetRow(long key)
        {
            return (int)(key >> 32);
        }

        private static int GetColumn(long key)
        {
            return (int)key;
        }
    }
}
