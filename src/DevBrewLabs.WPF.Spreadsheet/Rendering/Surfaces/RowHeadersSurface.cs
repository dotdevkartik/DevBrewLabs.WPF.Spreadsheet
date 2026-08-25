using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RowHeadersSurface : SurfaceBase
    {
        private readonly int _resizeDelta;

        public RowHeadersSurface(SheetView view) : base(view)
        {
            _resizeDelta = 5;
        }

        protected override InteractionLayer CreateInteractionLayer()
        {
            return new RowHeadersInteractionLayer(SheetView);
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

            var hitTestInfo = new SpreadHitTestResult() { Element = SheetElement.RowHeader, Sheet = SheetView, ActualHitTestPoint = hitPoint };
            var rows = workSheet.Rows.As<Rows>();
            var columns = workSheet.RowHeaders.Columns.As<RowHeaderColumns>();
            var viewRange = SheetView.ViewPort.ViewRange;

            double zoom = SheetView != null && SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + viewPort.LeftColumnLocation,
                hitPoint.Y / zoom + viewPort.TopRowLocation);

            double x = 0, y = 0;

            // 1. Check if the sheet begins with hidden/filtered rows at the very top (Y = 0)
            if (workSheet.RowCount > 0 && !rows.IsRowVisible(0))
            {
                if (point.Y >= 0 && point.Y <= _resizeDelta + 2)
                {
                    int lastHiddenRow = 0;
                    while (lastHiddenRow + 1 < workSheet.RowCount && !rows.IsRowVisible(lastHiddenRow + 1))
                    {
                        lastHiddenRow++;
                    }

                    hitTestInfo.Element = SheetElement.RowHeaderResizeBar;
                    hitTestInfo.Row = lastHiddenRow;
                    y = 0;
                    hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                        y - viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            // 2. Check visible row resize boundaries and unhide handles for following hidden rows
            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                if (!rows.IsRowVisible(row)) continue;

                var rowLocation = SheetView.ViewPort.GetRowLocation(row);
                double rowHeight = workSheet.Rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                double bottomEdge = rowLocation + rowHeight;
                int nextRow = row + 1;
                bool hasHiddenAfter = nextRow < workSheet.RowCount && !rows.IsRowVisible(nextRow);

                if (hasHiddenAfter)
                {
                    int lastHiddenRow = nextRow;
                    while (lastHiddenRow + 1 < workSheet.RowCount && !rows.IsRowVisible(lastHiddenRow + 1))
                    {
                        lastHiddenRow++;
                    }

                    // Upper half of double-line indicator -> resizes the visible row above
                    if (point.Y >= bottomEdge - _resizeDelta && point.Y <= bottomEdge - 0.5)
                    {
                        hitTestInfo.Element = SheetElement.RowHeaderResizeBar;
                        hitTestInfo.Row = row;
                        y = rowLocation;
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }
                    // Lower half of double-line indicator -> unhides/expands the last hidden/filtered row
                    if (point.Y > bottomEdge - 0.5 && point.Y <= bottomEdge + _resizeDelta)
                    {
                        hitTestInfo.Element = SheetElement.RowHeaderResizeBar;
                        hitTestInfo.Row = lastHiddenRow;
                        y = bottomEdge;
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }
                }
                else
                {
                    // Standard single border between visible rows
                    if (point.Y >= bottomEdge - _resizeDelta && point.Y <= bottomEdge + _resizeDelta)
                    {
                        hitTestInfo.Element = SheetElement.RowHeaderResizeBar;
                        hitTestInfo.Row = row;
                        y = rowLocation;
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }
                }
            }

            // 3. Check visible row body hit
            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                if (!rows.IsRowVisible(row)) continue;

                var rowLocation = SheetView.ViewPort.GetRowLocation(row);
                double rowHeight = workSheet.Rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    break;
                }
            }

            for (int col = 0; col < workSheet.RowHeaders.ColumnCount; col++)
            {
                var colLocation = SheetView.ViewPort.GetHeaderColumnLocation(col);
                var sheetColumn = columns.GetItem(col);
                double columnWidth = sheetColumn == null ? workSheet.DefaultColumnWidth : sheetColumn.Width;

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
