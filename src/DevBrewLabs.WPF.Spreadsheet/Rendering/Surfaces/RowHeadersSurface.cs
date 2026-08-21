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

            var hitTestInfo = new SpreadHitTestResult() { Element = VisualElement.RowHeader, Sheet = SheetView, ActualHitTestPoint = hitPoint };
            var rows = workSheet.Rows.As<Rows>();
            var columns = workSheet.RowHeaders.Columns.As<RowHeaderColumns>();
            var viewRange = SheetView.ViewPort.ViewRange;

            double zoom = SheetView != null && SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + viewPort.LeftColumnLocation,
                hitPoint.Y / zoom + viewPort.TopRowLocation);

            double x = 0, y = 0;

            // 1. Check visible row resize boundaries (centered around bottom edge)
            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                if (!rows.IsRowVisible(row)) continue;

                var rowLocation = SheetView.ViewPort.GetRowLocation(row);
                double rowHeight = workSheet.Rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                double bottomEdge = rowLocation + rowHeight;
                if (point.Y >= bottomEdge - _resizeDelta && point.Y <= bottomEdge + _resizeDelta)
                {
                    hitTestInfo.Element = VisualElement.RowHeaderResizeBar;
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                        y - viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            // 2. Check for manually hidden row resize handle (expand/unhide)
            int startRowSearch = Math.Max(0, viewPort.ViewRange.TopRow - 1);
            int endRowSearch = Math.Min(workSheet.RowCount, viewPort.ViewRange.BottomRow + 2);

            for (int row = startRowSearch; row < endRowSearch; row++)
            {
                var rowObj = rows.GetItem(row);
                bool isManuallyHidden = !rows.IsRowVisible(row) && (rowObj == null || !rowObj.IsFilteredOut);
                if (isManuallyHidden)
                {
                    int startHiddenRow = row;
                    int lastHiddenRow = row;
                    while (lastHiddenRow + 1 < workSheet.RowCount)
                    {
                        var nextRowObj = rows.GetItem(lastHiddenRow + 1);
                        if (!rows.IsRowVisible(lastHiddenRow + 1) && (nextRowObj == null || !nextRowObj.IsFilteredOut))
                        {
                            lastHiddenRow++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var rowLocation = SheetView.ViewPort.GetRowLocation(startHiddenRow);
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
                        hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation,
                            y - viewPort.TopRowLocation);
                        return hitTestInfo;
                    }

                    row = lastHiddenRow;
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
