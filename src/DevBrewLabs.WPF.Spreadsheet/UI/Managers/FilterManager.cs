using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;
using DevBrewLabs.WPF.Spreadsheet.Components;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class FilterManager : IDisposable
    {
        private Spread _spread;
        private Popup _filterPopup;
        private FilterDropdown _filterDropdown;
        private int _activeFilterColumn = -1;
        private SheetView _activeSheetView;
        private Window _parentWindow;

        internal FilterManager(Spread spread)
        {
            _spread = spread;
        }

        internal bool IsFilterDropdownOpen => _filterPopup != null && _filterPopup.IsOpen;

        internal void ShowFilterDropdown(SheetView sheetView, int column)
        {
            if (IsFilterDropdownOpen)
            {
                HideFilterDropdown();
                if (_activeFilterColumn == column && _activeSheetView == sheetView)
                {
                    return; // Toggle off
                }
            }

            _activeSheetView = sheetView;
            _activeFilterColumn = column;

            if (_filterDropdown == null)
            {
                _filterDropdown = new FilterDropdown();
                _filterDropdown.Applied += OnFilterApplied;
                _filterDropdown.Cancelled += OnFilterCancelled;
                _filterDropdown.SortRequested += OnSortRequested;

                _filterPopup = new Popup
                {
                    Child = _filterDropdown,
                    Placement = PlacementMode.Bottom,
                    StaysOpen = false,
                    AllowsTransparency = true
                };

                _filterPopup.Closed += (s, e) => 
                {
                    DetachWindowEvents();
                    _activeFilterColumn = -1;
                    _activeSheetView = null;
                };
            }

            var unzoomedRect = sheetView.ViewPort.GetCellRect(sheetView.WorkSheet.AutoFilter.Range.TopRow, column);
            var zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            
            var x = (unzoomedRect.X - sheetView.ViewPort.LeftColumnLocation) * zoom;
            var y = (unzoomedRect.Y - sheetView.ViewPort.TopRowLocation) * zoom;
            
            var cellWidth = unzoomedRect.Width * zoom;
            var cellHeight = unzoomedRect.Height * zoom;

            _filterPopup.PlacementTarget = sheetView.CellsSurface;
            _filterPopup.PlacementRectangle = new Rect(x, y, cellWidth, cellHeight);

            // Fetch available values to initialize dropdown
            var availableValues = sheetView.WorkSheet.AutoFilter.GetAvailableValues(column);
            
            var columnFilter = sheetView.WorkSheet.AutoFilter.GetColumnFilter(column);
            _filterDropdown.Initialize(availableValues, columnFilter);

            const double shadowMarginLeft = 8;
            const double shadowMarginTop = 6;
            const double shadowMarginRight = 8;
            const double shadowMarginBottom = 14;

            _filterDropdown.LayoutTransform = Transform.Identity;

            _filterDropdown.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double dropdownWidth = _filterDropdown.DesiredSize.Width > 0 
                ? _filterDropdown.DesiredSize.Width 
                : (!double.IsNaN(_filterDropdown.Width) ? _filterDropdown.Width : 264);
            double dropdownHeight = _filterDropdown.DesiredSize.Height > 0 
                ? _filterDropdown.DesiredSize.Height 
                : 360;

            double contentHeight = dropdownHeight - (shadowMarginTop + shadowMarginBottom);

            // Vertical placement: flip to Top if bottom would overflow visible canvas and Top fits
            double surfaceHeight = sheetView.CellsSurface?.ActualHeight ?? 0;
            if (surfaceHeight > 0 && (y + cellHeight + contentHeight > surfaceHeight) && (y - contentHeight >= 0))
            {
                _filterPopup.Placement = PlacementMode.Top;
                _filterPopup.VerticalOffset = shadowMarginBottom;
            }
            else
            {
                _filterPopup.Placement = PlacementMode.Bottom;
                _filterPopup.VerticalOffset = -shadowMarginTop;
            }

            // Horizontal placement:
            // 1. Prefer right-aligning content box with cell right edge (where filter button is)
            // 2. If right-alignment overflows left boundary (< 0), align content box with left boundary
            double rightAlignedLeft = x + cellWidth - dropdownWidth + shadowMarginRight;
            if (rightAlignedLeft >= -shadowMarginLeft)
            {
                _filterPopup.HorizontalOffset = cellWidth - dropdownWidth + shadowMarginRight;
            }
            else
            {
                _filterPopup.HorizontalOffset = -x - shadowMarginLeft;
            }

            AttachWindowEvents(sheetView.CellsSurface);
            _filterPopup.IsOpen = true;
        }

        private void AttachWindowEvents(UIElement element)
        {
            DetachWindowEvents();

            _parentWindow = Window.GetWindow(element ?? _spread);
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged += OnWindowMovedOrResized;
                _parentWindow.SizeChanged += OnWindowMovedOrResized;
                _parentWindow.Deactivated += OnWindowDeactivated;
                _parentWindow.StateChanged += OnWindowMovedOrResized;
            }
        }

        private void DetachWindowEvents()
        {
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged -= OnWindowMovedOrResized;
                _parentWindow.SizeChanged -= OnWindowMovedOrResized;
                _parentWindow.Deactivated -= OnWindowDeactivated;
                _parentWindow.StateChanged -= OnWindowMovedOrResized;
                _parentWindow = null;
            }
        }

        private void OnWindowMovedOrResized(object sender, EventArgs e)
        {
            HideFilterDropdown();
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            HideFilterDropdown();
        }

        internal void HideFilterDropdown()
        {
            if (_filterPopup != null)
            {
                _filterPopup.IsOpen = false;
            }
            DetachWindowEvents();
        }

        private void OnFilterApplied(object sender, FilterApplyEventArgs e)
        {
            if (_activeSheetView != null && _activeFilterColumn >= 0)
            {
                var valListFilter = new ValueListFilter(e.SelectedValues);
                _activeSheetView.WorkSheet.AutoFilter.SetFilter(_activeFilterColumn, valListFilter);
            }

            HideFilterDropdown();
        }

        private void OnFilterCancelled(object sender, EventArgs e)
        {
            HideFilterDropdown();
        }

        private void OnSortRequested(object sender, SortRequestedEventArgs e)
        {
            if (_activeSheetView != null && _activeFilterColumn >= 0)
            {
                _activeSheetView.WorkSheet.AutoFilter.SortColumn(_activeFilterColumn, e.Ascending);
            }

            HideFilterDropdown();
        }

        public void Dispose()
        {
            HideFilterDropdown();
            if (_filterDropdown != null)
            {
                _filterDropdown.Applied -= OnFilterApplied;
                _filterDropdown.Cancelled -= OnFilterCancelled;
                _filterDropdown.SortRequested -= OnSortRequested;
                _filterDropdown = null;
            }
            _filterPopup = null;
            _spread = null;
        }
    }
}
