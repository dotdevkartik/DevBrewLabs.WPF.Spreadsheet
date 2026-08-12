using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class CellsSurface : SheetViewSurface
    {
        private WorkSheet _workSheet;
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
            _workSheet = (WorkSheet)sheetView.WorkSheet;
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

            double x = 0, y = 0;

            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                var rowLocation = rows.GetLocation(row);
                double rowHeight = _workSheet.Rows.GetRowHeight(row);

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    hitTestInfo.Row = row;
                    y = rowLocation;
                    for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
                    {
                        var colLocation = columns.GetLocation(col);
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

                            if (sheetView.Selection.RightColumn == hitTestInfo.Column && sheetView.Selection.BottomRow == hitTestInfo.Row)
                            {
                                if (point.X > x + columnWidth - _dragFillOffset && point.Y > y + rowHeight - _dragFillOffset
                                    && point.X <= x + columnWidth && point.Y <= y + rowHeight)
                                    hitTestInfo.Element = VisualElement.DragFill;
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