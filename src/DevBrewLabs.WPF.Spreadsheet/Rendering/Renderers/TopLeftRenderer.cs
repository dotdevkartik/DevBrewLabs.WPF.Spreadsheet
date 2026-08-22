using DevBrewLabs.WPF.Spreadsheet.Styling;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class TopLeftRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var width = context.RowHeaderWidth;
            var height = context.ColumnHeaderHeight;
            var topLeft = context.Worksheet?.TopLeft;
            var style = context.Worksheet?.GetTopLeftStyle();

            var rect = new Rect(-context.GridLinePen.Thickness, -context.GridLinePen.Thickness, width, height);

            context.DrawRectangle(WpfResourceCache.GetBrush(style.BackColor), context.GridLinePen, rect);
       
            var pathGeometry = new PathGeometry();
            
            double margin = 3 * context.Zoom;
            double size = 10 * context.Zoom;

            pathGeometry.Figures.Add(new PathFigure(new Point(width - margin, height - margin), new PathSegment[]
            {
                new LineSegment(new Point(width - margin, height - margin - size), false),
                new LineSegment(new Point(width - margin - size, height - margin), false)
            }, true));

            context.DrawGeometry(style.ForeColor, null, pathGeometry);
        }
    }
}



