using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class CellsSurface : SheetViewSurface
    {
        private Worksheet _workSheet;
        private DrawingGroup _drawing;
        private ViewPort _viewPort;
        private double _dragFillOffset;

        public CellsSurface()
        {
            _drawing = new DrawingGroup();
            _dragFillOffset = 5;
        }

        public override void AttachSheet(SheetView sheetView)
        {
            base.AttachSheet(sheetView);
            _workSheet = (Worksheet)sheetView.WorkSheet;
            _drawing.Children.Clear();
            _drawing.Children.Add(sheetView.Spread.RenderEngine.CellsRenderer.Drawing);
            _drawing.Children.Add(sheetView.Spread.RenderEngine.GridLinesRenderer.Drawing);

            _viewPort = sheetView.ViewPort.As<ViewPort>();
        }

        protected override Drawing GetDrawing()
        {
            return _drawing;
        }

        protected override SpreadHitTestResult HitTestCore(SheetView sheetView, Point hitPoint)
        {
            var hitTestInfo = new SpreadHitTestResult() { Element = VisualElement.Cell, Row = -1, Column = -1, Sheet = sheetView };
            hitTestInfo.ActualHitTestPoint = hitPoint;
            var rows = _workSheet.Rows.As<Rows>();
            var columns = _workSheet.Columns.As<Columns>();
            var viewRange = _viewPort.ViewRange;

            double zoom = sheetView != null && sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            var point = new Point(hitPoint.X / zoom + _viewPort.LeftColumnLocation, 
                hitPoint.Y / zoom + _viewPort.TopRowLocation);

            // Fast path for DragFill handle hit test
            if (sheetView.Selection.RightColumn >= 0 && sheetView.Selection.BottomRow >= 0)
            {
                var brCellRect = _viewPort.GetCellRect(sheetView.Selection.BottomRow, sheetView.Selection.RightColumn);
                if (Math.Abs(point.X - brCellRect.BottomRight.X) <= _dragFillOffset &&
                    Math.Abs(point.Y - brCellRect.BottomRight.Y) <= _dragFillOffset)
                {
                    hitTestInfo.Element = VisualElement.DragFill;
                    hitTestInfo.Row = sheetView.Selection.BottomRow;
                    hitTestInfo.Column = sheetView.Selection.RightColumn;
                    hitTestInfo.Position = new Point(brCellRect.X - _viewPort.LeftColumnLocation, 
                        brCellRect.Y - _viewPort.TopRowLocation);
                    return hitTestInfo;
                }
            }

            double x = 0, y = 0;

            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                var rowLocation = sheetView.ViewPort.GetRowLocation(row);
                double rowHeight = _workSheet.Rows.GetRowHeight(row);

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
                    {
                        var colLocation = sheetView.ViewPort.GetColumnLocation(col);
                        double columnWidth = _workSheet.Columns.GetColumnWidth(col);

                        if (point.X >= colLocation && point.X < colLocation + columnWidth)
                        {
                            hitTestInfo.Column = col;
                            hitTestInfo.Row = row;
                            x = colLocation;
                            y = rowLocation;

                            var anchor = _workSheet.GetSpanCellRange(hitTestInfo.Row, hitTestInfo.Column);
                            if (anchor != default)
                            {
                                hitTestInfo.Row = anchor.TopRow;
                                hitTestInfo.Column = anchor.LeftColumn;
                                var cellRect = _viewPort.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
                                x = cellRect.X;
                                y = cellRect.Y;
                                columnWidth = cellRect.Width;
                                rowHeight = cellRect.Height;
                            }

                            break;
                        }
                    }
                    break;
                }
            }
            
            hitTestInfo.Position = new Point(x - _viewPort.LeftColumnLocation, 
                y - _viewPort.TopRowLocation);
            return hitTestInfo;
        }
    }
}