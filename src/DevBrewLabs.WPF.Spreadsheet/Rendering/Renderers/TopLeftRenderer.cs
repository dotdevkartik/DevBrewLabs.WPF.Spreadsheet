using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class TopLeftRenderer : Renderer
    {
        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = SheetView.WorkSheet;
            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var width = SheetView.GetRowHeaderWidth() * zoom;
            var height = SheetView.GetColumnHeaderHeight() * zoom;
            var topLeft = workSheet.TopLeft;
            var style = workSheet.GetTopLeftStyle();

            double halfPenWidth = (SheetView.Spread.GridLinePen.Thickness * SheetView.Spread.PixelPerDip) / 2;
            var rect = new Rect(-SheetView.Spread.GridLinePen.Thickness, -SheetView.Spread.GridLinePen.Thickness, width, height);

            context.DrawRectangle(WpfResourceCache.GetBrush(style.BackColor), SheetView.Spread.GridLinePen, rect);
       
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(new PathFigure(new Point(5 * zoom, height - 5 * zoom), new PathSegment[]
            {
                new LineSegment(new Point(width - 5 * zoom, 5 * zoom), false),
                new LineSegment(new Point(width - 5 * zoom, height - 5 * zoom), false),
                new LineSegment(new Point(5 * zoom, height - 5 * zoom), false)
            }, true));

            context.DrawGeometry(WpfResourceCache.GetBrush(style.ForeColor), null, pathGeometry);
        }
    }
}



