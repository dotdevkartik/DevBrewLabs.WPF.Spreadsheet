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

namespace DevBrewLabs.Spreadsheet
{
    internal partial class Worksheet : IWorksheet
    {
        public event EventHandler<CellChangedEventArgs> CellChanged;
        public event EventHandler<RangeChangedEventArgs> RangeChanged;
        public event EventHandler<RowChangedEventArgs> RowChanged;
        public event EventHandler<ColumnChangedEventArgs> ColumnChanged;

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
            _dataStore = new WorkSheetDataStore(this);
            _spanManager = new SpanManager();
        }

        public IDataMap GetDataMap(int row, int column)
        {
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetDataMap(row);
        }

        public void SetDataMap(int row, int column, IDataMap dataMap)
        {
            var existingDataMap = GetDataMap(row, column);
            if (existingDataMap == dataMap)
            {
                return;
            }
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetDataMap(row, dataMap);
        }

        public IStyle GetStyle(int row, int column)
        {
            var colData = _dataStore.GetColumnData(column, false);
            ushort? styleId = colData?.GetStyleId(row);

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

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetStyleId(row, styleId);

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
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetStyleName(row);
        }

        public void SetStyleName(int row, int column, string styleName)
        {
            string existingStyleName = GetStyleName(row, column);

            if (existingStyleName == styleName)
            {
                return;
            }

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetStyleName(row, styleName);

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

            var colData = _dataStore.GetColumnData(column, false);

            if(colData != null)
            {
                colData.SetFormula(row, null);
            }

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
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetFormula(row);
        }

        public void SetFormula(int row, int column, string formula)
        {
            var colData = _dataStore.GetColumnData(column, true);
            var existingFormula = colData.GetFormula(row);

            if (existingFormula == formula)
            {
                return;
            }

            colData.SetFormula(row, formula);
            colData.SetValue(row, null);

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
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetFormatter(row);
        }

        public void SetFormatter(int row, int column, IFormatter formatter)
        {
            IFormatter existingFormatter = GetFormatter(row, column);

            if (formatter == existingFormatter)
            {
                return;
            }

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetFormatter(row, formatter);
        }

        public bool GetLocked(int row, int column)
        {
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetLocked(row) ?? false;
        }

        public void SetLocked(int row, int column, bool locked)
        {
            bool existingLocked = GetLocked(row, column);
            if (existingLocked == locked)
            {
                return;
            }

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetLocked(row, locked);
        }

        public ICellType GetCellType(int row, int column)
        {
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetCellType(row);
        }

        public void SetCellType(int row, int column, ICellType cellType)
        {
            var existingCellType = GetCellType(row, column);
            if (existingCellType == cellType)
            {
                return;
            }
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetCellType(row, cellType);
        }

        public int GetRowSpan(int row, int column)
        {
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetRowSpan(row) ?? 0;
        }

        public void SetRowSpan(int row, int column, int rowSpan)
        {
            var existingRowSpan = GetRowSpan(row, column);
            if (existingRowSpan == rowSpan)
            {
                return;
            }
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetRowSpan(row, rowSpan);

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
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetColumnSpan(row) ?? 0;
        }

        public void SetColumnSpan(int row, int column, int columnSpan)
        {
            var existingColumnSpan = GetColumnSpan(row, column);
            if (existingColumnSpan == columnSpan)
            {
                return;
            }
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetColumnSpan(row, columnSpan);

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

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetRowSpan(row, rowCount);
            colData.SetColumnSpan(row, columnCount);

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

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetRowSpan(row, 0);
            colData.SetColumnSpan(row, 0);

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
            var colData = _dataStore.GetColumnData(column, false);
            return colData?.GetMetaData(row);
        }

        public void SetMetadata(int row, int column, object metadata)
        {
            var colData = _dataStore.GetColumnData(column, true);
            colData.SetMetaData(row, metadata);
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
                    var colData = _dataStore.GetColumnData(c, false);
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
                    var colData = _dataStore.GetColumnData(c, true);
                    if (snapshot.Data.TryGetValue(c, out var cellData))
                    {
                        colData.SetCellData(targetRow, cellData);

                        if (DataSource != null)
                            SetValue(targetRow, c, cellData.Value);
                    }
                    else
                    {
                        colData.ClearRow(targetRow);
                        if (DataSource != null)
                            SetValue(targetRow, c, null);
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

        private void InitializeDataStore(object dataSource)
        {
            if(dataSource == null && _dataStore != null)
            {
                _dataStore.Dispose();
                _dataStore = null;
                return;
            }

            if(_dataStore != null)
            {
                _dataStore.Dispose();
                _dataStore = null;
            }

            _dataStore = new WorkSheetDataStore(this, dataSource);          
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
        #endregion

        #region worksheet datastore
        private class WorkSheetDataStore : IDisposable
        {
            private Worksheet _workSheet;
            private DataCollection _collection;
            private Dictionary<int, ColumnData> _columnStore;

            public object ActualDataSource { get; private set; }
            public bool IsValid { get; private set; }

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
            }

            private void InitializeBoundDataStore(object dataSource)
            {
                IsValid = false;
                _collection = new DataCollection(dataSource);

                if (_collection.DataSourceType != DataSourceType.NotSupported)
                {
                    IsValid = true;
                    _workSheet.RowCount = _collection.Count;
                }

                if (IsValid)
                    ActualDataSource = dataSource;
            }

            /// <summary>
            /// Gets the cell value. If the cell has a formula, it will return the calculated value.
            /// </summary>
            /// <param name="row"></param>
            /// <param name="column"></param>
            /// <returns></returns>
            public object GetValue(int row, int column)
            {
                var colData = GetColumnData(column, false);
                object value = colData?.GetValue(row);

                if (value != null)
                {
                    return value;
                }

                if (colData?.GetFormula(row) != null)
                {
                    var result = _workSheet.WorkBook.CalcEngine.GetValue(_workSheet.Name, row, column) as CalcValue;

                    if (result.Kind == CalcValueKind.Error)
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

                    return result.Value;
                }
                else if (IsValid && ActualDataSource != null && row <= _collection.Count - 1)
                {
                    var sheetColumn = ((Columns)_workSheet.Columns).GetItem(column);
                    var dataMap = colData?.GetDataMap(row) ?? sheetColumn?.DataMap;
                    if (dataMap != null && dataMap is PropertyDataMap propertyDataMap
                        && !string.IsNullOrEmpty(propertyDataMap.PropertyName))
                    {
                        var item = _collection.GetItemAt(row);
                        return _collection.GetPropertyInfo(propertyDataMap.PropertyName).GetValue(item);
                    }
                    else if (dataMap != null && dataMap is DataColumnDataMap dataColumnMap
                        && !string.IsNullOrEmpty(dataColumnMap.ColumnName))
                    {
                        var item = _collection.GetItemAt(row) as DataRow;
                        return item[dataColumnMap.ColumnName];
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
                var sheetColumn = _workSheet.Columns[column];
                var dataMap = _workSheet.GetDataMap(row, column) ?? sheetColumn?.DataMap;

                if (_collection != null && row >= _collection.Count)
                    dataMap = null;

                if (dataMap != null)
                {
                    if (dataMap is PropertyDataMap propertyDataMap)
                        SetPropertyValue(row, column, propertyDataMap, value);
                    else if (dataMap is DataColumnDataMap dataColumnMap)
                        SetDataTableCellValue(row, column, dataColumnMap, value);
                }
                else
                {
                    var colData = GetColumnData(column, true);
                    colData.SetValue(row, value);
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
            /// <param name="row"></param>
            /// <param name="column"></param>
            /// <param name="map"></param>
            /// <param name="value"></param>
            private void SetPropertyValue(int row, int column, PropertyDataMap map, object value)
            {
                var item = _collection.GetItemAt(row);
                var propertyInfo = _collection.GetPropertyInfo(map.PropertyName);

                if (propertyInfo.PropertyType != value.GetType() || propertyInfo.SetMethod == null)
                    return;

                propertyInfo.SetValue(item, value);
            }

            /// <summary>
            /// Sets the value of cell bound to DataTable.
            /// </summary>
            /// <param name="row"></param>
            /// <param name="column"></param>
            /// <param name="map"></param>
            /// <param name="value"></param>
            private void SetDataTableCellValue(int row, int column, DataColumnDataMap map, object value)
            {
                var item = _collection.GetItemAt(row) as DataRow;
                var type = item.Table.Columns[map.ColumnName].DataType;

                if (type != value.GetType())
                    return;

                item.BeginEdit();
                item[map.ColumnName] = value;
                item.EndEdit();
            }

            public void Dispose()
            {
                _workSheet = null;
                _collection = null;
                ActualDataSource = null;
                _columnStore = null;
            }
        }
        #endregion

        #region private
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
