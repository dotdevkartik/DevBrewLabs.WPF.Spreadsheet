using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class FilterManager
    {
        private Spread _spread;
        private Popup _filterPopup;
        private Filtering.FilterDropdown _filterDropdown;
        private int _activeFilterColumn = -1;
        private SheetView _activeSheetView;

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
                _filterDropdown = new Filtering.FilterDropdown();
                _filterDropdown.Applied += OnFilterApplied;
                _filterDropdown.Cancelled += OnFilterCancelled;
                _filterDropdown.SortRequested += OnSortRequested;

                _filterPopup = new Popup
                {
                    Child = _filterDropdown,
                    Placement = PlacementMode.Bottom,
                    StaysOpen = false,
                    AllowsTransparency = true,
                    PopupAnimation = PopupAnimation.Fade
                };

                _filterPopup.Closed += (s, e) => 
                {
                    _activeFilterColumn = -1;
                    _activeSheetView = null;
                };
            }

            var unzoomedRect = sheetView.ViewPort.GetCellRect(sheetView.WorkSheet.AutoFilter.Range.TopRow, column);
            var zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
            
            var x = (unzoomedRect.X - sheetView.ViewPort.LeftColumnLocation) * zoom;
            var y = (unzoomedRect.Y - sheetView.ViewPort.TopRowLocation) * zoom;
            
            var rowHeaderWidth = sheetView.GetRowHeaderWidth() * zoom;
            var columnHeaderHeight = sheetView.GetColumnHeaderHeight() * zoom;

            _filterPopup.PlacementTarget = sheetView.CellsSurface;
            _filterPopup.PlacementRectangle = new Rect(x, y, unzoomedRect.Width * zoom, unzoomedRect.Height * zoom);
            _filterPopup.HorizontalOffset = 0;
            _filterPopup.VerticalOffset = 0;

            // Fetch available values to initialize dropdown
            var availableValues = sheetView.WorkSheet.AutoFilter.GetAvailableValues(column);
            
            var columnFilter = sheetView.WorkSheet.AutoFilter.GetColumnFilter(column);
            _filterDropdown.Initialize(availableValues, columnFilter);

            _filterPopup.IsOpen = true;
        }

        internal void HideFilterDropdown()
        {
            if (_filterPopup != null)
            {
                _filterPopup.IsOpen = false;
            }
        }

        private void OnFilterApplied(object sender, Filtering.FilterApplyEventArgs e)
        {
            if (_activeSheetView != null && _activeFilterColumn >= 0)
            {
                var valListFilter = new ValueListFilter(e.SelectedValues);
                _activeSheetView.WorkSheet.AutoFilter.SetFilter(_activeFilterColumn, valListFilter);
                _activeSheetView.Spread.InvalidateVisual();
            }
            HideFilterDropdown();
        }

        private void OnFilterCancelled(object sender, EventArgs e)
        {
            HideFilterDropdown();
        }

        private void OnSortRequested(object sender, Filtering.SortRequestedEventArgs e)
        {
            if (_activeSheetView != null && _activeFilterColumn >= 0)
            {
                _activeSheetView.WorkSheet.AutoFilter.SortColumn(_activeFilterColumn, e.Ascending);
                _activeSheetView.Spread.InvalidateVisual();
            }
            HideFilterDropdown();
        }
    }
}

