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

            // 1. Check if the sheet begins with hidden columns at the very left (X = 0)
            if (workSheet.ColumnCount > 0 && !columns.IsColumnVisible(0))
            {
                if (point.X >= 0 && point.X <= _resizeDelta + 2)
                {
                    int lastHiddenCol = 0;
                    while (lastHiddenCol + 1 < workSheet.ColumnCount && !columns.IsColumnVisible(lastHiddenCol + 1))
                    {
                        lastHiddenCol++;
                    }

                    hitTestInfo.Element = VisualElement.ColumnHeaderResizeBar;
                    hitTestInfo.Column = lastHiddenCol;
                    x = 0;
                    hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                        y - viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            // 2. Check visible column resize boundaries and unhide handles for following hidden columns
            for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
            {
                if (!columns.IsColumnVisible(col)) continue;

                var colLocation = SheetView.ViewPort.GetColumnLocation(col);
                double columnWidth = workSheet.Columns.GetColumnWidth(col);

                if (columnWidth == 0)
                    continue;

                double rightEdge = colLocation + columnWidth;
                int nextCol = col + 1;
                bool hasHiddenAfter = nextCol < workSheet.ColumnCount && !columns.IsColumnVisible(nextCol);

                if (hasHiddenAfter)
                {
                    int lastHiddenCol = nextCol;
                    while (lastHiddenCol + 1 < workSheet.ColumnCount && !columns.IsColumnVisible(lastHiddenCol + 1))
                    {
                        lastHiddenCol++;
                    }

                    // Left half of double-line indicator -> resizes the visible column to the left
                    if (point.X >= rightEdge - _resizeDelta && point.X <= rightEdge - 0.5)
                    {
                        hitTestInfo.Element = VisualElement.ColumnHeaderResizeBar;
                        hitTestInfo.Column = col;
                        x = colLocation;
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }
                    // Right half of double-line indicator -> unhides/expands the last hidden column in the contiguous block
                    if (point.X > rightEdge - 0.5 && point.X <= rightEdge + _resizeDelta)
                    {
                        hitTestInfo.Element = VisualElement.ColumnHeaderResizeBar;
                        hitTestInfo.Column = lastHiddenCol;
                        x = rightEdge;
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }
                }
                else
                {
                    // Standard single border between visible columns
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
            }

            // 3. Check visible column body hit
            for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
            {
                if (!columns.IsColumnVisible(col)) continue;

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
