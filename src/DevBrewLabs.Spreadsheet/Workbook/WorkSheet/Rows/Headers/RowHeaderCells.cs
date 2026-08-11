using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal class RowHeaderCells : IRange, IDisposable
    {
        private int _rowCount;
        private int _columnCount;
        private WorkSheet _workSheet;
        private RowHeaders _rowHeaders;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Dictionary<long, RowHeaderCell> _activeCellInstances;

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
                    return _workSheet.RowCount;

                return _rowCount;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (_columnCount == -1)
                    return _rowHeaders.ColumnCount;

                return _columnCount;
            }
        }

        public object Value
        {
            get { return _rowHeaders.GetValue(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetValue(r, c, value)); }
        }

        public string Formula
        {
            get { return _rowHeaders.GetFormula(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetFormula(r, c, value)); }
        }

        public IFormatter Formatter
        {
            get { return _rowHeaders.GetFormatter(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetFormatter(r, c, value)); }
        }

        public string StyleName
        {
            get { return _rowHeaders.GetStyleName(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetStyleName(r, c, value)); }
        }

        public IStyle Style
        {
            get { return _rowHeaders.GetStyle(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetStyle(r, c, value)); }
        }

        public IRange ParentRange { get; private set; }

        public IDataMap DataMap
        {
            get { return _rowHeaders.GetDataMap(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetDataMap(r, c, value)); }
        }

        public ICellType CellType
        {
            get { return _rowHeaders.GetCellType(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetCellType(r, c, value)); }
        }

        public bool HasFormula => _rowHeaders.HasFormula(Row, Column);

        public bool Locked
        {
            get { return _rowHeaders.GetLocked(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetLocked(r, c, value)); }
        }

        public bool IsVisible
        {
            get { return GetCell(Row, Column)?.IsVisible ?? true; }
            internal set { ApplyToRange((r, c) => ((RowHeaderCell)GetCell(r, c)).IsVisible = value); }
        }

        public int RowSpan
        {
            get { return _rowHeaders.GetRowSpan(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetRowSpan(r, c, value)); }
        }

        public int ColumnSpan
        {
            get { return _rowHeaders.GetColumnSpan(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetColumnSpan(r, c, value)); }
        }

        public WorkSheet WorkSheet => _workSheet;
        public RowHeaders RowHeaders => _rowHeaders;

        internal RowHeaderCells(RowHeaders parent)
        {
            _rowHeaders = parent;
            _workSheet = parent.WorkSheet;
            Row = Column = 0;
            _rowCount = _columnCount = -1;
            _activeCellInstances = new Dictionary<long, RowHeaderCell>();
        }

        internal RowHeaderCells(RowHeaderCells parentRange, int row, int column, int rowCount, int columnCount)
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

        private RowHeaderCell GetCell(int row, int column)
        {
            ValidateIndexes(row, column, 1, 1);
            long key = MakeKey(row, column);

            if (_activeCellInstances.TryGetValue(key, out var existingCell))
            {
                existingCell.Row = row;
                existingCell.Column = column;
                return existingCell;
            }

            var cell = new RowHeaderCell(this)
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

        private RowHeaderCells GetRange(int row, int column, int rowCount, int columnCount)
        {
            ValidateIndexes(row, column, rowCount, columnCount);
            return new RowHeaderCells(this, row, column, rowCount, columnCount);
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
