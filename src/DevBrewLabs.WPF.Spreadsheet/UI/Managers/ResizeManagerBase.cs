using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal abstract class ResizeManagerBase : UIManager
    {
        public Line ResizeLine { get; }

        protected ResizeManagerBase(Spread spread) : base(spread)
        {
            ResizeLine = new Line
            {
                Visibility = Visibility.Collapsed
            };

            UpdateResizeMarkerStyle(spread.ResizeMarkerStyle);
        }

        public void UpdateResizeMarkerStyle(Style style)
        {
            if (style != null)
            {
                ResizeLine.Style = style;
            }
            else
            {
                ResizeLine.Style = null;
                ResizeLine.Stroke = Brushes.Black;
                ResizeLine.StrokeThickness = 0.75;
                ResizeLine.StrokeDashArray = new DoubleCollection(new double[] { 5, 2 });
            }
        }

        public abstract void BeginResize(SheetView sheetView, int index, int location);
        public abstract void Resize(SheetView sheetView, int currentLocation);
        public abstract void EndResize(SheetView sheetView);
        public abstract void CancelResize(SheetView sheetView);
    }
}
