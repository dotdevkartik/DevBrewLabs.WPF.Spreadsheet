using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using System;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class ZoomManager : UIManager
    {
        public ZoomManager(Spread spread) : base(spread)
        {
        }

        public void ZoomIn()
        {
            if (!Spread.AllowZooming) return;
            SetZoom(Spread.ZoomFactor + 0.1);
        }

        public void ZoomOut()
        {
            if (!Spread.AllowZooming) return;
            SetZoom(Spread.ZoomFactor - 0.1);
        }

        public void SetZoom(double zoomFactor)
        {
            var clamped = Math.Max(0.1, Math.Min(4.0, Math.Round(zoomFactor, 2)));
            Spread.SetCurrentValue(Spread.ZoomFactorProperty, clamped);
        }

        public void HandleMouseWheel(MouseWheelEventArgs e)
        {
            if (!Spread.AllowZooming)
                return;

            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else if (e.Delta < 0)
            {
                ZoomOut();
            }
        }

        public void OnSpreadZoomFactorChanged(double oldZoom, double newZoom)
        {
            var activeSheetView = Spread.Sheets?.ActiveSheet as SheetView;
            if (activeSheetView != null)
            {
                if (Math.Abs(activeSheetView.ZoomFactor - newZoom) > 0.001)
                {
                    activeSheetView.InternalSetZoomFactor(newZoom);
                }

                Spread.FilterManager?.HideFilterDropdown();
                Spread.FormulaSuggestionManager?.Hide();
                Spread.RaiseZoomChanged(oldZoom, newZoom);
                Spread.UpdateZoomTransform();
                TextLayoutCache.Clear();
                Spread.Refresh();
            }
        }
    }
}
