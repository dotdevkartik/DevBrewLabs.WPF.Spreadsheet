using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
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
                ApplyToRange((row, column) => _workSheet.SetValue(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetFormula(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetStyleName(row, column, value));
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
                ApplyToRange((row, column) => _workSheet.SetStyle(row, column, value));
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

        public bool HasFormula => _workSheet.HasFormula(Row, Column);

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

        internal Cells(WorkSheet parent)
        {
            _workSheet = parent;
            Row = Column = 0;
            _rowCount = _columnCount = -1;
            _cellCollection = new Dictionary<long, Cell>();
        }

        internal Cells(Cells parentRange, int row, int column, int rowCount, int columnCount)
        {
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
            ValidateIndexes(row, column, 1, 1);
            long key = MakeKey(row, column);

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

        internal void Clear()
        {
            _cellCollection.Clear();
        }

        internal void ClearColumnCells(int column)
        {
            var columnCells = _cellCollection.Where(x => GetColumn(x.Key) == column).ToList();

            foreach(var cell in columnCells)
            {
                _cellCollection.Remove(cell.Key);
            }
        }

        private Cells GetRange(int row, int column, int rowCount, int columnCount)
        {
            ValidateIndexes(row, column, rowCount, columnCount);
            return new Cells(this, row, column, rowCount, columnCount);
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
            Clear();
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
