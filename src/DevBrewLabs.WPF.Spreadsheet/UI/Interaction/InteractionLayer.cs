using DevBrewLabs.WPF.Spreadsheet.Rendering;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal abstract class InteractionLayer : Canvas
    {
        protected SheetView SheetView { get; private set; }

        public InteractionLayer(SheetView view)
        {
            SheetView = view;
            Background = Brushes.Transparent;
            Focusable = true;
            FocusVisualStyle = null;
        }

        /// <summary>
        /// Hittest this layer at the current mouse point.
        /// </summary>
        /// <returns></returns>
        protected SpreadHitTestResult HitTest()
        {
            return HitTest(Mouse.GetPosition(SheetView.Spread));
        }

        protected SpreadHitTestResult HitTest(Point point)
        {
            return SheetView.Spread.HitTest(point);
        }

        protected Rect ToSheetViewRect(Rect rect)
        {
            var viewPort = SheetView.ViewPort.As<ViewPort>();
            rect.X -= viewPort.LeftColumnLocation;
            rect.Y -= viewPort.TopRowLocation;
            return rect;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focus();
            CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
        }
    }
}
