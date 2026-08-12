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
            return new Rect(_columns.GetLocation(column), _actualBounds.Top,
                _columns.GetColumnWidth(column), _actualBounds.Height);
        }

        public Rect GetRowRect(int row)
        {
            return new Rect(_actualBounds.Left, _rows.GetLocation(row), 
                _actualBounds.Width, _rows.GetRowHeight(row));
        }

        public Rect GetRangeRect(CellRange range)
        {
            return GetRangeRect(range.TopRow, range.LeftColumn, range.BottomRow, range.RightColumn);
        }

        public Rect GetRangeRect(int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var topLeftCellRect = GetCellRect(topRow, leftColumn);

            if(bottomRow > topRow || rightColumn > leftColumn)
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

            var colLocation = _columns.GetLocation(col);
            var rowLocation = _rows.GetLocation(row);
            var width = _columns.GetColumnWidth(col);
            var height = _rows.GetRowHeight(row);

            if (rowSpan > 1)
            {
                int bottomRow = row + rowSpan - 1;
                height = (int)(_rows.GetLocation(bottomRow) - rowLocation + _rows.GetRowHeight(bottomRow));
            }

            if (colSpan > 1)
            {
                int rightColumn = col + colSpan - 1;
                width = (int)(_columns.GetLocation(rightColumn) - colLocation + _columns.GetColumnWidth(rightColumn));
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

            if (_rows.InternalCollection.Count == 0)
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
                    var rowLocation = _rows.GetLocation(row);
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
                    var rowLocation = _rows.GetLocation(row);
                    if(IsTopRow(row, rowLocation))
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

            if (_columns.InternalCollection.Count == 0)
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
                    var colLocation = _columns.GetLocation(col);
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
                    var colLocation = _columns.GetLocation(col);
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
    }
}
