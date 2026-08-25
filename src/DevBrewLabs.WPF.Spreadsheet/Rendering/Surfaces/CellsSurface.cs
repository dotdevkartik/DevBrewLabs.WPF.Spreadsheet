using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class CellsSurface : SurfaceBase
    {
        private double _dragFillOffset;

        public CellsSurface(SheetView view) : base(view)
        {
            _dragFillOffset = 5;
        }

        protected override InteractionLayer CreateInteractionLayer()
        {
            return new CellsInteractionLayer(SheetView);
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
            var workSheet = (Worksheet)base.SheetView.WorkSheet;
            var viewPort = base.SheetView.ViewPort;

            var hitTestInfo = new SpreadHitTestResult() { Element = SheetElement.Cell, Row = -1, Column = -1, Sheet = SheetView };
            hitTestInfo.ActualHitTestPoint = hitPoint;
            var rows = workSheet.Rows.As<Rows>();
            var columns = workSheet.Columns.As<Columns>();
            var viewRange = viewPort.ViewRange;

            double zoom = SheetView != null && SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + viewPort.LeftColumnLocation, 
                hitPoint.Y / zoom + viewPort.TopRowLocation);

            // Fast path for DragFill handle hit test
            if (SheetView.Selection.RightColumn >= 0 && SheetView.Selection.BottomRow >= 0)
            {
                var brCellRect = viewPort.GetCellRect(SheetView.Selection.BottomRow, SheetView.Selection.RightColumn);
                if (Math.Abs(point.X - brCellRect.BottomRight.X) <= _dragFillOffset &&
                    Math.Abs(point.Y - brCellRect.BottomRight.Y) <= _dragFillOffset)
                {
                    hitTestInfo.Element = SheetElement.DragFill;
                    hitTestInfo.Row = SheetView.Selection.BottomRow;
                    hitTestInfo.Column = SheetView.Selection.RightColumn;
                    hitTestInfo.Position = new Point(brCellRect.X - viewPort.LeftColumnLocation, 
                        brCellRect.Y - viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            double x = 0, y = 0;

            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                var sheetRowObj = rows.GetItem(row);
                if (sheetRowObj != null && !sheetRowObj.Visible) continue;

                var rowLocation = SheetView.ViewPort.GetRowLocation(row);
                double rowHeight = workSheet.Rows.GetRowHeight(row);

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
                    {
                        var colLocation = SheetView.ViewPort.GetColumnLocation(col);
                        double columnWidth = workSheet.Columns.GetColumnWidth(col);

                        if (point.X >= colLocation && point.X < colLocation + columnWidth)
                        {
                            hitTestInfo.Column = col;
                            hitTestInfo.Row = row;
                            x = colLocation;
                            y = rowLocation;

                            var anchor = workSheet.GetSpanCellRange(hitTestInfo.Row, hitTestInfo.Column);
                            if (anchor != default)
                            {
                                hitTestInfo.Row = anchor.TopRow;
                                hitTestInfo.Column = anchor.LeftColumn;
                                var cellRect = viewPort.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
                                x = cellRect.X;
                                y = cellRect.Y;
                                columnWidth = cellRect.Width;
                                rowHeight = cellRect.Height;
                            }

                            var scaledCellRect = new Rect(
                                (x - viewPort.LeftColumnLocation) * zoom,
                                (y - viewPort.TopRowLocation) * zoom,
                                columnWidth * zoom,
                                rowHeight * zoom);

                            foreach (var element in SheetView.Spread.CellInteractionManager.GetCellElements(SheetView, hitTestInfo.Row, hitTestInfo.Column))
                            {
                                var elementBounds = element.GetBounds(scaledCellRect, zoom);
                                if (elementBounds.Contains(hitPoint))
                                {
                                    hitTestInfo.Element = SheetElement.CellElement;
                                    hitTestInfo.CellElement = element;
                                    break;
                                }
                            }

                            break;
                        }
                    }
                    break;
                }
            }
            
            hitTestInfo.Position = new Point(x - viewPort.LeftColumnLocation, 
                y - viewPort.TopRowLocation);
            return hitTestInfo;
        }
    }
}