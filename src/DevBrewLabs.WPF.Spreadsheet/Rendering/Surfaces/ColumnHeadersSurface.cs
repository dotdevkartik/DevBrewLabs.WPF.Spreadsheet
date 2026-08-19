using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class ColumnHeadersSurface : SurfaceBase
    {
        private readonly int _resizeDelta;

        public ColumnHeadersSurface(SheetView view): base(view)
        {
            _resizeDelta = 5;
        }

        protected override InteractionLayer CreateInteractionLayer()
        {
            return new ColumnHeadersInteractionLayer(SheetView);
        }

        protected override DrawingGroup CreateDrawing()
        {
            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(new DrawingGroup()); // cells drawing
            drawing.Children.Add(new DrawingGroup()); // grid line drawing
            return drawing;
        }

        protected override SpreadHitTestResult HitTestCore(Point hitPoint)
        {
            var workSheet = (Worksheet)SheetView.WorkSheet;
            var viewPort = SheetView.ViewPort;

            var hitTestInfo = new SpreadHitTestResult() { Element = VisualElement.ColumnHeader, Sheet = SheetView, ActualHitTestPoint = hitPoint };
            var rows = workSheet.ColumnHeaders.Rows.As<ColumnHeaderRows>();
            var columns = workSheet.Columns.As<Columns>();
            var viewRange = SheetView.ViewPort.ViewRange;

            double zoom = SheetView != null && SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + viewPort.LeftColumnLocation,
                hitPoint.Y / zoom + viewPort.TopRowLocation);

            double x = 0, y = 0;

            for (int row = 0; row < workSheet.ColumnHeaders.RowCount; row++)
            {
                var rowLocation = SheetView.ViewPort.GetHeaderRowLocation(row);
                var sheetRow = rows.GetItem(row);
                double rowHeight = sheetRow == null ? workSheet.DefaultRowHeight : sheetRow.Height;

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    break;
                }
            }

            // Check for hidden column resize handle hit first (only if custom column settings exist)
            int startColSearch = Math.Max(0, viewPort.ViewRange.LeftColumn - 1);
            int endColSearch = Math.Min(workSheet.ColumnCount, viewPort.ViewRange.RightColumn + 2);

            for (int col = startColSearch; col < endColSearch; col++)
            {
                if (columns.GetColumnWidth(col) == 0)
                {
                    int startHiddenCol = col;
                    int lastHiddenCol = col;
                    while (lastHiddenCol + 1 < workSheet.ColumnCount && columns.GetColumnWidth(lastHiddenCol + 1) == 0)
                    {
                        lastHiddenCol++;
                    }

                    var colLocation = SheetView.ViewPort.GetColumnLocation(startHiddenCol);
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
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }

                    col = lastHiddenCol;
                }
            }

            // Check visible column resize boundaries (centered around right edge)
            for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
            {
                var colLocation = SheetView.ViewPort.GetColumnLocation(col);
                double columnWidth = workSheet.Columns.GetColumnWidth(col);

                if (columnWidth == 0)
                    continue;

                double rightEdge = colLocation + columnWidth;
                if (point.X >= rightEdge - _resizeDelta && point.X <= rightEdge + _resizeDelta)
                {
                    hitTestInfo.Element = VisualElement.ColumnHeaderResizeBar;
                    hitTestInfo.Column = col;
                    x = colLocation;
                    hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                        y - viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            // Check visible column body hit
            for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
            {
                var colLocation = SheetView.ViewPort.GetColumnLocation(col);
                double columnWidth = workSheet.Columns.GetColumnWidth(col);

                if (columnWidth == 0)
                    continue;

                if (point.X >= colLocation && point.X < colLocation + columnWidth)
                {
                    hitTestInfo.Column = col;
                    x = colLocation;
                    break;
                }
            }

            hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                y - viewPort.TopRowLocation);
            return hitTestInfo;
        }
    }
}
