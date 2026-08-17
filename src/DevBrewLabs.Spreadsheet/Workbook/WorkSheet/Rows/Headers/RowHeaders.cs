using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Formatters;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet
{
    internal class RowHeaders : HeadersBase, IRowHeaders
    {
        private RowHeaderCells _cells;
        private RowHeaderColumns _columns;
        private Dictionary<int, ColumnData> _columnStore;

        public int ColumnCount { get; set; }
        public int DefaultColumnWidth { get; set; }
        public IRange Cells => _cells;
        public IColumns Columns => _columns;

        public bool HasSpans { get; }

        internal RowHeaders(Worksheet workSheet) : base(workSheet)
        {
            DefaultColumnWidth = 30;
            ColumnCount = 1;
            _cells = new RowHeaderCells(this);
            _columns = new RowHeaderColumns(this);
            _columnStore = new Dictionary<int, ColumnData>();
        }

        internal ColumnData GetColumnData(int column, bool createIfNotExists = true)
        {
            if (_columnStore.TryGetValue(column, out var colData))
                return colData;

            if (createIfNotExists)
            {
                colData = new ColumnData(column);
                _columnStore[column] = colData;
                return colData;
            }

            return null;
        }

        internal void ClearColumnData(int column)
        {
            var colData = GetColumnData(column, false);
            colData?.Clear();
        }

        public IDataMap GetDataMap(int row, int column)
        {
            return GetColumnData(column, false)?.GetDataMap(row);
        }

        public void SetDataMap(int row, int column, IDataMap dataMap)
        {
            var existingDataMap = GetDataMap(row, column);
            if (existingDataMap == dataMap) return;
            GetColumnData(column, true).SetDataMap(row, dataMap);
        }

        public IStyle GetStyle(int row, int column)
        {
            ushort? styleId = GetColumnData(column, false)?.GetStyleId(row);
            if (!styleId.HasValue) return null;
            return WorkSheet.WorkBook.StylePalette.GetStyle(styleId.Value);
        }

        public void SetStyle(int row, int column, IStyle style)
        {
            if (GetStyle(row, column) == style) return;
            ushort styleId = WorkSheet.WorkBook.StylePalette.GetOrAdd(style);
            GetColumnData(column, true).SetStyleId(row, styleId);

            WorkSheet.OnCellChanged(new CellChangedEventArgs(
                   SheetRegion.RowHeader,
                   WorkSheet,
                   row,
                   column,
                   null,
                   null,
                   CellChangeType.Style));
        }

        public string GetStyleName(int row, int column)
        {
            return GetColumnData(column, false)?.GetStyleName(row);
        }

        public void SetStyleName(int row, int column, string styleName)
        {
            if (GetStyleName(row, column) == styleName) return;
            GetColumnData(column, true).SetStyleName(row, styleName);

            WorkSheet.OnCellChanged(new CellChangedEventArgs(
                   SheetRegion.RowHeader,
                   WorkSheet,
                   row,
                   column,
                   null,
                   null,
                   CellChangeType.StyleName));
        }

        public object GetValue(int row, int column)
        {
            return GetColumnData(column, false)?.GetValue(row);
        }

        public void SetValue(int row, int column, object value)
        {
            if (GetValue(row, column) == value) return;
            GetColumnData(column, true).SetValue(row, value);

            WorkSheet.OnCellChanged(new CellChangedEventArgs(
                   SheetRegion.RowHeader,
                   WorkSheet,
                   row,
                   column,
                   null,
                   null,
                   CellChangeType.Value));
        }

        public bool HasFormula(int row, int column)
        {
            return !string.IsNullOrEmpty(GetFormula(row, column));
        }

        public string GetFormula(int row, int column)
        {
            return GetColumnData(column, false)?.GetFormula(row);
        }

        public void SetFormula(int row, int column, string formula)
        {
            if (GetFormula(row, column) == formula) return;
            GetColumnData(column, true).SetFormula(row, formula);

            WorkSheet.OnCellChanged(new CellChangedEventArgs(
                   SheetRegion.RowHeader,
                   WorkSheet,
                   row,
                   column,
                   null,
                   null,
                   CellChangeType.Formula));

        }

        public IFormatter GetFormatter(int row, int column)
        {
            return GetColumnData(column, false)?.GetFormatter(row);
        }

        public void SetFormatter(int row, int column, IFormatter formatter)
        {
            if (GetFormatter(row, column) == formatter) return;
            GetColumnData(column, true).SetFormatter(row, formatter);
        }

        public bool GetLocked(int row, int column)
        {
            return GetColumnData(column, false)?.GetLocked(row) ?? false;
        }

        public void SetLocked(int row, int column, bool locked)
        {
            if (GetLocked(row, column) == locked) return;
            GetColumnData(column, true).SetLocked(row, locked);
        }

        public ICellType GetCellType(int row, int column)
        {
            return GetColumnData(column, false)?.GetCellType(row);
        }

        public void SetCellType(int row, int column, ICellType cellType)
        {
            if (GetCellType(row, column) == cellType) return;
            GetColumnData(column, true).SetCellType(row, cellType);
        }

        public int GetRowSpan(int row, int column)
        {
            return GetColumnData(column, false)?.GetRowSpan(row) ?? 0;
        }

        public void SetRowSpan(int row, int column, int rowSpan)
        {
            if (GetRowSpan(row, column) == rowSpan) return;
            GetColumnData(column, true).SetRowSpan(row, rowSpan);
        }

        public int GetColumnSpan(int row, int column)
        {
            return GetColumnData(column, false)?.GetColumnSpan(row) ?? 0;
        }

        public void SetColumnSpan(int row, int column, int columnSpan)
        {
            if (GetColumnSpan(row, column) == columnSpan) return;
            GetColumnData(column, true).SetColumnSpan(row, columnSpan);
        }

        public void AddSpan(int row, int column, int rowCount, int columnCount)
        {
            throw new NotImplementedException();
        }

        public void RemoveSpan(int row, int column)
        {
            throw new NotImplementedException();
        }

        public void SetRawValue(int row, int column, string value)
        {
            throw new NotImplementedException();
        }

        public CellRange GetSpanCellRange(int row, int column)
        {
            throw new NotImplementedException();
        }

        public CellRange ExpandSpanRange(CellRange range)
        {
            throw new NotImplementedException();
        }

        public bool IsCovered(int row, int column)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            base.Dispose();
            _cells.Dispose();
            _columns.Dispose();
            foreach (var col in _columnStore.Values)
            {
                col.Clear();
            }
            _columnStore.Clear();
            _columnStore = null;
        }

    }
}
