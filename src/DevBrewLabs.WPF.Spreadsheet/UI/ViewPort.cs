using DevBrewLabs.Spreadsheet;
using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI
{
    internal class ViewPort : IViewPort
    {
        private Rect _actualBounds;
        private SheetView _sheetView;
        private WorkSheet _workSheet;
        private Rows _rows;
        private Columns _columns;
        private CellRange _viewRange;
        private LocationCache<IRow> _rowLocCache;
        private LocationCache<IColumn> _colLocCache;
        private LocationCache<IRow> _headerRowLocCache;
        private LocationCache<IColumn> _headerColLocCache;

        public double TopRowLocation { get; private set; }
        public double LeftColumnLocation { get; private set; }
        public CellRange ViewRange => _viewRange;
        public bool IsEmpty => GetIsEmpty();
        public Rect ActualBounds => _actualBounds;

        internal ViewPort(SheetView sheetView)
        {
            _sheetView = sheetView;
            _workSheet = (WorkSheet)sheetView.WorkSheet;
            _rows = _workSheet.Rows.As<Rows>();
            _columns = _workSheet.Columns.As<Columns>();
            _viewRange = new CellRange(0, 0, 0, 0);

            _rowLocCache = new LocationCache<IRow>(
                () => _workSheet.RowCount,
                () => _workSheet.DefaultRowHeight,
                _rows,
                row => row.Height);

            _colLocCache = new LocationCache<IColumn>(
                () => _workSheet.ColumnCount,
                () => _workSheet.DefaultColumnWidth,
                _columns,
                column => column.Width);

            _headerRowLocCache = new LocationCache<IRow>(
                () => _workSheet.ColumnHeaders.RowCount,
                () => _workSheet.ColumnHeaders.DefaultRowHeight,
                _workSheet.ColumnHeaders.Rows as ColumnHeaderRows,
                row => row.Height);

            _headerColLocCache = new LocationCache<IColumn>(
                () => _workSheet.RowHeaders.ColumnCount,
                () => _workSheet.RowHeaders.DefaultColumnWidth,
                _workSheet.RowHeaders.Columns as RowHeaderColumns,
                column => column.Width);
        }

        /// <summary>
        /// Get row location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal double GetRowLocation(int index)
        {
            return _rowLocCache.GetLocation(index);
        }

        /// <summary>
        /// Update row location in cache
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="offset"></param>
        internal void UpdateRowLocation(int fromIndex, double offset)
        {
            _rowLocCache.UpdateLocation(fromIndex, offset);
        }

        /// <summary>
        /// Get column location
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal double GetColumnLocation(int index)
        {
            return _colLocCache.GetLocation(index);
        }

        /// <summary>
        /// Update column location in cache.
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="offset"></param>
        internal void UpdateColumnLocation(int fromIndex, double offset)
        {
            _colLocCache.UpdateLocation(fromIndex, offset);
        }

        /// <summary>
        /// Get column header row location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal double GetHeaderRowLocation(int index)
        {
            return _headerRowLocCache.GetLocation(index);
        }

        /// <summary>
        /// Update column header row location.
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="offset"></param>
        internal void UpdateHeaderRowLocation(int fromIndex, double offset)
        {
            _headerRowLocCache.UpdateLocation(fromIndex, offset);
        }

        /// <summary>
        /// Get row header column location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal double GetHeaderColumnLocation(int index)
        {
            return _headerColLocCache.GetLocation(index);
        }

        /// <summary>
        /// Update row header column location.
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="offset"></param>
        internal void UpdateHeaderColumnLocation(int fromIndex, double offset)
        {
            _headerColLocCache.UpdateLocation(fromIndex, offset);
        }


        internal CellRange ShrinkRangeToViewPort(CellRange range)
        {
            int topRow = range.TopRow < ViewRange.TopRow ? ViewRange.TopRow : range.TopRow;
            int bottomRow = range.BottomRow > ViewRange.BottomRow ? ViewRange.BottomRow : range.BottomRow;
            int leftColumn = range.LeftColumn < ViewRange.LeftColumn ? ViewRange.LeftColumn : range.LeftColumn;
            int rightColumn = range.RightColumn > ViewRange.RightColumn ? ViewRange.RightColumn : range.RightColumn;
            int rowCount = bottomRow + 1 - topRow;
            int columnCount = rightColumn + 1 - leftColumn;
            return new CellRange(topRow, leftColumn, rowCount, columnCount);
        }

        public Rect GetColumnRect(int column)
        {
            return new Rect(GetColumnLocation(column), _actualBounds.Top,
                _columns.GetColumnWidth(column), _actualBounds.Height);
        }

        public Rect GetRowRect(int row)
        {
            return new Rect(_actualBounds.Left, GetRowLocation(row),
                _actualBounds.Width, _rows.GetRowHeight(row));
        }

        public Rect GetRangeRect(CellRange range)
        {
            return GetRangeRect(range.TopRow, range.LeftColumn, range.BottomRow, range.RightColumn);
        }

        public Rect GetRangeRect(int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var topLeftCellRect = GetCellRect(topRow, leftColumn);

            if (bottomRow > topRow || rightColumn > leftColumn)
            {
                var bottomRightCellRect = GetCellRect(bottomRow, rightColumn);
                var rangeRect = new Rect(topLeftCellRect.TopLeft, bottomRightCellRect.BottomRight);
                return rangeRect;
            }
            else
            {
                return topLeftCellRect;
            }
        }

        public Rect GetViewRangeRect()
        {
            return GetRangeRect(ViewRange);
        }

        public bool IsRowVisible(int row)
        {
            var rowRect = GetRowRect(row);
            return ActualBounds.ContainsOrIntersectsWith(rowRect);
        }

        public bool IsColumnVisible(int col)
        {
            var colRect = GetColumnRect(col);
            return ActualBounds.ContainsOrIntersectsWith(colRect);
        }

        public void RefreshBounds()
        {
            var sheetCanvas = _sheetView.Spread.SheetViewPane.CellsRegion;
            double zoom = _sheetView != null && _sheetView.ZoomFactor > 0 ? _sheetView.ZoomFactor : 1.0;
            _actualBounds.X = _sheetView.ScrollPosition.X;
            _actualBounds.Y = _sheetView.ScrollPosition.Y;
            _actualBounds.Width = sheetCanvas.ActualWidth / zoom;
            _actualBounds.Height = sheetCanvas.ActualHeight / zoom;
        }

        public Rect GetCellRect(int row, int col)
        {
            if (row == -1 || col == -1)
                return new Rect();

            int rowSpan = 1;
            int colSpan = 1;

            var spanRange = _workSheet.GetSpanCellRange(row, col);
            if (spanRange != default)
            {
                row = spanRange.TopRow;
                col = spanRange.LeftColumn;
                rowSpan = spanRange.RowCount;
                colSpan = spanRange.ColumnCount;
            }

            var colLocation = GetColumnLocation(col);
            var rowLocation = GetRowLocation(row);
            var width = _columns.GetColumnWidth(col);
            var height = _rows.GetRowHeight(row);

            if (rowSpan > 1)
            {
                int bottomRow = row + rowSpan - 1;
                height = (int)(GetRowLocation(bottomRow) - rowLocation + _rows.GetRowHeight(bottomRow));
            }

            if (colSpan > 1)
            {
                int rightColumn = col + colSpan - 1;
                width = (int)(GetColumnLocation(rightColumn) - colLocation + _columns.GetColumnWidth(rightColumn));
            }

            return new Rect(colLocation, rowLocation, width, height);
        }

        /// <summary>
        /// Calculates the view port from the current top row and left column.
        /// </summary>
        internal void CalculateVisibleRange()
        {
            RefreshBounds();

            if (ViewRange.TopRow < 0 || ViewRange.LeftColumn < 0)
                return;

            for (int row = ViewRange.TopRow; row < _workSheet.RowCount; row++)
            {
                if (IsRowVisible(row))
                {
                    SetRowCount(row - _viewRange.TopRow + 1);
                }
                else
                    break;
            }

            for (int col = ViewRange.LeftColumn; col < _workSheet.ColumnCount; col++)
            {
                if (IsColumnVisible(col))
                    SetColumnCount(col - _viewRange.LeftColumn + 1);
                else
                    break;
            }
        }

        /// <summary>
        /// Calculates the first visible row.
        /// </summary>
        /// <param name="delta"></param>
        internal void CalculateTopRow(double delta)
        {
            if (_workSheet.RowCount == 0)
                return;

            if (!_rows.HasItems)
            {
                int defaultRowHeight = _workSheet.DefaultRowHeight;
                int targetRow = (int)(_sheetView.ScrollPosition.Y / defaultRowHeight);
                targetRow = Math.Max(0, Math.Min(_workSheet.RowCount - 1, targetRow));
                double rowLocation = targetRow * defaultRowHeight;

                TopRowLocation = _sheetView.Spread.ScrollMode == SheetScrollMode.Pixel
                    ? _sheetView.ScrollPosition.Y : rowLocation;
                SetTopRow(targetRow);
                return;
            }

            if (delta >= 0)
            {
                for (int row = ViewRange.TopRow; row < _workSheet.RowCount; row++)
                {
                    var rowLocation = GetRowLocation(row);
                    if (IsTopRow(row, rowLocation))
                    {
                        TopRowLocation = _sheetView.Spread.ScrollMode == SheetScrollMode.Pixel
                            ? _sheetView.ScrollPosition.Y : rowLocation;
                        SetTopRow(row);
                        break;
                    }
                }
            }
            else
            {
                for (int row = ViewRange.TopRow; row >= 0; row--)
                {
                    var rowLocation = GetRowLocation(row);
                    if (IsTopRow(row, rowLocation))
                    {
                        TopRowLocation = _sheetView.Spread.ScrollMode == SheetScrollMode.Pixel
                            ? _sheetView.ScrollPosition.Y : rowLocation;
                        SetTopRow(row);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the first visible column.
        /// </summary>
        /// <param name="delta"></param>
        internal void CalculateLeftColumn(double delta)
        {
            if (_workSheet.ColumnCount == 0)
                return;

            if (!_columns.HasItems)
            {
                int defaultColumnWidth = _workSheet.DefaultColumnWidth;
                int targetCol = (int)(_sheetView.ScrollPosition.X / defaultColumnWidth);
                targetCol = Math.Max(0, Math.Min(_workSheet.ColumnCount - 1, targetCol));
                double colLocation = targetCol * defaultColumnWidth;

                LeftColumnLocation = _sheetView.Spread.ScrollMode == SheetScrollMode.Pixel
                    ? _sheetView.ScrollPosition.X : colLocation;
                SetLeftColumn(targetCol);
                return;
            }

            if (delta >= 0)
            {
                for (int col = ViewRange.LeftColumn; col < _workSheet.ColumnCount; col++)
                {
                    var colLocation = GetColumnLocation(col);
                    if (IsLeftColumn(col, colLocation))
                    {
                        LeftColumnLocation = _sheetView.Spread.ScrollMode == SheetScrollMode.Pixel
                            ? _sheetView.ScrollPosition.X : colLocation;
                        SetLeftColumn(col);
                        break;
                    }
                }
            }
            else
            {
                for (int col = ViewRange.LeftColumn; col >= 0; col--)
                {
                    var colLocation = GetColumnLocation(col);
                    if (IsLeftColumn(col, colLocation))
                    {
                        LeftColumnLocation = _sheetView.Spread.ScrollMode == SheetScrollMode.Pixel
                            ? _sheetView.ScrollPosition.X : colLocation;
                        SetLeftColumn(col);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the row is top row.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool IsTopRow(int row, double rowLocation)
        {
            return _sheetView.ScrollPosition.Y >= rowLocation &&
                _sheetView.ScrollPosition.Y < rowLocation + _rows.GetRowHeight(row);
        }

        /// <summary>
        /// Gets whether the column is left column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool IsLeftColumn(int column, double colLocation)
        {
            return _sheetView.ScrollPosition.X >= colLocation &&
                _sheetView.ScrollPosition.X < colLocation + _columns.GetColumnWidth(column);
        }

        public void SetTopRow(int row)
        {
            _viewRange.SetTopRow(row);
        }

        public void SetLeftColumn(int column)
        {
            _viewRange.SetLeftColumn(column);
        }

        public void SetRowCount(int rowCount)
        {
            _viewRange.SetRowCount(rowCount);
        }

        public void SetColumnCount(int columnCount)
        {
            _viewRange.SetColumnCount(columnCount);
        }

        private bool GetIsEmpty()
        {
            return _sheetView.WorkSheet.RowCount == 0 || _sheetView.WorkSheet.ColumnCount == 0;
        }

        public override string ToString()
        {
            return $"TopRow:{ViewRange.TopRow}, BottomRow:{ViewRange.BottomRow}, LeftColumn:{ViewRange.LeftColumn}, RightColumn:{ViewRange.RightColumn}";
        }

        private sealed class LocationCache<T> where T : class
        {
            private readonly Func<int> _count;
            private readonly Func<double> _defaultSize;
            private readonly SheetDimensionCollection<T> _items;
            private readonly Func<T, double> _getSize;

            private double[] _locations;
            private int _lastCalculated;

            public LocationCache(
                Func<int> count,
                Func<double> defaultSize,
                SheetDimensionCollection<T> items,
                Func<T, double> getSize)
            {
                _count = count;
                _defaultSize = defaultSize;
                _items = items;
                _getSize = getSize;
            }

            public double GetLocation(int index)
            {
                if (index <= 0)
                    return 0;

                EnsureCapacity(index + 1);

                while (_lastCalculated < index)
                {
                    double size = _defaultSize();

                    var item = _items.GetItem(_lastCalculated);

                    if (item != null)
                        size = _getSize(item);
                      
                    _locations[_lastCalculated + 1] =
                        _locations[_lastCalculated] + size;

                    _lastCalculated++;
                }

                return _locations[index];
            }

            public void UpdateLocation(int fromIndex, double delta)
            {
                if (delta == 0 || _locations == null)
                    return;

                for (int i = fromIndex; i <= _lastCalculated; i++)
                    _locations[i] += delta;
            }

            public void Reset()
            {
                _lastCalculated = 0;

                if (_locations != null && _locations.Length > 0)
                    _locations[0] = 0;
            }

            private void EnsureCapacity(int requiredCapacity = 0)
            {
                int count = Math.Max(_count() + 1, requiredCapacity);

                if (_locations == null)
                {
                    _locations = new double[Math.Max(16, count)];
                }

                if (_locations.Length >= count)
                    return;

                int size = _locations.Length;

                while (size < count)
                    size *= 2;

                Array.Resize(ref _locations, size);
            }
        }
    }
}
