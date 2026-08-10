using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet
{
    internal class WorkSheet : IWorkSheet
    {
        public event EventHandler<CellChangedEventArgs> CellChanged;
        public event EventHandler<RangeChangedEventArgs> RangeChanged;
        public event EventHandler<RowChangedEventArgs> RowsChanged;
        public event EventHandler<ColumnChangedEventArgs> ColumnsChanged;
        private string _name;
        private WorkBook _workBook;
        private Cells _cells;
        private Rows _rows;
        private Columns _columns;
        private RowHeaders _rowHeaders;
        private ColumnHeaders _columnHeaders;
        private TopLeft _topLeft;
        private FilterProvider _filterProvider;
        private WorkSheetDataStore _dataStore;
        private Dictionary<int, ColumnData> _columnStore;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    ((WorkSheets)_workBook.WorkSheets).VerifySheetName(value);
                    _name = value;
                }
            }
        }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public int DefaultRowHeight { get; set; }
        public int DefaultColumnWidth { get; set; }
        public object DataSource
        {
            get
            {
                if (_dataStore.IsValid && _dataStore.ActualDataSource != null)
                    return _dataStore.ActualDataSource;

                return null;
            }
            set
            {
                InitializeDataStore(value);
            }
        }

        public IRows Rows => _rows;
        public IColumns Columns => _columns;
        public IRange Cells => _cells;
        public IRowHeaders RowHeaders => _rowHeaders;
        public IColumnHeaders ColumnHeaders => _columnHeaders;
        public IFilterProvider FilterProvider => _filterProvider;
        public ITopLeft TopLeft => _topLeft;
        public IDataStore DataStore => _dataStore;
        public IWorkBook WorkBook => _workBook;

        internal WorkSheet(WorkBook book, string name)
        {
            _workBook = book;
            Name = name;
            DefaultRowHeight = 22;
            DefaultColumnWidth = 70;
            _rows = new Rows(this);
            _columns = new Columns(this);
            _topLeft = new TopLeft(this);
            _rowHeaders = new RowHeaders(this);
            _columnHeaders = new ColumnHeaders(this);
            _cells = new Cells(this);
            RowCount = ColumnCount = 500;
            _dataStore = new WorkSheetDataStore(this);
            _filterProvider = new FilterProvider(this);
            _columnStore = new Dictionary<int, ColumnData>();
        }

        public void SortRange(CellRange range, SortOptions options)
        {
            SortImpl(range, options);
        }

        public void Sort(SortOptions options)
        {
            SortImpl(new CellRange(
                _cells.Row, 
                _cells.Column, 
                _cells.RowCount, 
                _cells.ColumnCount), 
                options);
        }


        public object[,] GetData(CellRange range)
        {
            return GetData(range.TopRow, range.LeftColumn, range.RowCount, range.ColumnCount);
        }

        public object[,] GetData(int row, int column, int rowCount, int columnCount)
        {
            object[,] data = new object[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    data[i, j] = DataStore.GetValue(i + row, j + column);
                }
            }
            return data;
        }

        public void Load(object[,] data, int startRow = 0, int startCol = 0)
        {
            if (data == null)
                return;

            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            if (rows == 0 || cols == 0)
                return;

            for (int c = 0; c < cols; c++)
            {
                int colIndex = startCol + c;
                var colData = GetColumnData(colIndex, true);

                for (int r = 0; r < rows; r++)
                {
                    int rowIndex = startRow + r;
                    object val = data[r, c];
                    colData.SetValue(rowIndex, val);
                }
            }

            OnRangeChanged(new RangeChangedEventArgs(
                     SheetRegion.Cells,
                     new CellRange(startRow, startCol, rows, cols),
                      RangeChangeType.Value));
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

        internal void ClearColumnCells(int column)
        {
            var colData = GetColumnData(column, false);
            colData?.Clear();
            _cells.ClearColumnCells(column);
        }

        private void SortImpl(CellRange range, SortOptions options)
        {
            int startRow = range.TopRow;
            int totalRows = range.RowCount;
            int startCol = range.LeftColumn;
            int totalCols = range.ColumnCount;

            if (options == null || options.SortLevels == null || options.SortLevels.Count == 0)
                return;

            if (totalRows <= 1)
                return;

            int sortStartRow = options.HasHeader ? startRow + 1 : startRow;
            int sortRowCount = options.HasHeader ? totalRows - 1 : totalRows;

            if (sortRowCount <= 1)
                return;

            int minCol = int.MaxValue;
            int maxCol = int.MinValue;
            foreach (var level in options.SortLevels)
            {
                if (level.ColumnIndex < minCol) minCol = level.ColumnIndex;
                if (level.ColumnIndex > maxCol) maxCol = level.ColumnIndex;
            }

            if (minCol == int.MaxValue)
            {
                minCol = startCol;
                maxCol = startCol;
            }

            int targetStartCol = options.SortColumnOnly ? minCol : startCol;
            int targetEndCol = options.SortColumnOnly ? maxCol : (startCol + totalCols - 1);

            List<RowSnapshot> snapshots = new List<RowSnapshot>(sortRowCount);

            for (int r = sortStartRow; r < sortStartRow + sortRowCount; r++)
            {
                var snapshot = new RowSnapshot(r, null);

                for (int c = targetStartCol; c <= targetEndCol; c++)
                {
                    var colData = GetColumnData(c, false);
                    if (colData != null)
                    {
                        var cellData = colData.GetCellData(r);
                        snapshot.Data[c] = cellData;
                    }
                }

                snapshots.Add(snapshot);
            }

            snapshots.Sort(new MultiLevelSnapshotComparer(options, this));

            for (int i = 0; i < snapshots.Count; i++)
            {
                int targetRow = sortStartRow + i;
                var snapshot = snapshots[i];

                for (int c = targetStartCol; c <= targetEndCol; c++)
                {
                    var colData = GetColumnData(c, true);
                    if (snapshot.Data.TryGetValue(c, out var cellData))
                    {
                        colData.SetCellData(targetRow, cellData);

                        if (DataSource != null)
                            DataStore.SetValue(targetRow, c, cellData.Value);
                    }
                    else
                    {
                        colData.ClearRow(targetRow);
                        if (DataSource != null)
                            DataStore.SetValue(targetRow, c, null);
                    }
                }
            }

            OnRangeChanged(new RangeChangedEventArgs(
                 SheetRegion.Cells,
                new CellRange(sortStartRow, targetStartCol, sortRowCount, targetEndCol - targetStartCol + 1),
                RangeChangeType.Sort
            ));
        }

        private void InitializeDataStore(object dataSource)
        {
            if(dataSource == null && DataStore != null)
            {
                _dataStore.Dispose();
                _dataStore = null;
                return;
            }

            if(DataStore != null)
            {
                _dataStore.Dispose();
                _dataStore = null;
            }

            _dataStore = new WorkSheetDataStore(this, dataSource);          
        }

        internal void OnCellChanged(CellChangedEventArgs args)
        {
            args.WorkSheet = this;
            if (_workBook.UpdateProvider != null && !_workBook.UpdateProvider.SuspendUpdates)
                _workBook.UpdateProvider.CellChanged(this, args.Row, args.Column, args.OldValue, args.NewValue, args.Region, args.ChangeType);

            CellChanged?.Invoke(this, args);
        }

        internal void OnRangeChanged(RangeChangedEventArgs args)
        {
            args.WorkSheet = this;
            if (_workBook.UpdateProvider != null && !_workBook.UpdateProvider.SuspendUpdates)
                _workBook.UpdateProvider.RangeChanged(this, args.Range, args.Region, args.ChangeType);

            RangeChanged?.Invoke(this, args);
        }

        internal void OnRowsChanged(RowChangedEventArgs args)
        {
            args.WorkSheet = this;
            if (_workBook.UpdateProvider != null && !_workBook.UpdateProvider.SuspendUpdates)
                _workBook.UpdateProvider.RowsChanged(this, args.Index, args.Count, args.Region, args.ChangeType);

            RowsChanged?.Invoke(this, args);
        }

        internal void OnColumnsChanged(ColumnChangedEventArgs args)
        {
            args.WorkSheet = this;

            if (_workBook.UpdateProvider != null && !_workBook.UpdateProvider.SuspendUpdates)
                _workBook.UpdateProvider.ColumnsChanged(this, args.Index, args.Count, args.Region, args.ChangeType);

            ColumnsChanged?.Invoke(this, args);
        }

        public void Dispose()
        {
            _dataStore.Dispose();
            _dataStore = null;
            DataSource = null;
            _rows.Dispose();
            _columns.Dispose();
            _cells.Dispose();
            _rowHeaders.Dispose();
            _columnHeaders.Dispose();
            _rows = null;
            _columns = null;
            _cells = null;
            _rowHeaders = null;
            _columnHeaders = null;
            _topLeft = null;
            _filterProvider = null;
            _workBook = null;
        }

        public void Clear(WorkSheetClearMode mode)
        {
            switch(mode)
            {
                case WorkSheetClearMode.Data:
                    foreach (var col in _columnStore.Values)
                    {
                        col.Clear();
                    }
                    _columnStore.Clear();
                    _cells.ClearCellStore();
                    break;
            }
        }

        public bool ContainsRange(int row, int column, int rowCount, int columnCount)
        {
            return row >= 0 && column >= 0 &&
                row < RowCount && column < ColumnCount &&
                row + rowCount - 1 < RowCount && 
                column + columnCount - 1 < ColumnCount;
        }


        #region private
        internal struct RowSnapshot
        {
            public int OriginalRow { get; }
            public object KeyValue { get; }
            public Dictionary<int, CellData> Data { get; }

            public RowSnapshot(int originalRow, object keyValue)
            {
                OriginalRow = originalRow;
                KeyValue = keyValue;
                Data = new Dictionary<int, CellData>();
            }
        }

        internal class MultiLevelSnapshotComparer : IComparer<RowSnapshot>
        {
            private readonly SortOptions _options;
            private readonly NaturalSortComparer _defaultComparer;
            private readonly WorkSheet _sheet;

            public MultiLevelSnapshotComparer(SortOptions options, WorkSheet sheet)
            {
                _options = options;
                _sheet = sheet;
                _defaultComparer = new NaturalSortComparer(options.MatchCase);
            }

            public int Compare(RowSnapshot x, RowSnapshot y)
            {
                foreach (var level in _options.SortLevels)
                {
                    object valX = GetValue(x, level.ColumnIndex);
                    object valY = GetValue(y, level.ColumnIndex);

                    int result;
                    if (level.CustomComparer != null)
                    {
                        result = level.CustomComparer.Compare(valX, valY);
                    }
                    else
                    {
                        result = _defaultComparer.Compare(valX, valY);
                    }

                    if (result != 0)
                    {
                        return level.Ascending ? result : -result;
                    }
                }
                return 0;
            }

            private object GetValue(RowSnapshot snapshot, int col)
            {
                if (snapshot.Data.TryGetValue(col, out var cellData))
                {
                    return cellData.Value;
                }
                
                object val = _sheet.DataStore.GetValue(snapshot.OriginalRow, col);
                if (val == null)
                    val = _sheet._cells.GetCell(snapshot.OriginalRow, col, false)?.Value;
                return val;
            }
        }
        #endregion
    }
}
