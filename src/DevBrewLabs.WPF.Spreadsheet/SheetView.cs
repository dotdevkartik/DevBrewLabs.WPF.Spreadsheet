using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Utils;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class SheetView : ISheetView, IDisposable
    {
        private HeadersVisibility _headersVisibility;
        private ViewPort _viewPort;
        private WorkSheet _workSheet;
        private Rows _rows;
        private Cells _cells;
        private Columns _columns;
        private double _zoomFactor = 1.0;
        private CellRange _selection;

        #region Properties
        public GridLineVisibility GridLineVisibility { get; set; }
        public HeadersVisibility HeadersVisibility
        {
            get
            {
                return _headersVisibility;
            }
            set
            {
                _headersVisibility = value;
                SetHeadersVisibility();
            }
        }
        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                if (Spread?.ZoomManager != null && Spread.SheetViews?.ActiveSheetView == this)
                {
                    Spread.ZoomManager.SetZoom(value);
                }
                else
                {
                    InternalSetZoomFactor(Math.Max(0.1, Math.Min(4.0, Math.Round(value, 2))));
                }
            }
        }
        public IViewPort ViewPort => _viewPort;
        public Point ScrollPosition { get; private set; }
        public SelectionMode SelectionMode { get; set; }
        public MouseWheelScrollDirection MouseWheelScrollDirection { get; set; }
        public Spread Spread { get; }
        public int ActiveRow { get; internal set; }
        public int ActiveColumn { get; internal set; }
        public CellRange Selection => _selection;
        public IWorkSheet WorkSheet => _workSheet;
        public bool AutoSizeRows { get; set; }
        public bool AutoSizeColumns { get; set; }
        #endregion

        public SheetView(Spread spread, WorkSheet worksheet)
        {
            Spread = spread;
            _workSheet = worksheet;
            _rows = (Rows)_workSheet.Rows;
            _columns = (Columns)_workSheet.Columns;
            _cells = (Cells)_workSheet.Cells;
            _zoomFactor = 1.0;
            GridLineVisibility = GridLineVisibility.Both;
            SelectionMode = SelectionMode.CellRange;
            MouseWheelScrollDirection = MouseWheelScrollDirection.Vertical;
            ScrollPosition = new Point(0, 0);
            _viewPort = new ViewPort(this);
            HeadersVisibility = HeadersVisibility.Both;
            _selection = new CellRange(0, 0);
            AutoSizeRows = true;
            AutoSizeColumns = false;
        }

        #region Public
        public void Copy()
        {
            Spread.ClipboardManager.Copy(this);
        }

        public void Paste()
        {
            Spread.ClipboardManager.Paste(this);
        }

        public void CopyRange(CellRange range)
        {
            Spread.ClipboardManager.Copy(this, range);
        }

        public void MergeRange(CellRange range)
        {
            if (range.RowCount > 1 || range.ColumnCount > 1)
            {
                var action = new SpanChangedAction()
                {
                    SheetView = this,
                    Row = range.TopRow,
                    Column = range.LeftColumn,
                    OldRowSpan = WorkSheet.GetRowSpan(range.TopRow, range.LeftColumn),
                    OldColumnSpan = WorkSheet.GetColumnSpan(range.TopRow, range.LeftColumn),
                    NewRowSpan = range.RowCount,
                    NewColumnSpan = range.ColumnCount,
                    OldValues = new object[range.RowCount, range.ColumnCount]
                };

                for (int r = 0; r < range.RowCount; r++)
                {
                    for (int c = 0; c < range.ColumnCount; c++)
                    {
                        action.OldValues[r, c] = WorkSheet.GetValue(range.TopRow + r, range.LeftColumn + c);
                    }
                }

                WorkSheet.AddSpan(range.TopRow, range.LeftColumn, range.RowCount, range.ColumnCount);
                Spread.UndoRedoManager.AddAction(action);
            }
        }

        public void UnmergeRange(CellRange range)
        {
            var anchor = WorkSheet.GetSpanCellRange(range.TopRow, range.LeftColumn);
            if (anchor != default)
            {
                var action = new SpanChangedAction()
                {
                    SheetView = this,
                    Row = anchor.TopRow,
                    Column = anchor.LeftColumn,
                    OldRowSpan = anchor.RowCount,
                    OldColumnSpan = anchor.ColumnCount,
                    NewRowSpan = 1,
                    NewColumnSpan = 1
                };

                WorkSheet.RemoveSpan(anchor.TopRow, anchor.LeftColumn);
                Spread.UndoRedoManager.AddAction(action);
            }
        }

        public void SelectCell(int row, int col)
        {
            Spread.SelectionManager.SelectCell(this, row, col);
        }

        public void SelectColumn(int column)
        {
            Spread.SelectionManager.SelectColumn(this, column);
        }

        public void SelectColumns(int column, int count)
        {
            Spread.SelectionManager.SelectColumns(this, column, count);
        }

        public void SelectRow(int row)
        {
            Spread.SelectionManager.SelectRow(this, row);
        }

        public void SelectRows(int row, int count)
        {
            Spread.SelectionManager.SelectRows(this, row, count);
        }

        public void SelectRange(CellRange range)
        {
            Spread.SelectionManager.SelectRange(this, range);
        }

        public void SelectRange(int row, int column, int rowCount, int columnCount)
        {
            Spread.SelectionManager.SelectRange(this, row, column, rowCount, columnCount);
        }

        public void ScrollToHorizontalOffset(double offset)
        {
            double delta = offset - ScrollPosition.X;
            ScrollPosition = new Point(offset, ScrollPosition.Y);
            _viewPort.CalculateLeftColumn(delta);
            _viewPort.CalculateVisibleRange();
            Spread.Invalidate(false, true, true);
        }

        public void ScrollToVerticalOffset(double offset)
        {
            double delta = offset - ScrollPosition.Y;
            ScrollPosition = new Point(ScrollPosition.X, offset);
            _viewPort.CalculateTopRow(delta);
            _viewPort.CalculateVisibleRange();
            Spread.Invalidate(true, false, true);
        }
        #endregion

        #region Private
        private void SetHeadersVisibility()
        {
            Spread.SheetViewPane.UpdateHeadersSize();
        }
        #endregion

        #region Internal
        internal void InternalSetZoomFactor(double zoomFactor)
        {
            if (Math.Abs(_zoomFactor - zoomFactor) > 0.001)
            {
                var oldVal = _zoomFactor;
                _zoomFactor = zoomFactor;
            }
        }

        internal double GetRowHeaderWidth()
        {
            if (HeadersVisibility == HeadersVisibility.Row || HeadersVisibility == HeadersVisibility.Both)
                return _workSheet.RowHeaders.Width;
            else return 0;
        }

        internal double GetColumnHeaderHeight()
        {
            if (HeadersVisibility == HeadersVisibility.Column || HeadersVisibility == HeadersVisibility.Both)
                return _workSheet.ColumnHeaders.Height;
            else return 0;
        }

        internal void SetSelection(CellRange range)
        {
            _selection = range;
        }

        #endregion

        public override string ToString()
        {
            return _workSheet.Name;
        }

        public void AutoSizeRow(int row)
        {
            if (row < 0 || row >= _workSheet.RowCount)
                return;

            int maxRequiredHeight = _workSheet.DefaultRowHeight;

            for (int col = 0; col < _workSheet.ColumnCount; col++)
            {
                var value = _workSheet.GetValue(row, col);
                if (value == null)
                    continue;

                var sheetColumn = _columns.GetItem(col);
                var sheetRow = _rows.GetItem(row);

                var formatter = _workSheet.GetCellFormatter(row, col, sheetRow, sheetColumn);
                string text = formatter != null ? formatter.Format(value) : value.ToString();

                if (string.IsNullOrEmpty(text))
                    continue;

                IStyle style = _workSheet.GetCellStyle(row, col, sheetRow, sheetColumn);

                string[] lines = style.AllowMultiLineText 
                    ? TextUtils.GetLines(text) 
                    : new[] { TextUtils.NormalizeToSingleLine(text) };

                var metrics = Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics;
                double lineHeight = metrics != null ? metrics.Height * style.FontSize : style.FontSize * 1.3;
                
                // Add a small padding (4) so standard fonts (~16px) evaluate to ~20px 
                // which is <= DefaultRowHeight (22), preventing unwanted resizing for single lines.
                int cellRequiredHeight = (int)Math.Ceiling(lines.Length * lineHeight + 4);
                
                if (cellRequiredHeight > maxRequiredHeight)
                {
                    maxRequiredHeight = cellRequiredHeight;
                }
            }

            int currentHeight = _workSheet.Rows.GetRowHeight(row);
            if (currentHeight != maxRequiredHeight)
            {
                _workSheet.Rows[row].Height = maxRequiredHeight;
            }
        }

        public void AutoSizeColumn(int column)
        {
            var sheetColumn = _columns.GetItem(column);
            var width = 0;
            var cellValues = _cells.GetCellValues(column);

            foreach(var cellValue in cellValues)
            {
                if(cellValue.Value != null)
                {
                    var sheetRow = _rows.GetItem(cellValue.Key);
                    var formatter = _workSheet.GetCellFormatter(cellValue.Key, column, sheetRow, sheetColumn);
                    string text = formatter != null ? formatter.Format(cellValue.Value) : cellValue.Value.ToString();

                    if (string.IsNullOrEmpty(text))
                        continue;

                    IStyle style = _workSheet.GetCellStyle(cellValue.Key, column, sheetRow, sheetColumn);
                    string[] lines = style.AllowMultiLineText
                     ? TextUtils.GetLines(text)
                     : new[] { TextUtils.NormalizeToSingleLine(text) };

                    foreach (string line in lines)
                    {
                        var textWidth = TextMeasurer.MeasureWidth(line, style.FontSize, style != null ? Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics : null);
                        width = Math.Max(width, (int)Math.Ceiling(textWidth) + 11);
                    }
                }
            }

            if (width == 0)
            {
                width = WorkSheet.DefaultColumnWidth;
            }

            if(width != WorkSheet.Columns.GetColumnWidth(column))
            {
                WorkSheet.Columns[column].Width = width;
            }

            _viewPort.CalculateVisibleRange();
            Spread.SheetTabControl.UpdateScrollbars();
            Spread.Invalidate();
        }

        public void Dispose()
        {
            _workSheet = null;
            _cells = null;
            _rows = null;
            _columns = null;
        }
    }
}


