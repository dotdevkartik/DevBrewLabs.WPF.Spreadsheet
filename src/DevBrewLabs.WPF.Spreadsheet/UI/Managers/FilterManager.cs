using System;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;
using DevBrewLabs.WPF.Spreadsheet.Components;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class FilterManager : UIManager
    {
        private FilterDropdown _filterDropdown;
        private int _activeFilterColumn = -1;
        private SheetView _activeSheetView;

        internal FilterManager(Spread spread) : base(spread)
        {
        }

        internal bool IsFilterDropdownOpen => Spread?.PopupManager != null && 
                                              Spread.PopupManager.IsPopupOpen && 
                                              Spread.PopupManager.CurrentContent == _filterDropdown;

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
                _filterDropdown.ClearFilterRequested += OnClearFilterRequested;
            }

            // Fetch available values to initialize dropdown
            var availableValues = sheetView.WorkSheet.AutoFilter.GetAvailableValues(column);
            var columnFilter = sheetView.WorkSheet.AutoFilter.GetColumnFilter(column);
            _filterDropdown.Initialize(availableValues, columnFilter);

            Spread.PopupManager.ShowForCell(
                sheetView,
                sheetView.WorkSheet.AutoFilter.Range.TopRow,
                column,
                _filterDropdown,
                new PopupPlacementOptions
                {
                    Alignment = PopupAlignment.Right,
                    AutoFlip = true,
                    UseStandardContainer = false // FilterDropdown provides its own template container
                });
        }

        internal void HideFilterDropdown()
        {
            if (IsFilterDropdownOpen)
            {
                Spread?.PopupManager?.ClosePopup();
            }
            _activeFilterColumn = -1;
            _activeSheetView = null;
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

        private void OnClearFilterRequested(object sender, EventArgs e)
        {
            if (_activeSheetView != null && _activeFilterColumn >= 0)
            {
                _activeSheetView.WorkSheet.AutoFilter.ClearFilter(_activeFilterColumn);
            }

            HideFilterDropdown();
        }

        public override void Dispose()
        {
            HideFilterDropdown();
            if (_filterDropdown != null)
            {
                _filterDropdown.Applied -= OnFilterApplied;
                _filterDropdown.Cancelled -= OnFilterCancelled;
                _filterDropdown.SortRequested -= OnSortRequested;
                _filterDropdown.ClearFilterRequested -= OnClearFilterRequested;
                _filterDropdown = null;
            }
            _activeSheetView = null;
            base.Dispose();
        }
    }
}
