using System;
using System.Collections.Generic;
using System.Linq;
using DevBrewLabs.Spreadsheet.Sorting;

namespace DevBrewLabs.Spreadsheet.Filtering
{
    /// <summary>
    /// Central filter manager for a Worksheet.
    /// </summary>
    public sealed class AutoFilter
    {
        private readonly IWorksheet _worksheet;
        private CellRange _range = default;
        private readonly Dictionary<int, ColumnFilter> _columnFilters = new Dictionary<int, ColumnFilter>();

        internal AutoFilter(IWorksheet worksheet)
        {
            _worksheet = worksheet;
        }

        /// <summary>
        /// Gets the AutoFilter range. Row 0 of the range is the header row; remaining rows are data rows.
        /// </summary>
        public CellRange Range => _range;

        /// <summary>
        /// Gets whether the AutoFilter range has been set.
        /// </summary>
        public bool IsEnabled => _range != default;

        /// <summary>
        /// Gets whether any column has an active filter.
        /// </summary>
        public bool IsFiltered => _columnFilters.Values.Any(f => f.IsFiltered);

        /// <summary>
        /// Gets the column indices that have active filters.
        /// </summary>
        public IReadOnlyList<int> FilteredColumns => _columnFilters.Values.Where(f => f.IsFiltered).Select(f => f.ColumnIndex).ToList();

        /// <summary>
        /// Sets the AutoFilter range. The first row is the header row; remaining rows are data rows.
        /// Only one range per worksheet.
        /// </summary>
        public void SetRange(CellRange range)
        {
            _range = range;
            ClearAll(); // Setting a new range clears existing filters
        }

        /// <summary>
        /// Sets a filter condition on the specified column. Replaces any existing condition on that column.
        /// Automatically calls Apply().
        /// </summary>
        public void SetFilter(int columnIndex, IFilterCondition condition)
        {
            if (!IsEnabled) return;
            if (condition == null)
            {
                ClearFilter(columnIndex);
                return;
            }

            if (!_columnFilters.TryGetValue(columnIndex, out var colFilter))
            {
                colFilter = new ColumnFilter(columnIndex);
                _columnFilters[columnIndex] = colFilter;
            }

            colFilter.Conditions.Clear();
            colFilter.Conditions.Add(condition);
            
            Apply();
        }

        /// <summary>
        /// Clears the filter on a specific column and re-applies remaining filters.
        /// </summary>
        public void ClearFilter(int columnIndex)
        {
            if (_columnFilters.ContainsKey(columnIndex))
            {
                _columnFilters.Remove(columnIndex);
                Apply();
            }
        }

        /// <summary>
        /// Clears all filters and restores all row visibility.
        /// </summary>
        public void ClearAll()
        {
            if (!IsEnabled) return;

            _columnFilters.Clear();
            
            var rowsCol = (DevBrewLabs.Spreadsheet.Rows)_worksheet.Rows;
            // Restore all rows in range
            for (int r = _range.TopRow + 1; r <= _range.BottomRow; r++)
            {
                var row = rowsCol.GetItem(r) as Row;
                if (row != null && row.IsFilteredOut)
                {
                    row.IsFilteredOut = false;
                }
            }

            RaiseFilterChanged(null);
        }

        /// <summary>
        /// Evaluates all active column filters and updates Row.IsFilteredOut for data rows.
        /// </summary>
        public void Apply()
        {
            if (!IsEnabled) return;

            for (int r = _range.TopRow + 1; r <= _range.BottomRow; r++)
            {
                bool rowVisible = true;

                foreach (var kvp in _columnFilters)
                {
                    var colIndex = kvp.Key;
                    var colFilter = kvp.Value;

                    if (colFilter.IsFiltered)
                    {
                        var cellValue = _worksheet.GetValue(r, colIndex);
                        var context = new FilterContext(_worksheet, r, colIndex, cellValue);

                        if (!colFilter.MatchRow(context))
                        {
                            rowVisible = false;
                            break;
                        }
                    }
                }

                var rowsCol = (Rows)_worksheet.Rows;
                var rowObj = rowsCol.GetItem(r) as Row;
                if (!rowVisible)
                {
                    if (rowObj == null) rowObj = _worksheet.Rows[r] as Row;
                    if (!rowObj.IsFilteredOut) rowObj.IsFilteredOut = true;
                }
                else if (rowObj != null)
                {
                    if (rowObj.IsFilteredOut) rowObj.IsFilteredOut = false;
                }
            }

            RaiseFilterChanged(null); // Passing null as it affects potentially multiple/general
        }
        
        /// <summary>
        /// Re-evaluates a single row. Used when a cell value changes.
        /// </summary>
        internal void ReEvaluateRow(int row)
        {
            if (!IsEnabled || row <= _range.TopRow || row > _range.BottomRow) return;

            bool rowVisible = true;

            foreach (var kvp in _columnFilters)
            {
                var colIndex = kvp.Key;
                var colFilter = kvp.Value;

                if (colFilter.IsFiltered)
                {
                    var cellValue = _worksheet.GetValue(row, colIndex);
                    var context = new FilterContext(_worksheet, row, colIndex, cellValue);

                    if (!colFilter.MatchRow(context))
                    {
                        rowVisible = false;
                        break;
                    }
                }
            }

            var rowsCol = (Rows)_worksheet.Rows;
            var rowObj = rowsCol.GetItem(row) as Row;
            bool changed = false;
            if (!rowVisible)
            {
                if (rowObj == null) rowObj = _worksheet.Rows[row] as Row;
                if (!rowObj.IsFilteredOut) { rowObj.IsFilteredOut = true; changed = true; }
            }
            else if (rowObj != null)
            {
                if (rowObj.IsFilteredOut) { rowObj.IsFilteredOut = false; changed = true; }
            }
            if (changed)
            {
                RaiseFilterChanged(null); // Notify that a row's visibility changed
            }
        }

        /// <summary>
        /// Gets unique values for the specified column, respecting all OTHER active column filters.
        /// The current column's own filter is ignored when generating its values.
        /// </summary>
        public IReadOnlyList<object> GetAvailableValues(int columnIndex)
        {
            var values = new HashSet<object>();
            if (!IsEnabled) return values.ToList();

            for (int r = _range.TopRow + 1; r <= _range.BottomRow; r++)
            {
                bool rowVisible = true;

                foreach (var kvp in _columnFilters)
                {
                    var otherColIndex = kvp.Key;
                    var colFilter = kvp.Value;

                    // Ignore current column's filter
                    if (otherColIndex == columnIndex) continue;

                    if (colFilter.IsFiltered)
                    {
                        var cellValue = _worksheet.GetValue(r, otherColIndex);
                        var context = new FilterContext(_worksheet, r, otherColIndex, cellValue);

                        if (!colFilter.MatchRow(context))
                        {
                            rowVisible = false;
                            break;
                        }
                    }
                }

                if (rowVisible)
                {
                    var val = _worksheet.GetValue(r, columnIndex);
                    values.Add(val);
                }
            }

            return values.ToList();
        }

        /// <summary>
        /// Returns true if the given cell is a filter header cell (first row of range, within range columns).
        /// Used by CellRenderer to decide whether to draw a filter icon.
        /// </summary>
        public bool IsFilterHeaderCell(int row, int column)
        {
            if (!IsEnabled) return false;
            return row == _range.TopRow && column >= _range.LeftColumn && column <= _range.RightColumn;
        }

        /// <summary>
        /// Returns true if the specified column has an active filter.
        /// </summary>
        public bool IsColumnFiltered(int columnIndex)
        {
            return _columnFilters.TryGetValue(columnIndex, out var filter) && filter.IsFiltered;
        }

        public ColumnFilter GetColumnFilter(int columnIndex)
        {
            if (_columnFilters.TryGetValue(columnIndex, out var filter))
                return filter;
            return null;
        }

        public event EventHandler<FilterChangedEventArgs> FilterChanged;

        private void RaiseFilterChanged(int? columnIndex)
        {
            if (FilterChanged != null)
            {
                int totalRows = IsEnabled ? _range.RowCount - 1 : 0;
                int visibleRows = 0;
                
                if (IsEnabled)
                {
                    var rowsCol = (DevBrewLabs.Spreadsheet.Rows)_worksheet.Rows;
                    for (int r = _range.TopRow + 1; r <= _range.BottomRow; r++)
                    {
                        var rowObj = rowsCol.GetItem(r) as Row;
                        if (rowObj == null || !rowObj.IsFilteredOut)
                        {
                            visibleRows++;
                        }
                    }
                }

                FilterChanged.Invoke(this, new FilterChangedEventArgs(_worksheet, columnIndex, visibleRows, totalRows));
            }
        }

        public void SortColumn(int column, bool ascending)
        {
            if (!IsEnabled) return;
            
            var options = new SortOptions
            {
                HasHeader = true, // AutoFilter always has a header row
                SortLevels = new List<SortInfo>
                {
                    new SortInfo(column, ascending)
                }
            };
            
            _worksheet.SortRange(_range, options);
            Apply(); // Re-apply filter after sorting in case rows moved around
        }
    }
}


