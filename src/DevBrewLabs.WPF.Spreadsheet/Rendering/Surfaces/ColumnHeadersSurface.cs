using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class ColumnHeadersSurface : SheetViewSurface
    {
        private WorkSheet _workSheet;
        private ViewPort _viewPort;
        private readonly int _resizeDelta;
        private DrawingGroup _drawing;

        public ColumnHeadersSurface()
        {
            _resizeDelta = 5;
            _drawing = new DrawingGroup();
        }

        public override void AttachSheet(SheetView sheetView)
        {
            base.AttachSheet(sheetView);
            _workSheet = (WorkSheet)sheetView.WorkSheet;
            _viewPort = sheetView.ViewPort.As<ViewPort>();

            _drawing.Children.Clear();
            _drawing.Children.Add(sheetView.Spread.RenderEngine.ColumnHeadersRenderer.Drawing);
            _drawing.Children.Add(sheetView.Spread.RenderEngine.ColumnHeaderGridLinesRenderer.Drawing);
        }

        protected override Drawing GetDrawing()
        {
            return _drawing;
        }

        protected override SpreadHitTestResult HitTestCore(SheetView sheetView, Point hitPoint)
        {
            var hitTestInfo = new SpreadHitTestResult() { Element = VisualElement.ColumnHeader, Sheet = sheetView, ActualHitTestPoint = hitPoint };
            var rows = _workSheet.ColumnHeaders.Rows.As<ColumnHeaderRows>();
            var columns = _workSheet.Columns.As<Columns>();
            var viewRange = sheetView.ViewPort.ViewRange;

            double zoom = sheetView != null && sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + _viewPort.LeftColumnLocation,
                hitPoint.Y / zoom + _viewPort.TopRowLocation);

            double x = 0, y = 0;

            for (int row = 0; row < _workSheet.ColumnHeaders.RowCount; row++)
            {
                var rowLocation = sheetView.ViewPort.GetHeaderRowLocation(row);
                var sheetRow = rows.GetItem(row);
                double rowHeight = sheetRow == null ? _workSheet.DefaultRowHeight : sheetRow.Height;

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    break;
                }
            }

            // Check for hidden column resize handle hit first (only if custom column settings exist)
            int startColSearch = Math.Max(0, _viewPort.ViewRange.LeftColumn - 1);
            int endColSearch = Math.Min(_workSheet.ColumnCount, _viewPort.ViewRange.RightColumn + 2);

            for (int col = startColSearch; col < endColSearch; col++)
            {
                if (columns.GetColumnWidth(col) == 0)
                {
                    int startHiddenCol = col;
                    int lastHiddenCol = col;
                    while (lastHiddenCol + 1 < _workSheet.ColumnCount && columns.GetColumnWidth(lastHiddenCol + 1) == 0)
                    {
                        lastHiddenCol++;
                    }

                    var colLocation = sheetView.ViewPort.GetColumnLocation(startHiddenCol);
                    bool isHit;
                    if (colLocation == 0)
                    {
                        isHit = point.X >= 0 && point.X <= _resizeDelta + 2;
                    }
                    else
                    {
                        isHit = point.X >= colLocation - 2 && point.X <= colLocation + _resizeDelta;
                    }

                    if (isHit)
                    {
                        hitTestInfo.Element = VisualElement.ColumnHeaderResizeBar;
                        hitTestInfo.Column = lastHiddenCol;
                        x = colLocation;
                        hitTestInfo.Position = new Point(x - _viewPort.LeftColumnLocation,
                            y - _viewPort.TopRowLocation);
                        return hitTestInfo;
                    }

                    col = lastHiddenCol;
                }
            }

            // Check visible column resize boundaries (centered around right edge)
            for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
            {
                var colLocation = sheetView.ViewPort.GetColumnLocation(col);
                double columnWidth = _workSheet.Columns.GetColumnWidth(col);

                if (columnWidth == 0)
                    continue;

                double rightEdge = colLocation + columnWidth;
                if (point.X >= rightEdge - _resizeDelta && point.X <= rightEdge + _resizeDelta)
                {
                    hitTestInfo.Element = VisualElement.ColumnHeaderResizeBar;
                    hitTestInfo.Column = col;
                    x = colLocation;
                    hitTestInfo.Position = new Point(x - _viewPort.LeftColumnLocation,
                        y - _viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            // Check visible column body hit
            for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
            {
                var colLocation = sheetView.ViewPort.GetColumnLocation(col);
                double columnWidth = _workSheet.Columns.GetColumnWidth(col);

                if (columnWidth == 0)
                    continue;

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
