using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class RowResizeManager : ResizeManagerBase
    {
        private int _rowLocation;
        private int _resizingRow;
        private int[] _initialHeights;

        public bool IsResizing => _rowLocation != -1 && _resizingRow != -1;

        public RowResizeManager(Spread spread) : base(spread)
        {
            _rowLocation = -1;
            _resizingRow = -1;
        }

        public override void BeginResize(SheetView sheetView, int row, int rowLocation)
        {
            _rowLocation = rowLocation;
            _resizingRow = row;

            var workSheet = sheetView.WorkSheet;

            _initialHeights = new int[workSheet.RowCount];
            for (int i = 0; i < workSheet.RowCount; i++)
            {
                _initialHeights[i] = workSheet.Rows.GetRowHeight(i);
            }

            Spread.SuspendUpdates = true;
        }

        public override void Resize(SheetView sheetView, int currentLocation)
        {
            var workSheet = sheetView.WorkSheet;

            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            double logicalCurrentLocation = currentLocation / zoom;

            if (logicalCurrentLocation < 0)
            {
                logicalCurrentLocation = 0;
                currentLocation = 0;
            }

            ResizeLine.Y1 = ResizeLine.Y2 = currentLocation;
            ResizeLine.X1 = sheetView.GetRowHeaderWidth() * zoom;
            ResizeLine.X2 = sheetView.Spread.SheetViewPane.ActualWidth;
            ResizeLine.Visibility = Visibility.Visible;

            var view = (SheetView)sheetView;

            if (_initialHeights == null || _resizingRow < 0 || _resizingRow >= workSheet.RowCount)
                return;

            view.ClearTemporaryRowHeights();

            if (logicalCurrentLocation >= _rowLocation)
            {
                var newHeight = (int)(logicalCurrentLocation - _rowLocation);
                view.SetTemporaryRowHeight(_resizingRow, newHeight);
            }
            else
            {
                view.SetTemporaryRowHeight(_resizingRow, 0);

                double currentTop = _rowLocation;
                int activeRow = -1;
                double activeRowTop = 0;

                for (int r = _resizingRow - 1; r >= 0; r--)
                {
                    double rowTop = currentTop - _initialHeights[r];
                    if (logicalCurrentLocation >= rowTop)
                    {
                        activeRow = r;
                        activeRowTop = rowTop;
                        break;
                    }
                    currentTop = rowTop;
                }

                if (activeRow != -1)
                {
                    for (int r = activeRow + 1; r < _resizingRow; r++)
                    {
                        view.SetTemporaryRowHeight(r, 0);
                    }

                    view.SetTemporaryRowHeight(activeRow, Math.Max(0, (int)(logicalCurrentLocation - activeRowTop)));
                }
                else
                {
                    for (int r = 0; r <= _resizingRow; r++)
                    {
                        view.SetTemporaryRowHeight(r, 0);
                    }
                }
            }

            sheetView.ViewPort.CalculateVisibleRange();
            Spread.Invalidate(true, false, false, false);
        }

        public override void EndResize(SheetView sheetView)
        {
            if (_initialHeights != null)
            {
                var workSheet = sheetView.WorkSheet;
                var view = (SheetView)sheetView;
                
                var action = new RowResizedAction { SheetView = sheetView };
                bool hasChanges = false;
                
                for (int i = 0; i < workSheet.RowCount; i++)
                {
                    int oldHeight = _initialHeights[i];
                    int newHeight = view.GetTemporaryRowHeight(i) ?? oldHeight;
                    if (oldHeight != newHeight)
                    {
                        workSheet.Rows[i].Height = newHeight;
                        action.OldHeights[i] = oldHeight;
                        action.NewHeights[i] = newHeight;
                        hasChanges = true;
                    }
                }
                
                if (hasChanges)
                {
                    Spread.UndoRedoManager.AddAction(action);
                }

                view.ClearTemporaryRowHeights();
            }

            _resizingRow = -1;
            _rowLocation = -1;
            _initialHeights = null;
            ResizeLine.Visibility = Visibility.Collapsed;
            Spread.SheetTabControl.UpdateScrollbars();
            Spread.SheetViewPane.RefreshInteractionLayers(true, false, true);
            Spread.SuspendUpdates = false;
        }
    }
}
