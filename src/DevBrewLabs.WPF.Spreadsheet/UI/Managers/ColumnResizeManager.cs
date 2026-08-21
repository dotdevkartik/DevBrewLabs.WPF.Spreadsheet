using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class ColumnResizeManager : ResizeManagerBase
    {
        private int _columnLocation;
        private int _resizingColumn;
        private int _initialWidth;
        private int _resizedWidth;

        public bool IsResizing => _columnLocation != -1 && _resizingColumn != -1;

        public ColumnResizeManager(Spread spread) : base(spread)
        {
            _columnLocation = -1;
            _resizingColumn = -1;
            _initialWidth = -1;
            _resizedWidth = -1;
        }

        public override void BeginResize(SheetView sheetView, int column, int columnLocation)
        {
            _columnLocation = columnLocation;
            _resizingColumn = column;
            _initialWidth = sheetView.WorkSheet.Columns.GetColumnWidth(column);
            _resizedWidth = _initialWidth;
            Spread.SuspendUpdates = true;
        }

        public override void Resize(SheetView sheetView, int currentLocation)
        {
            var workSheet = sheetView.WorkSheet;
   
            if (_resizingColumn < 0 || _resizingColumn >= workSheet.ColumnCount)
                return;

            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            double minVisualLocation = _columnLocation * zoom;
            double maxVisualLocation = (sheetView.ViewPort.ActualBounds.Right - 3) * zoom;
            double visualLocation = Math.Min(Math.Max(minVisualLocation, currentLocation), maxVisualLocation);

            if (ResizeLine != null)
            {
                ResizeLine.X1 = ResizeLine.X2 = visualLocation;
                ResizeLine.Y1 = 0;
                ResizeLine.Y2 = sheetView.Spread.ActualHeight;
                ResizeLine.Visibility = Visibility.Visible;
            }

            double logicalCurrentLocation = visualLocation / zoom;
            _resizedWidth = Math.Max(0, (int)Math.Round(logicalCurrentLocation - _columnLocation));
        }

        public override void EndResize(SheetView sheetView)
        {
            if (_resizingColumn != -1 && _resizedWidth >= 0)
            {
                var workSheet = sheetView.WorkSheet;
                int oldWidth = _initialWidth;
                int newWidth = _resizedWidth;

                if (oldWidth != newWidth)
                {
                    workSheet.Columns[_resizingColumn].Width = newWidth;

                    var action = new ColumnResizedAction { SheetView = sheetView };
                    action.OldWidths[_resizingColumn] = oldWidth;
                    action.NewWidths[_resizingColumn] = newWidth;
                    Spread.UndoRedoManager.AddAction(action);
                }
            }

            _resizingColumn = -1;
            _columnLocation = -1;
            _initialWidth = -1;
            _resizedWidth = -1;
            if (ResizeLine != null)
            {
                ResizeLine.Visibility = Visibility.Collapsed;
            }
            Spread.SheetTabControl?.UpdateScrollbars();
            Spread.SheetViewHost?.RefreshInteractionLayers(false, true, true);
            Spread.SuspendUpdates = false;
        }

        public override void CancelResize(SheetView sheetView)
        {
            if (!IsResizing)
                return;

            _resizingColumn = -1;
            _columnLocation = -1;
            _initialWidth = -1;
            _resizedWidth = -1;
            if (ResizeLine != null)
            {
                ResizeLine.Visibility = Visibility.Collapsed;
            }
            Spread.SheetTabControl?.UpdateScrollbars();
            sheetView.ViewPort.CalculateVisibleRange();
            Spread.SheetViewHost?.RefreshInteractionLayers(false, true, true);
            Spread.SuspendUpdates = false;
        }
    }
}
