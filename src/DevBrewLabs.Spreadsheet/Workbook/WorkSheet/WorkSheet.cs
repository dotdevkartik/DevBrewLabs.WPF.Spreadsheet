using DevBrewLabs.Evalis;
using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DevBrewLabs.Spreadsheet
{
    internal partial class Worksheet : IWorksheet
    {
        public event EventHandler<CellChangedEventArgs> CellChanged;
        public event EventHandler<RangeChangedEventArgs> RangeChanged;
        public event EventHandler<RowChangedEventArgs> RowChanged;
        public event EventHandler<ColumnChangedEventArgs> ColumnChanged;
        public event EventHandler<CellValueSetFailedEventArgs> CellValueSetFailed;

        private string _name;
        private Workbook _workBook;
        private Cells _cells;
        private Rows _rows;
        private Columns _columns;
        private RowHeaders _rowHeaders;
        private ColumnHeaders _columnHeaders;
        private TopLeft _topLeft;
        private WorkSheetDataStore _dataStore;
        private SpanManager _spanManager;
        private AutoFilter _autoFilter;

        /// <summary>
        /// Gets the auto filter for this worksheet.
        /// </summary>
        public AutoFilter AutoFilter 
        {
            get
            {
                if (_autoFilter == null)
                {
                    _autoFilter = new AutoFilter(this);
                    _autoFilter.FilterChanged += (s, e) => _workBook?.ChangeListener?.OnFilterChanged(e);
                }
                return _autoFilter;
            }
        }

        private bool _suspendEvents;
        private int _rowCount;
        private int _columnCount;
        private int _defaultRowHeight;
        private int _defaultColumnWidth;

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
                    ((Worksheets)_workBook.WorkSheets).VerifySheetName(value, this);
                    _name = value;
                }
            }
        }
        public int RowCount
        {
            get
            {
                return _rowCount;
            }
            set 
            {
                var oldValue = _rowCount;

                if(oldValue == value)
                {
                    return;
                }

                _rowCount = value;

                OnWorksheetChanged(new WorksheetChangedEventArgs(SheetRegion.Cells, 
                    this, 
                    oldValue,
                    value, 
                    WorksheetChangeType.RowCount));
            }
        }

        public int ColumnCount
        {
            get
            {
                return _columnCount;
            }
            set
            {
                var oldValue = _columnCount;

                if (oldValue == value)
                {
                    return;
                }

                _columnCount = value;

                OnWorksheetChanged(new WorksheetChangedEventArgs(SheetRegion.Cells,
                    this,
                    oldValue,
                    value,
                    WorksheetChangeType.ColumnCount));
            }
        }

        public int DefaultRowHeight
        {
            get
            {
                return _defaultRowHeight;
            }
            set
            {
                var oldValue = _defaultRowHeight;

                if (oldValue == value)
                {
                    return;
                }

                _defaultRowHeight = value;

                OnWorksheetChanged(new WorksheetChangedEventArgs(SheetRegion.Cells,
                    this,
                    oldValue,
                    value,
                    WorksheetChangeType.DefaultRowHeight));
            }
        }

        public int DefaultColumnWidth
        {
            get
            {
                return _defaultColumnWidth;
            }
            set
            {
                var oldValue = _defaultColumnWidth;

                if (oldValue == value)
                {
                    return;
                }

                _defaultColumnWidth = value;

                OnWorksheetChanged(new WorksheetChangedEventArgs(SheetRegion.Cells,
                    this,
                    oldValue,
                    value,
                    WorksheetChangeType.DefaultColumnWidth));
            }
        }

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
        public bool IsBound => _dataStore != null && _dataStore.IsBound;
        public bool HasSpans => _spanManager.HasSpans;
        public IRows Rows => _rows;
        public IColumns Columns => _columns;
        public IRange Cells => _cells;
        public IRowHeaders RowHeaders => _rowHeaders;
        public IColumnHeaders ColumnHeaders => _columnHeaders;
        public ITopLeft TopLeft => _topLeft;
        public IWorkbook WorkBook => _workBook;

        internal Worksheet(Workbook book, string name)
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
            _spanManager = new SpanManager();
            InitializeDataStore(null);
        }

        public IDataMap GetDataMap(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetDataMap(dataRow);
        }

        public void SetDataMap(int row, int column, IDataMap dataMap)
        {
            var existingDataMap = GetDataMap(row, column);
            if (existingDataMap == dataMap)
            {
                return;
            }
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetDataMap(dataRow, dataMap);
        }

        public IStyle GetStyle(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            ushort? styleId = colData?.GetStyleId(dataRow);

            if (!styleId.HasValue)
            {
                return null;
            }

            return _workBook.StylePalette.GetStyle(styleId.Value);
        }

        public void SetStyle(int row, int column, IStyle style)
        {
            var existingStyle = GetStyle(row, column);
            if (existingStyle == style)
            {
                return;
            }

            ushort styleId = _workBook.StylePalette.GetOrAdd(style);
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetStyleId(dataRow, styleId);

            OnCellChanged(new CellChangedEventArgs(
                    SheetRegion.Cells,
                    this,
                    row,
                    column,
                    existingStyle,
                    style,
                    CellChangeType.Style));
        }

        public string GetStyleName(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetStyleName(dataRow);
        }

        public void SetStyleName(int row, int column, string styleName)
        {
            string existingStyleName = GetStyleName(row, column);

            if (existingStyleName == styleName)
            {
                return;
            }

            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetStyleName(dataRow, styleName);

            OnCellChanged(new CellChangedEventArgs(
                    SheetRegion.Cells,
                    this,
                    row,
                    column,
                    existingStyleName,
                    styleName,
                    CellChangeType.Style));
        }

        public object GetValue(int row, int column)
        {
            return _dataStore.GetValue(row, column);
        }

        public void SetValue(int row, int column, object value)
        {
            var existingValue = GetValue(row, column);

            if (existingValue != null && existingValue.Equals(value))
            {
                return;
            }

            _dataStore.SetValue(row, column, value);

            _workBook.RaiseValueChanged(new ValueChangedEventArgs()
            {
                Row = row,
                Column = column,
                SheetName = Name,
                OldValue = existingValue,
                NewValue = value
            });

            OnCellChanged(new CellChangedEventArgs(
                SheetRegion.Cells,
                this,
                row,
                column,
                existingValue,
                value,
                CellChangeType.Value));
        }

        public bool HasFormula(int row, int column)
        {
            return !string.IsNullOrEmpty(GetFormula(row, column));
        }

        public string GetFormula(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetFormula(dataRow);
        }

        public void SetFormula(int row, int column, string formula)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            var existingFormula = colData.GetFormula(dataRow);

            if (existingFormula == formula)
            {
                return;
            }

            colData.SetFormula(dataRow, formula);
            colData.SetValue(dataRow, null);

            _workBook.RaiseFormulaChanged(new FormulaChangedEventArgs()
            {
                Row = row,
                Column = column,
                SheetName = Name,
                OldFormula = existingFormula,
                NewFormula = formula
            });

            OnCellChanged(new CellChangedEventArgs(
                  SheetRegion.Cells,
                  this,
                  row,
                  column,
                  existingFormula,
                  formula,
                  CellChangeType.Formula));
        }

        public IFormatter GetFormatter(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetFormatter(dataRow);
        }

        public void SetFormatter(int row, int column, IFormatter formatter)
        {
            IFormatter existingFormatter = GetFormatter(row, column);

            if (formatter == existingFormatter)
            {
                return;
            }

            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetFormatter(dataRow, formatter);
        }

        public bool GetLocked(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetLocked(dataRow) ?? false;
        }

        public void SetLocked(int row, int column, bool locked)
        {
            bool existingLocked = GetLocked(row, column);
            if (existingLocked == locked)
            {
                return;
            }

            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetLocked(dataRow, locked);
        }

        public ICellType GetCellType(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetCellType(dataRow);
        }

        public void SetCellType(int row, int column, ICellType cellType)
        {
            var existingCellType = GetCellType(row, column);
            if (existingCellType == cellType)
            {
                return;
            }
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetCellType(dataRow, cellType);
        }

        public int GetRowSpan(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetRowSpan(dataRow) ?? 0;
        }

        public void SetRowSpan(int row, int column, int rowSpan)
        {
            var existingRowSpan = GetRowSpan(row, column);
            if (existingRowSpan == rowSpan)
            {
                return;
            }
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetRowSpan(dataRow, rowSpan);

            if (rowSpan > 1)
            {
                int colSpan = Math.Max(1, GetColumnSpan(row, column));
                _spanManager.AddSpan(row, column, rowSpan, colSpan);
                ClearCoveredCells(row, column, rowSpan, colSpan);
            }
            else
            {
                _spanManager.RemoveSpan(row, column);
            }
        }

        public int GetColumnSpan(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetColumnSpan(dataRow) ?? 0;
        }

        public void SetColumnSpan(int row, int column, int columnSpan)
        {
            var existingColumnSpan = GetColumnSpan(row, column);
            if (existingColumnSpan == columnSpan)
            {
                return;
            }
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetColumnSpan(dataRow, columnSpan);

            if (columnSpan > 1)
            {
                int rowSpan = Math.Max(1, GetRowSpan(row, column));
                _spanManager.AddSpan(row, column, rowSpan, columnSpan);
                ClearCoveredCells(row, column, rowSpan, columnSpan);
            }
            else
            {
                _spanManager.RemoveSpan(row, column);
            }
        }

        public void AddSpan(int row, int column, int rowCount, int columnCount)
        {
            if (rowCount <= 1 && columnCount <= 1)
                return;

            _spanManager.AddSpan(row, column, rowCount, columnCount);

            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetRowSpan(dataRow, rowCount);
            colData.SetColumnSpan(dataRow, columnCount);

            ClearCoveredCells(row, column, rowCount, columnCount);

            OnRangeChanged(new RangeChangedEventArgs(
                     SheetRegion.Cells,
                     this,
                     new CellRange(row, column, rowCount, columnCount),
                     null, null,
                     RangeChangeType.Value));
        }

        public void RemoveSpan(int row, int column)
        {
            var range = _spanManager.GetSpanRange(row, column);
            if (range == default)
                return;

            _spanManager.RemoveSpan(row, column);

            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetRowSpan(dataRow, 0);
            colData.SetColumnSpan(dataRow, 0);

            OnRangeChanged(new RangeChangedEventArgs(
                     SheetRegion.Cells,
                     this,
                     range,
                     null, null,
                     RangeChangeType.Value));
        }

        public CellRange GetSpanCellRange(int row, int column)
        {
            return _spanManager.GetSpanRange(row, column);
        }

        public CellRange ExpandSpanRange(CellRange range)
        {
            return _spanManager.ExpandRange(range);
        }

        public bool IsCovered(int row, int column)
        {
            return _spanManager.IsCovered(row, column);
        }

        private void ClearCoveredCells(int row, int column, int rowCount, int colCount)
        {
            for (int r = row; r < row + rowCount; r++)
            {
                for (int c = column; c < column + colCount; c++)
                {
                    if (r == row && c == column) continue;
                    SetValue(r, c, null);
                    SetFormula(r, c, null);
                }
            }
        }

        public object GetMetadata(int row, int column)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetMetaData(dataRow);
        }

        public void SetMetadata(int row, int column, object metadata)
        {
            int dataRow = _dataStore.GetDataRowIndex(row);
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetMetaData(dataRow, metadata);
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
                    data[i, j] = _dataStore.GetValue(i + row, j + column);
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
                var cd = _dataStore.GetColumnData(colIndex, true);

                for (int r = 0; r < rows; r++)
                {
                    int rowIndex = startRow + r;
                    object val = data[r, c];

                    cd.SetValue(rowIndex, val);
                }
            }

            OnRangeChanged(new RangeChangedEventArgs(
                     SheetRegion.Cells,
                     this,
                     new CellRange(startRow, startCol, rows, cols),
                     null, null,
                      RangeChangeType.Value));
        }

        public void SetRawValue(int row, int column, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Cells[row, column].Value = null;
                return;
            }

            if (value.StartsWith("="))
            {
                Cells[row, column].Formula = value;
            }
            else
            {
                SetValue(row, column, DataTypeConverter.ConvertType(value));
            }
        }

        public void Clear(WorkSheetClearMode mode)
        {
            throw new NotImplementedException();
        }

        public bool ContainsRange(int row, int column, int rowCount, int columnCount)
        {
            return row >= 0 && column >= 0 &&
                row < RowCount && column < ColumnCount &&
                row + rowCount - 1 < RowCount &&
                column + columnCount - 1 < ColumnCount;
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

            bool isBound = IsBound;
            if (isBound)
            {
                int maxBoundRow = _dataStore.CollectionCount - 1;
                if (sortStartRow <= maxBoundRow && (sortStartRow + sortRowCount - 1) > maxBoundRow)
                {
                    sortRowCount = maxBoundRow - sortStartRow + 1;
                    if (sortRowCount <= 1) return;
                }

                // Bound sorting always reorders entire rows to preserve object integrity
                options.SortColumnOnly = false;
            }

            if (!options.SortColumnOnly)
            {
                int fullRowCount = RowCount;
                int[] rowMap = _dataStore.GetRowMap();
                if (rowMap == null || rowMap.Length != fullRowCount)
                {
                    rowMap = new int[fullRowCount];
                    for (int i = 0; i < fullRowCount; i++) rowMap[i] = i;
                }
                else
                {
                    rowMap = (int[])rowMap.Clone();
                }

                int[] subIndices = new int[sortRowCount];
                for (int i = 0; i < sortRowCount; i++)
                {
                    subIndices[i] = rowMap[sortStartRow + i];
                }

                Array.Sort(subIndices, new VirtualRowIndexComparer(options, this));

                for (int i = 0; i < sortRowCount; i++)
                {
                    rowMap[sortStartRow + i] = subIndices[i];
                }

                _dataStore.SetRowMap(rowMap);

                OnRangeChanged(new RangeChangedEventArgs(
                    SheetRegion.Cells,
                    this,
                    new CellRange(sortStartRow, 0, sortRowCount, ColumnCount),
                    null, null,
                    RangeChangeType.Sort
                ));
            }
            else
            {
                int targetStartCol = startCol;
                int targetEndCol = startCol + totalCols - 1;

                List<RowSnapshot> snapshots = new List<RowSnapshot>(sortRowCount);

                for (int r = sortStartRow; r < sortStartRow + sortRowCount; r++)
                {
                    int dataRow = _dataStore.GetDataRowIndex(r);
                    var snapshot = new RowSnapshot(r, null);

                    for (int c = targetStartCol; c <= targetEndCol; c++)
                    {
                        var colData = _dataStore.GetColumnData(c, false);
                        if (colData != null)
                        {
                            var cellData = colData.GetCellData(dataRow);
                            snapshot.Data[c] = cellData;
                        }
                    }

                    snapshots.Add(snapshot);
                }

                snapshots.Sort(new MultiLevelSnapshotComparer(options, this));

                for (int i = 0; i < snapshots.Count; i++)
                {
                    int targetVisualRow = sortStartRow + i;
                    int targetDataRow = _dataStore.GetDataRowIndex(targetVisualRow);
                    var snapshot = snapshots[i];

                    for (int c = targetStartCol; c <= targetEndCol; c++)
                    {
                        var colData = _dataStore.GetColumnData(c, true);
                        if (snapshot.Data.TryGetValue(c, out var cellData))
                        {
                            colData.SetCellData(targetDataRow, cellData);
                        }
                        else
                        {
                            colData.ClearRow(targetDataRow);
                        }
                    }
                }

                OnRangeChanged(new RangeChangedEventArgs(
                     SheetRegion.Cells,
                     this,
                    new CellRange(sortStartRow, targetStartCol, sortRowCount, targetEndCol - targetStartCol + 1),
                    null, null,
                    RangeChangeType.Sort
                ));
            }
        }

        private void InitializeDataStore(object dataSource)
        {
            if(_dataStore != null)
            {
                _dataStore.Dispose();
                _dataStore = null;
            }

            _dataStore = dataSource != null ? new WorkSheetDataStore(this, dataSource) : new WorkSheetDataStore(this);      
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
            _workBook = null;
        }

        #region events
        internal void ExecuteSupressed(Action action)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                _suspendEvents = true;
                action();
            }
            finally
            {
                _suspendEvents = false;
            }
        }

        internal void OnCellChanged(CellChangedEventArgs args)
        {
            _workBook?.ChangeListener.CellChanged(args);

            if (_autoFilter != null && _autoFilter.IsEnabled && _autoFilter.Range.ContainsCell(args.Row, args.Column))
            {
                if (_autoFilter.IsColumnFiltered(args.Column))
                {
                    _autoFilter.ReEvaluateRow(args.Row);
                }
            }

            if (_suspendEvents) return;

            CellChanged?.Invoke(this, args);
        }

        internal void OnRangeChanged(RangeChangedEventArgs args)
        {
            _workBook?.ChangeListener.RangeChanged(args);

            if (_suspendEvents) return;

            RangeChanged?.Invoke(this, args);
        }

        internal void OnRowChanged(RowChangedEventArgs args)
        {
            _workBook?.ChangeListener.RowChanged(args);

            if (_suspendEvents) return;

            RowChanged?.Invoke(this, args);
        }

        internal void OnColumnChanged(ColumnChangedEventArgs args)
        {
            _workBook?.ChangeListener.ColumnChanged(args);

            if (_suspendEvents) return;

            ColumnChanged?.Invoke(this, args);
        }

        internal void OnWorksheetChanged(WorksheetChangedEventArgs args)
        {
            _workBook?.ChangeListener.OnWorksheetChanged(args);
        }

        internal void OnCellValueSetFailed(CellValueSetFailedEventArgs args)
        {
            CellValueSetFailed?.Invoke(this, args);
        }
        #endregion

        #region worksheet datastore
        private class WorkSheetDataStore : IDisposable
        {
            private Worksheet _workSheet;
            private DataCollection _collection;
            private Dictionary<int, ColumnData> _columnStore;
            private int[] _rowMap;

            public object ActualDataSource { get; private set; }
            public bool IsValid { get; private set; }
            public bool IsBound => IsValid && ActualDataSource != null && _collection?.DataSourceType != DataSourceType.NotSupported;
            public int CollectionCount => _collection?.Count ?? 0;

            internal WorkSheetDataStore(Worksheet worksheet)
            {
                _workSheet = worksheet;
                _columnStore = new Dictionary<int, ColumnData>();
                InitializeUnboundDataStore();
            }

            internal WorkSheetDataStore(Worksheet worksheet, object dataSource) : this(worksheet)
            {
                _workSheet = worksheet;
                InitializeBoundDataStore(dataSource);
            }

            private void InitializeUnboundDataStore()
            {
                IsValid = true;
                _rowMap = null;
            }

            private void InitializeBoundDataStore(object dataSource)
            {
                IsValid = false;
                _rowMap = null;
                _collection = new DataCollection(dataSource);

                if (_collection.DataSourceType != DataSourceType.NotSupported)
                {
                    IsValid = true;
                    _workSheet.RowCount = _collection.Count;
                }

                if (IsValid)
                    ActualDataSource = dataSource;
            }

            public int GetDataRowIndex(int visualRow)
            {
                if (_rowMap != null && visualRow >= 0 && visualRow < _rowMap.Length)
                    return _rowMap[visualRow];

                return visualRow;
            }

            public int GetVisualRowIndex(int dataRow)
            {
                if (_rowMap == null) return dataRow;
                return Array.IndexOf(_rowMap, dataRow);
            }

            public int[] GetRowMap() => _rowMap;

            public void SetRowMap(int[] rowMap)
            {
                _rowMap = rowMap;
            }

            public void ResetRowMap()
            {
                _rowMap = null;
            }

            /// <summary>
            /// Gets the raw cell value for a given data row (for sorting evaluation).
            /// </summary>
            public object GetRawValueForDataRow(int dataRow, int column)
            {
                var colData = GetColumnData(column, false);
                object value = colData?.GetValue(dataRow);

                if (value != null)
                {
                    return DataTypeConverter.ConvertType(value);
                }

                if (colData?.GetFormula(dataRow) != null)
                {
                    int visualRow = GetVisualRowIndex(dataRow);
                    var result = _workSheet.WorkBook.CalcEngine.GetValue(_workSheet.Name, visualRow, column) as CalcValue;
                    return result?.Value;
                }

                if (IsValid && ActualDataSource != null && dataRow <= _collection.Count - 1)
                {
                    var sheetColumn = ((Columns)_workSheet.Columns).GetItem(column);
                    var dataMap = colData?.GetDataMap(dataRow) ?? sheetColumn?.DataMap;
                    if (dataMap != null && dataMap is PropertyDataMap propertyDataMap
                        && !string.IsNullOrEmpty(propertyDataMap.PropertyName))
                    {
                        var item = _collection.GetItemAt(dataRow);
                        var prop = _collection.GetPropertyInfo(propertyDataMap.PropertyName);
                        return DataTypeConverter.ConvertType(prop?.GetValue(item));
                    }
                    else if (dataMap != null && dataMap is DataColumnDataMap dataColumnMap
                        && !string.IsNullOrEmpty(dataColumnMap.ColumnName))
                    {
                        var item = _collection.GetItemAt(dataRow) as DataRow;
                        return DataTypeConverter.ConvertType(item?[dataColumnMap.ColumnName]);
                    }
                }

                return null;
            }

            /// <summary>
            /// Gets the cell value. If the cell has a formula, it will return the calculated value.
            /// </summary>
            /// <param name="row"></param>
            /// <param name="column"></param>
            /// <returns></returns>
            public object GetValue(int row, int column)
            {
                int dataRow = GetDataRowIndex(row);
                var colData = GetColumnData(column, false);
                object value = colData?.GetValue(dataRow);

                if (value != null)
                {
                    value = DataTypeConverter.ConvertType(value);
                    return value;
                }

                if (colData?.GetFormula(dataRow) != null)
                {
                    var result = _workSheet.WorkBook.CalcEngine.GetValue(_workSheet.Name, row, column) as CalcValue;

                    if (result != null && result.Kind == CalcValueKind.Error)
                    {
                        switch (((Error)result.Value).Code)
                        {
                            case ErrorCode.Value:
                                return "#VALUE!";

                            case ErrorCode.DivideByZero:
                                return "#DIV/0!";

                            case ErrorCode.Name:
                                return "#NAME?";

                            case ErrorCode.Null:
                                return "#NULL!";

                            case ErrorCode.Syntax:
                                return "#SYNTAX!";

                            default:
                                return "#N/A";
                        }
                    }

                    return result?.Value;
                }
                else if (IsValid && ActualDataSource != null && dataRow <= _collection.Count - 1)
                {
                    var sheetColumn = ((Columns)_workSheet.Columns).GetItem(column);
                    var dataMap = colData?.GetDataMap(dataRow) ?? sheetColumn?.DataMap;
                    if (dataMap != null && dataMap is PropertyDataMap propertyDataMap
                        && !string.IsNullOrEmpty(propertyDataMap.PropertyName))
                    {
                        var item = _collection.GetItemAt(dataRow);
                        var prop = _collection.GetPropertyInfo(propertyDataMap.PropertyName);
                        return DataTypeConverter.ConvertType(prop?.GetValue(item));
                    }
                    else if (dataMap != null && dataMap is DataColumnDataMap dataColumnMap
                        && !string.IsNullOrEmpty(dataColumnMap.ColumnName))
                    {
                        var item = _collection.GetItemAt(dataRow) as DataRow;
                        return DataTypeConverter.ConvertType(item?[dataColumnMap.ColumnName]);
                    }
                }

                return null;
            }

            /// <summary>
            /// Sets the cell value.
            /// </summary>
            /// <param name="row"></param>
            /// <param name="column"></param>
            /// <param name="value"></param>
            public void SetValue(int row, int column, object value)
            {
                int dataRow = GetDataRowIndex(row);
                var sheetColumn = _workSheet.Columns[column];
                var colData = GetColumnData(column, true);
                var dataMap = colData.GetDataMap(dataRow) ?? sheetColumn?.DataMap;

                if (_collection != null && dataRow >= _collection.Count)
                    dataMap = null;

                if (dataMap != null)
                {
                    if (dataMap is PropertyDataMap propertyDataMap)
                        SetPropertyValue(dataRow, column, propertyDataMap, value);
                    else if (dataMap is DataColumnDataMap dataColumnMap)
                        SetDataTableCellValue(dataRow, column, dataColumnMap, value);
                }
                else
                {
                    colData.SetValue(dataRow, value);
                    colData.SetFormatter(dataRow, null);
                }
            }

            public ColumnData GetColumnData(int column, bool createIfNotExists = true)
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

            /// <summary>
            /// Sets the value of cell bound to an object
            /// </summary>
            private void SetPropertyValue(int dataRow, int column, PropertyDataMap map, object value)
            {
                var item = _collection.GetItemAt(dataRow);
                var propertyInfo = _collection.GetPropertyInfo(map.PropertyName);

                if (propertyInfo.SetMethod == null)
                {
                    return;
                }

                try
                {
                    value = TryConvertType(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(item, value);
                }
                catch (Exception ex)
                {
                    int visualRow = GetVisualRowIndex(dataRow);
                    _workSheet.OnCellValueSetFailed(new CellValueSetFailedEventArgs(visualRow, column, value, ex));
                }
            }

            /// <summary>
            /// Sets the value of cell bound to DataTable.
            /// </summary>
            private void SetDataTableCellValue(int dataRow, int column, DataColumnDataMap map, object value)
            {
                var item = _collection.GetItemAt(dataRow) as DataRow;
                var type = item.Table.Columns[map.ColumnName].DataType;

                try
                {
                    value = TryConvertType(value, type);
                    item.BeginEdit();
                    item[map.ColumnName] = value;
                    item.EndEdit();
                }
                catch (Exception ex)
                {
                    int visualRow = GetVisualRowIndex(dataRow);
                    _workSheet.OnCellValueSetFailed(new CellValueSetFailedEventArgs(visualRow, column, value, ex));
                }
            }

            private object TryConvertType(object value, Type targetType)
            {
                if (value == null) return null;
                if (value.GetType() == targetType)
                {
                    return value;
                }

                return Convert.ChangeType(value, targetType);
            }

            public void Dispose()
            {
                _workSheet = null;
                _collection = null;
                ActualDataSource = null;
                _columnStore = null;
                _rowMap = null;
            }
        }
        #endregion

        #region private
        private class VirtualRowIndexComparer : IComparer<int>
        {
            private readonly SortOptions _options;
            private readonly NaturalSortComparer _defaultComparer;
            private readonly Worksheet _sheet;

            public VirtualRowIndexComparer(SortOptions options, Worksheet sheet)
            {
                _options = options;
                _sheet = sheet;
                _defaultComparer = new NaturalSortComparer(options.MatchCase);
            }

            public int Compare(int dataRowA, int dataRowB)
            {
                if (dataRowA == dataRowB) return 0;

                foreach (var level in _options.SortLevels)
                {
                    object valA = _sheet._dataStore.GetRawValueForDataRow(dataRowA, level.ColumnIndex);
                    object valB = _sheet._dataStore.GetRawValueForDataRow(dataRowB, level.ColumnIndex);

                    int result;
                    if (level.CustomComparer != null)
                    {
                        result = level.CustomComparer.Compare(valA, valB);
                    }
                    else
                    {
                        result = _defaultComparer.Compare(valA, valB);
                    }

                    if (result != 0)
                    {
                        if (NaturalSortComparer.IsBlank(valA) || NaturalSortComparer.IsBlank(valB))
                        {
                            return result;
                        }

                        return level.Ascending ? result : -result;
                    }
                }

                return dataRowA.CompareTo(dataRowB);
            }
        }

        private struct RowSnapshot
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

        private class MultiLevelSnapshotComparer : IComparer<RowSnapshot>
        {
            private readonly SortOptions _options;
            private readonly NaturalSortComparer _defaultComparer;
            private readonly Worksheet _sheet;

            public MultiLevelSnapshotComparer(SortOptions options, Worksheet sheet)
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
                        if (NaturalSortComparer.IsBlank(valX) || NaturalSortComparer.IsBlank(valY))
                        {
                            return result;
                        }

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

                return _sheet.GetValue(snapshot.OriginalRow, col);
            }
        }
        #endregion
    }
}
