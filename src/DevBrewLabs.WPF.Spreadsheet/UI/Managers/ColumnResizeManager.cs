using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class ColumnResizeManager : ResizeManagerBase
    {
        private int _columnLocation;
        private int _resizingColumn;
        private int[] _initialWidths;

        public bool IsResizing => _columnLocation != -1 && _resizingColumn != -1;

        public ColumnResizeManager(Spread spread) : base(spread)
        {
            _columnLocation = -1;
            _resizingColumn = -1;           
        }

        public override void BeginResize(SheetView sheetView, int column, int columnLocation)
        {
            _columnLocation = columnLocation;
            _resizingColumn = column;

            var workSheet = sheetView.WorkSheet;

            _initialWidths = new int[workSheet.ColumnCount];
            for (int i = 0; i < workSheet.ColumnCount; i++)
            {
                _initialWidths[i] = workSheet.Columns.GetColumnWidth(i);
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

            ResizeLine.X1 = ResizeLine.X2 = currentLocation;
            ResizeLine.Y1 = sheetView.GetColumnHeaderHeight() * zoom;
            ResizeLine.Y2 = sheetView.Spread.SheetViewPane.ActualHeight;
            ResizeLine.Visibility = Visibility.Visible;

            var view = (SheetView)sheetView;

            if (_initialWidths == null || _resizingColumn < 0 || _resizingColumn >= workSheet.ColumnCount)
                return;

            view.ViewPort.ClearTemporaryColumnWidths();

            if (logicalCurrentLocation >= _columnLocation)
            {
                var newWidth = (int)(logicalCurrentLocation - _columnLocation);
                view.ViewPort.SetTemporaryColumnWidth(_resizingColumn, newWidth);
            }
            else
            {
                view.ViewPort.SetTemporaryColumnWidth(_resizingColumn, 0);

                double currentLeft = _columnLocation;
                int activeCol = -1;
                double activeColLeft = 0;

                for (int c = _resizingColumn - 1; c >= 0; c--)
                {
                    double colLeft = currentLeft - _initialWidths[c];
                    if (logicalCurrentLocation >= colLeft)
                    {
                        activeCol = c;
                        activeColLeft = colLeft;
                        break;
                    }
                    currentLeft = colLeft;
                }

                if (activeCol != -1)
                {
                    for (int c = activeCol + 1; c < _resizingColumn; c++)
                    {
                        view.ViewPort.SetTemporaryColumnWidth(c, 0);
                    }

                    view.ViewPort.SetTemporaryColumnWidth(activeCol, Math.Max(0, (int)(logicalCurrentLocation - activeColLeft)));
                }
                else
                {
                    for (int c = 0; c <= _resizingColumn; c++)
                    {
                        view.ViewPort.SetTemporaryColumnWidth(c, 0);
                    }
                }
            }

            sheetView.ViewPort.CalculateVisibleRange();
            Spread.Invalidate(false, true, false, false);
        }

        public override void EndResize(SheetView sheetView)
        {
            if (_initialWidths != null)
            {
                var workSheet = sheetView.WorkSheet;
                var view = (SheetView)sheetView;
                
                var action = new ColumnResizedAction { SheetView = sheetView };
                bool hasChanges = false;
                
                for (int i = 0; i < workSheet.ColumnCount; i++)
                {
                    int oldWidth = _initialWidths[i];
                    int newWidth = view.ViewPort.GetTemporaryColumnWidth(i) ?? oldWidth;
                    if (oldWidth != newWidth)
                    {
                        workSheet.Columns[i].Width = newWidth;
                        action.OldWidths[i] = oldWidth;
                        action.NewWidths[i] = newWidth;
                        hasChanges = true;
                    }
                }
                
                if (hasChanges)
                {
                    Spread.UndoRedoManager.AddAction(action);
                }

                view.ViewPort.ClearTemporaryColumnWidths();
            }

            _resizingColumn = -1;
            _columnLocation = -1;
            _initialWidths = null;
            ResizeLine.Visibility = Visibility.Collapsed;
            Spread.SheetTabControl.UpdateScrollbars();
            Spread.SheetViewPane.RefreshInteractionLayers(false, true, true);
            Spread.SuspendUpdates = false;
        }

        public override void CancelResize(SheetView sheetView)
        {
            if (!IsResizing)
                return;

            var view = (SheetView)sheetView;

            // Discard any temporary widths — restores visual state to original
            view.ViewPort.ClearTemporaryColumnWidths();

            _resizingColumn = -1;
            _columnLocation = -1;
            _initialWidths = null;
            ResizeLine.Visibility = Visibility.Collapsed;
            Spread.SheetTabControl.UpdateScrollbars();
            sheetView.ViewPort.CalculateVisibleRange();
            Spread.SheetViewPane.RefreshInteractionLayers(false, true, true);
            Spread.SuspendUpdates = false;
        }
    }
}
