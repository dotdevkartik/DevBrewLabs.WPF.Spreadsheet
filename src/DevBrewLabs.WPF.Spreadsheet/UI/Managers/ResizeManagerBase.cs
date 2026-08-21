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
                Stroke = Brushes.Black,
                StrokeThickness = 0.75,
                StrokeDashArray = new DoubleCollection(new double[] { 5, 2 }),
                Visibility = Visibility.Collapsed
            };
        }

        public abstract void BeginResize(SheetView sheetView, int index, int location);
        public abstract void Resize(SheetView sheetView, int currentLocation);
        public abstract void EndResize(SheetView sheetView);
        public abstract void CancelResize(SheetView sheetView);
    }
}
