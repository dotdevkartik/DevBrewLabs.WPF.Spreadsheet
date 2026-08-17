using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Utils;
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
        private Worksheet _workSheet;
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

        public bool Locked
        {
            get { return _rowHeaders.GetLocked(Row, Column); }
            set { ApplyToRange((r, c) => _rowHeaders.SetLocked(r, c, value)); }
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

        public Worksheet WorkSheet => _workSheet;
        public RowHeaders RowHeaders => _rowHeaders;

        public bool HasSpans { get; }

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

        private RowHeaderCell GetCell(int row, int column)
        {
            this.ValidateIndexes(row, column, 1, 1);
            long key = CellUtils.MakeKey(row, column);

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

        private RowHeaderCells GetRange(int row, int column, int rowCount, int columnCount)
        {
            return new RowHeaderCells(this, row, column, rowCount, columnCount);
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
            return _rowHeaders.GetDataMap(row, column);
        }

        public void SetDataMap(int row, int column, IDataMap dataMap)
        {
            _rowHeaders.SetDataMap(row, column, dataMap);
        }

        public IStyle GetStyle(int row, int column)
        {
            return _rowHeaders.GetStyle(row, column);
        }

        public void SetStyle(int row, int column, IStyle style)
        {
            _rowHeaders.SetStyle(row, column, style);
        }

        public string GetStyleName(int row, int column)
        {
            return _rowHeaders.GetStyleName(row, column);
        }

        public void SetStyleName(int row, int column, string styleName)
        {
            _rowHeaders.SetStyleName(row, column, styleName);
        }

        public object GetValue(int row, int column)
        {
            return _rowHeaders.GetValue(row, column);
        }

        public void SetValue(int row, int column, object value)
        {
            _rowHeaders.SetValue(row, column, value);
        }

        public bool HasFormula(int row, int column)
        {
            return _rowHeaders.HasFormula(row, column);
        }

        public string GetFormula(int row, int column)
        {
            return _rowHeaders.GetFormula(row,column);
        }

        public void SetFormula(int row, int column, string formula)
        {
            _rowHeaders.SetFormula(row, column, formula);
        }

        public IFormatter GetFormatter(int row, int column)
        {
            return _rowHeaders.GetFormatter(row, column);
        }

        public void SetFormatter(int row, int column, IFormatter formatter)
        {
            _rowHeaders.SetFormatter(row, column, formatter);
        }

        public bool GetLocked(int row, int column)
        {
            return _rowHeaders.GetLocked(row, column);
        }

        public void SetLocked(int row, int column, bool locked)
        {
            _rowHeaders.SetLocked(row, column, locked);
        }

        public ICellType GetCellType(int row, int column)
        {
            return _rowHeaders.GetCellType(row, column);
        }

        public void SetCellType(int row, int column, ICellType cellType)
        {
            _rowHeaders.SetCellType(row, column, cellType);
        }

        public int GetRowSpan(int row, int column)
        {
            return _rowHeaders.GetRowSpan(row, column);
        }

        public void SetRowSpan(int row, int column, int rowSpan)
        {
            _rowHeaders.SetRowSpan(row, column, rowSpan);
        }

        public int GetColumnSpan(int row, int column)
        {
            return _rowHeaders.GetColumnSpan(row, column);
        }

        public void SetColumnSpan(int row, int column, int columnSpan)
        {
            _rowHeaders.SetColumnSpan(row, column, columnSpan);
        }

        public void AddSpan(int row, int column, int rowCount, int columnCount)
        {
            _rowHeaders.AddSpan(row, column, rowCount, columnCount);
        }

        public void RemoveSpan(int row, int column)
        {
            _rowHeaders.RemoveSpan(row, column);
        }

        public void SetRawValue(int row, int column, string value)
        {
            _rowHeaders.SetRawValue(row, column, value);
        }

        public CellRange GetSpanCellRange(int row, int column)
        {
            return _rowHeaders.GetSpanCellRange(row, column);
        }

        public CellRange ExpandSpanRange(CellRange range)
        {
            return _rowHeaders.ExpandSpanRange(range);
        }

        public bool IsCovered(int row, int column)
        {
            return _rowHeaders.IsCovered(row, column);
        }
    }
}
