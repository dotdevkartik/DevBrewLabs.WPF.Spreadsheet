using DevBrewLabs.Evalis;
using DevBrewLabs.Spreadsheet.CalcEngine;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;
using System.Data;

namespace DevBrewLabs.Spreadsheet
{
    internal partial class WorkSheet : IWorkSheet
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
        private WorkSheetDataStore _dataStore;

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
                    ((WorkSheets)_workBook.WorkSheets).VerifySheetName(value, this);
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
        public ITopLeft TopLeft => _topLeft;
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

            if (existingValue == value)
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
            var existingFormula = GetFormula(row, column);

            if (existingFormula == formula)
            {
                return;
            }

            var colData = _dataStore.GetColumnData(column, true);
            colData.SetFormula(row, formula);

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
                     new CellRange(startRow, startCol, rows, cols),
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
                Cells[row, column].Value = DataTypeConverter.ConvertType(value);
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
                new CellRange(sortStartRow, targetStartCol, sortRowCount, targetEndCol - targetStartCol + 1),
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
                
                return _sheet.GetValue(snapshot.OriginalRow, col);
            }
        }
        #endregion

        #region events
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
        #endregion

        #region worksheet datastore
        private class WorkSheetDataStore : IDisposable
        {
            private WorkSheet _workSheet;
            private DataCollection _collection;
            private Dictionary<int, ColumnData> _columnStore;

            public object ActualDataSource { get; private set; }
            public bool IsValid { get; private set; }

            internal WorkSheetDataStore(WorkSheet worksheet)
            {
                _workSheet = worksheet;
                _columnStore = new Dictionary<int, ColumnData>();
                InitializeUnboundDataStore();
            }

            internal WorkSheetDataStore(WorkSheet worksheet, object dataSource) : this(worksheet)
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
    }
}
