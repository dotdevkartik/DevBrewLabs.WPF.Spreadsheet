using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RowHeadersSurface : SheetViewSurface
    {
        private Worksheet _workSheet;
        private ViewPort _viewPort;
        private DrawingGroup _drawing;
        private readonly int _resizeDelta;

        public RowHeadersSurface()
        {
            _resizeDelta = 5;
            _drawing = new DrawingGroup();
        }

        public override void AttachSheet(SheetView sheetView)
        {
            base.AttachSheet(sheetView);
            _workSheet = (Worksheet)sheetView.WorkSheet;
            _viewPort = sheetView.ViewPort.As<ViewPort>();

            _drawing.Children.Clear();
            _drawing.Children.Add(sheetView.Spread.RenderEngine.RowHeadersRenderer.Drawing);
            _drawing.Children.Add(sheetView.Spread.RenderEngine.RowHeaderGridLinesRenderer.Drawing);
        }

        protected override Drawing GetDrawing()
        {
            return _drawing;
        }

        protected override SpreadHitTestResult HitTestCore(SheetView sheetView, Point hitPoint)
        {
            var hitTestInfo = new SpreadHitTestResult() { Element = VisualElement.RowHeader, Sheet = sheetView, ActualHitTestPoint = hitPoint };
            var rows = _workSheet.Rows.As<Rows>();
            var columns = _workSheet.RowHeaders.Columns.As<RowHeaderColumns>();
            var viewRange = sheetView.ViewPort.ViewRange;

            double zoom = sheetView != null && sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + _viewPort.LeftColumnLocation,
                hitPoint.Y / zoom + _viewPort.TopRowLocation);

            double x = 0, y = 0;

            // Check for hidden row resize handle hit first (only if custom row settings exist)
            int startRowSearch = Math.Max(0, _viewPort.ViewRange.TopRow - 1);
            int endRowSearch = Math.Min(_workSheet.RowCount, _viewPort.ViewRange.BottomRow + 2);

            for (int row = startRowSearch; row < endRowSearch; row++)
            {
                if (rows.GetRowHeight(row) == 0)
                {
                    int startHiddenRow = row;
                    int lastHiddenRow = row;
                    while (lastHiddenRow + 1 < _workSheet.RowCount && rows.GetRowHeight(lastHiddenRow + 1) == 0)
                    {
                        lastHiddenRow++;
                    }

                    var rowLocation = sheetView.ViewPort.GetRowLocation(startHiddenRow);
                    bool isHit;
                    if (rowLocation == 0)
                    {
                        isHit = point.Y >= 0 && point.Y <= _resizeDelta + 2;
                    }
                    else
                    {
                        isHit = point.Y >= rowLocation - 2 && point.Y <= rowLocation + _resizeDelta;
                    }

                    if (isHit)
                    {
                        hitTestInfo.Element = VisualElement.RowHeaderResizeBar;
                        hitTestInfo.Row = lastHiddenRow;
                        y = rowLocation;
                        hitTestInfo.Position = new Point(x - _viewPort.LeftColumnLocation,
                            y - _viewPort.TopRowLocation);
                        return hitTestInfo;
                    }

                    row = lastHiddenRow;
                }
            }

            // Check visible row resize boundaries (centered around bottom edge)
            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                var rowLocation = sheetView.ViewPort.GetRowLocation(row);
                double rowHeight = _workSheet.Rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                double bottomEdge = rowLocation + rowHeight;
                if (point.Y >= bottomEdge - _resizeDelta && point.Y <= bottomEdge + _resizeDelta)
                {
                    hitTestInfo.Element = VisualElement.RowHeaderResizeBar;
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    hitTestInfo.Position = new Point(x - _viewPort.LeftColumnLocation,
                        y - _viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            // Check visible row body hit
            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                var rowLocation = sheetView.ViewPort.GetRowLocation(row);
                double rowHeight = _workSheet.Rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    break;
                }
            }

            for (int col = 0; col < _workSheet.RowHeaders.ColumnCount; col++)
            {
                var colLocation = sheetView.ViewPort.GetHeaderColumnLocation(col);
                var sheetColumn = columns.GetItem(col);
                double columnWidth = sheetColumn == null ? _workSheet.DefaultColumnWidth : sheetColumn.Width;

                if (point.X >= colLocation && point.X < colLocation + columnWidth)
                {
                    hitTestInfo.Column = col;
                    x = colLocation;
                    break;
                }
            }

            hitTestInfo.Position = new Point(x - _viewPort.LeftColumnLocation,
                y - _viewPort.TopRowLocation);
            return hitTestInfo;
        }
    }
}
