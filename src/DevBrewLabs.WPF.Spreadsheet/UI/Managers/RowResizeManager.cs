using System;
using System.Windows;
using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class RowResizeManager : ResizeManagerBase
    {
        private int _rowLocation;
        private int _resizingRow;
        private int _initialHeight;
        private int _resizedHeight;

        public bool IsResizing => _rowLocation != -1 && _resizingRow != -1;

        public RowResizeManager(Spread spread) : base(spread)
        {
            _rowLocation = -1;
            _resizingRow = -1;
            _initialHeight = -1;
            _resizedHeight = -1;
        }

        public override void BeginResize(SheetView sheetView, int row, int rowLocation)
        {
            _rowLocation = rowLocation;
            _resizingRow = row;
            _initialHeight = sheetView.WorkSheet.Rows.GetRowHeight(row);
            _resizedHeight = _initialHeight;
            Spread.SuspendUpdates = true;
        }

        public override void Resize(SheetView sheetView, int currentLocation)
        {
            var workSheet = sheetView.WorkSheet;

            if (_resizingRow < 0 || _resizingRow >= workSheet.RowCount)
                return;

            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            double minVisualLocation = _rowLocation * zoom;
            double maxVisualLocation = (sheetView.ViewPort.ActualBounds.Bottom - 3) * zoom;
            double visualLocation = Math.Min(Math.Max(minVisualLocation, currentLocation), maxVisualLocation);

            if (ResizeLine != null)
            {
                ResizeLine.Y1 = ResizeLine.Y2 = visualLocation;
                ResizeLine.X1 = 0;
                ResizeLine.X2 = sheetView.Spread.ActualWidth;
                ResizeLine.Visibility = Visibility.Visible;
            }

            double logicalCurrentLocation = visualLocation / zoom;
            _resizedHeight = Math.Max(0, (int)Math.Round(logicalCurrentLocation - _rowLocation));
        }

        public override void EndResize(SheetView sheetView)
        {
            if (_resizingRow != -1 && _resizedHeight >= 0)
            {
                var workSheet = sheetView.WorkSheet;
                int oldHeight = _initialHeight;
                int newHeight = _resizedHeight;

                if (oldHeight != newHeight || (!workSheet.Rows.IsRowVisible(_resizingRow) && newHeight > 0))
                {
                    var rowObj = workSheet.Rows[_resizingRow];
                    if (rowObj != null && newHeight > 0)
                    {
                        if (rowObj.IsHidden)
                        {
                            rowObj.IsHidden = false;
                        }
                        if (rowObj.IsFilteredOut)
                        {
                            rowObj.IsFilteredOut = false;
                        }
                    }
                    workSheet.Rows[_resizingRow].Height = newHeight;

                    var action = new RowResizedAction { SheetView = sheetView };
                    action.OldHeights[_resizingRow] = oldHeight;
                    action.NewHeights[_resizingRow] = newHeight;
                    Spread.UndoRedoManager.AddAction(action);
                }
            }

            _resizingRow = -1;
            _rowLocation = -1;
            _initialHeight = -1;
            _resizedHeight = -1;
            if (ResizeLine != null)
            {
                ResizeLine.Visibility = Visibility.Collapsed;
            }
            Spread.SheetTabControl?.UpdateScrollbars();
            Spread.SheetViewHost?.RefreshInteractionLayers(true, false, true);
            Spread.SuspendUpdates = false;
        }

        public override void CancelResize(SheetView sheetView)
        {
            if (!IsResizing)
                return;

            _resizingRow = -1;
            _rowLocation = -1;
            _initialHeight = -1;
            _resizedHeight = -1;
            if (ResizeLine != null)
            {
                ResizeLine.Visibility = Visibility.Collapsed;
            }
            Spread.SheetTabControl?.UpdateScrollbars();
            sheetView.ViewPort.CalculateVisibleRange();
            Spread.SheetViewHost?.RefreshInteractionLayers(true, false, true);
            Spread.SuspendUpdates = false;
        }
    }
}
