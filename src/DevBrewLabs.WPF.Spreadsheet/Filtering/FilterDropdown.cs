using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;

namespace DevBrewLabs.WPF.Spreadsheet.Filtering
{
    public class FilterApplyEventArgs : EventArgs
    {
        public HashSet<object> SelectedValues { get; }

        public FilterApplyEventArgs(HashSet<object> selectedValues)
        {
            SelectedValues = selectedValues;
        }
    }

    public class SortRequestedEventArgs : EventArgs
    {
        public bool Ascending { get; }

        public SortRequestedEventArgs(bool ascending)
        {
            Ascending = ascending;
        }
    }

    internal class FilterDropdown : Control
    {
        public event EventHandler<FilterApplyEventArgs> Applied;
        public event EventHandler Cancelled;
        public event EventHandler<SortRequestedEventArgs> SortRequested;

        private TextBox _searchBox;
        private ListBox _valuesList;
        private CheckBox _selectAllCheckBox;
        private Button _sortAscendingBtn;
        private Button _sortDescendingBtn;
        private Button _applyBtn;
        private Button _cancelBtn;

        private IReadOnlyList<object> _availableValues;
        private HashSet<object> _currentSelected;
        private bool _isUpdatingList;

        public FilterDropdown()
        {
            DefaultStyleKey = typeof(FilterDropdown);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _searchBox = GetTemplateChild("PART_SearchBox") as TextBox;
            if (_searchBox != null)
                _searchBox.TextChanged += SearchBox_TextChanged;

            _valuesList = GetTemplateChild("PART_ValuesList") as ListBox;

            _selectAllCheckBox = GetTemplateChild("PART_SelectAll") as CheckBox;
            if (_selectAllCheckBox != null)
            {
                _selectAllCheckBox.Checked += SelectAll_Changed;
                _selectAllCheckBox.Unchecked += SelectAll_Changed;
            }

            _sortAscendingBtn = GetTemplateChild("PART_SortAscending") as Button;
            if (_sortAscendingBtn != null)
                _sortAscendingBtn.Click += (s, e) => SortRequested?.Invoke(this, new SortRequestedEventArgs(true));

            _sortDescendingBtn = GetTemplateChild("PART_SortDescending") as Button;
            if (_sortDescendingBtn != null)
                _sortDescendingBtn.Click += (s, e) => SortRequested?.Invoke(this, new SortRequestedEventArgs(false));

            _applyBtn = GetTemplateChild("PART_ApplyButton") as Button;
            if (_applyBtn != null)
                _applyBtn.Click += (s, e) => Applied?.Invoke(this, new FilterApplyEventArgs(new HashSet<object>(_currentSelected)));

            _cancelBtn = GetTemplateChild("PART_CancelButton") as Button;
            if (_cancelBtn != null)
                _cancelBtn.Click += (s, e) => Cancelled?.Invoke(this, EventArgs.Empty);
                
            UpdateList();
        }

        public void Initialize(IReadOnlyList<object> availableValues, ColumnFilter currentFilter)
        {
            _availableValues = availableValues;
            _currentSelected = new HashSet<object>();

            if (currentFilter != null && currentFilter.IsFiltered)
            {
                // Find the ValueListFilter if any
                var valFilter = currentFilter.Conditions.OfType<ValueListFilter>().FirstOrDefault();
                if (valFilter != null)
                {
                    foreach (var val in valFilter.AllowedValues)
                        _currentSelected.Add(val);
                }
                else
                {
                    // For now, if there's a custom filter, just select all
                    foreach (var val in _availableValues)
                        _currentSelected.Add(val);
                }
            }
            else
            {
                if (_availableValues != null)
                {
                    foreach (var val in _availableValues)
                        _currentSelected.Add(val);
                }
            }

            if (_searchBox != null)
                _searchBox.Text = "";

            UpdateList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateList();
        }

        private void SelectAll_Changed(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingList) return;

            bool isChecked = _selectAllCheckBox.IsChecked == true;
            
            var items = GetFilteredValues();
            foreach(var item in items)
            {
                if (isChecked)
                    _currentSelected.Add(item.Value);
                else
                    _currentSelected.Remove(item.Value);
            }

            UpdateList();
        }

        private IEnumerable<FilterItem> GetFilteredValues()
        {
            if (_availableValues == null) yield break;

            string searchText = _searchBox?.Text ?? "";

            foreach (var val in _availableValues)
            {
                if (string.IsNullOrEmpty(searchText) || (val != null && val.ToString().IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0))
                {
                    yield return new FilterItem(val, _currentSelected.Contains(val), this);
                }
            }
        }

        private void UpdateList()
        {
            if (_valuesList == null) return;
            _isUpdatingList = true;

            var items = GetFilteredValues().ToList();
            _valuesList.ItemsSource = items;

            if (_selectAllCheckBox != null)
            {
                if (items.Count == 0)
                    _selectAllCheckBox.IsChecked = false;
                else if (items.All(x => x.IsSelected))
                    _selectAllCheckBox.IsChecked = true;
                else if (items.Any(x => x.IsSelected))
                    _selectAllCheckBox.IsChecked = null;
                else
                    _selectAllCheckBox.IsChecked = false;
            }

            _isUpdatingList = false;
        }

        internal void ItemCheckChanged(FilterItem item)
        {
            if (_isUpdatingList) return;

            if (item.IsSelected)
                _currentSelected.Add(item.Value);
            else
                _currentSelected.Remove(item.Value);

            _isUpdatingList = true;
            if (_selectAllCheckBox != null)
            {
                var items = _valuesList.ItemsSource as List<FilterItem>;
                if (items != null)
                {
                    if (items.All(x => x.IsSelected))
                        _selectAllCheckBox.IsChecked = true;
                    else if (items.Any(x => x.IsSelected))
                        _selectAllCheckBox.IsChecked = null;
                    else
                        _selectAllCheckBox.IsChecked = false;
                }
            }
            _isUpdatingList = false;
        }
    }

    internal class FilterItem
    {
        private FilterDropdown _parent;
        public object Value { get; }
        public string DisplayText => Value?.ToString() ?? "(Blanks)";
        
        private bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    _parent.ItemCheckChanged(this);
                }
            }
        }

        public FilterItem(object value, bool isSelected, FilterDropdown parent)
        {
            Value = value;
            _isSelected = isSelected;
            _parent = parent;
        }
    }
}

