using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    /// <summary>
    /// Represents the dropdown list control for ComboBox cell types and spreadsheet dropdown menus.
    /// </summary>
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    public class SpreadDropDownListBox : Control
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(SpreadDropDownListBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(SpreadDropDownListBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(SpreadDropDownListBox),
                new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                nameof(SelectedValue),
                typeof(object),
                typeof(SpreadDropDownListBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(
                nameof(SelectedValuePath),
                typeof(string),
                typeof(SpreadDropDownListBox),
                new PropertyMetadata(string.Empty, OnSelectedValuePathChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(SpreadDropDownListBox),
                new PropertyMetadata(string.Empty, OnDisplayMemberPathChanged));

        public static readonly DependencyProperty SearchMemberPathProperty =
            DependencyProperty.Register(
                nameof(SearchMemberPath),
                typeof(string),
                typeof(SpreadDropDownListBox),
                new PropertyMetadata(string.Empty, OnSearchMemberPathChanged));

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(SpreadDropDownListBox),
                new PropertyMetadata(null, OnItemTemplateChanged));

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                nameof(ItemContainerStyle),
                typeof(Style),
                typeof(SpreadDropDownListBox),
                new PropertyMetadata(null, OnItemContainerStyleChanged));

        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SelectionChanged),
                RoutingStrategy.Bubble,
                typeof(SelectionChangedEventHandler),
                typeof(SpreadDropDownListBox));

        public static readonly RoutedEvent SelectionCommittedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SelectionCommitted),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SpreadDropDownListBox));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        public event RoutedEventHandler SelectionCommitted
        {
            add => AddHandler(SelectionCommittedEvent, value);
            remove => RemoveHandler(SelectionCommittedEvent, value);
        }

        static SpreadDropDownListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SpreadDropDownListBox),
                new FrameworkPropertyMetadata(typeof(SpreadDropDownListBox)));
        }

        private ListBox _listBox;
        private bool _isSyncingSelection;

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public object SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public string SearchMemberPath
        {
            get => (string)GetValue(SearchMemberPathProperty);
            set => SetValue(SearchMemberPathProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public Style ItemContainerStyle
        {
            get => (Style)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        public ItemCollection Items => _listBox?.Items;

        public int ItemsCount
        {
            get
            {
                if (_listBox?.Items != null && _listBox.Items.Count > 0)
                    return _listBox.Items.Count;

                if (ItemsSource is ICollection col)
                    return col.Count;

                if (ItemsSource is IEnumerable enumerable)
                {
                    int count = 0;
                    var enumerator = enumerable.GetEnumerator();
                    while (enumerator.MoveNext()) count++;
                    return count;
                }

                return 0;
            }
        }

        public ListBox InnerListBox => _listBox;

        public SpreadDropDownListBox()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_listBox != null)
            {
                _listBox.SelectionChanged -= OnListBoxSelectionChanged;
                _listBox.PreviewMouseLeftButtonUp -= OnListBoxPreviewMouseLeftButtonUp;
            }

            _listBox = GetTemplateChild("PART_ListBox") as ListBox;

            if (_listBox != null)
            {
                _listBox.ItemsSource = ItemsSource;
                _listBox.DisplayMemberPath = DisplayMemberPath;
                _listBox.SelectedValuePath = SelectedValuePath;
                _listBox.ItemTemplate = ItemTemplate;
                if (ItemContainerStyle != null)
                    _listBox.ItemContainerStyle = ItemContainerStyle;

                if (SelectedIndex >= 0)
                    _listBox.SelectedIndex = SelectedIndex;
                else if (SelectedItem != null)
                    _listBox.SelectedItem = SelectedItem;

                _listBox.SelectionChanged += OnListBoxSelectionChanged;
                _listBox.PreviewMouseLeftButtonUp += OnListBoxPreviewMouseLeftButtonUp;
            }
        }

        private void OnListBoxPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dep = e.OriginalSource as DependencyObject;
            while (dep != null && !(dep is ListBoxItem))
            {
                if (dep is ListBox) break;
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is ListBoxItem item && item.DataContext != null)
            {
                SelectedItem = item.DataContext;
                CommitSelection();
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listBox == null)
                return;

            if (!_isSyncingSelection)
            {
                _isSyncingSelection = true;
                try
                {
                    SelectedIndex = _listBox.SelectedIndex;
                    SelectedItem = _listBox.SelectedItem;
                    SelectedValue = _listBox.SelectedValue;
                    if (_listBox.SelectedItem != null)
                        _listBox.ScrollIntoView(_listBox.SelectedItem);
                }
                finally
                {
                    _isSyncingSelection = false;
                }
            }

            var args = new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems)
            {
                Source = this
            };
            RaiseEvent(args);
        }

        private void RaiseSelectionChangedEvent(object removedItem, object addedItem)
        {
            var removed = removedItem != null ? new object[] { removedItem } : Array.Empty<object>();
            var added = addedItem != null ? new object[] { addedItem } : Array.Empty<object>();
            var args = new SelectionChangedEventArgs(SelectionChangedEvent, removed, added)
            {
                Source = this
            };
            RaiseEvent(args);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._listBox != null)
            {
                control._listBox.ItemsSource = (IEnumerable)e.NewValue;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._isSyncingSelection) return;

            control._isSyncingSelection = true;
            try
            {
                object newItem = e.NewValue;
                object oldItem = e.OldValue;

                if (control._listBox != null)
                {
                    control._listBox.SelectedItem = newItem;
                    control.SelectedIndex = control._listBox.SelectedIndex;
                    control.SelectedValue = control._listBox.SelectedValue;
                    if (newItem != null)
                        control._listBox.ScrollIntoView(newItem);
                }
                else
                {
                    control.SelectedIndex = control.GetIndexOfItem(newItem);
                    control.SelectedValue = control.GetItemValue(newItem);
                    control.RaiseSelectionChangedEvent(oldItem, newItem);
                }
            }
            finally
            {
                control._isSyncingSelection = false;
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._isSyncingSelection) return;

            control._isSyncingSelection = true;
            try
            {
                int newIndex = (int)e.NewValue;
                object oldItem = control.SelectedItem;

                if (control._listBox != null)
                {
                    control._listBox.SelectedIndex = newIndex;
                    control.SelectedItem = control._listBox.SelectedItem;
                    control.SelectedValue = control._listBox.SelectedValue;
                    if (control._listBox.SelectedItem != null)
                        control._listBox.ScrollIntoView(control._listBox.SelectedItem);
                }
                else
                {
                    var newItem = control.GetItemAtIndex(newIndex);
                    control.SelectedItem = newItem;
                    control.SelectedValue = control.GetItemValue(newItem);
                    control.RaiseSelectionChangedEvent(oldItem, newItem);
                }
            }
            finally
            {
                control._isSyncingSelection = false;
            }
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._isSyncingSelection) return;

            control._isSyncingSelection = true;
            try
            {
                object newVal = e.NewValue;
                if (control._listBox != null)
                {
                    control._listBox.SelectedValue = newVal;
                    control.SelectedItem = control._listBox.SelectedItem;
                    control.SelectedIndex = control._listBox.SelectedIndex;
                }
                else
                {
                    control.SelectItemByValue(newVal);
                }
            }
            finally
            {
                control._isSyncingSelection = false;
            }
        }

        private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._listBox != null)
            {
                control._listBox.SelectedValuePath = (string)e.NewValue;
            }
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._listBox != null)
            {
                control._listBox.DisplayMemberPath = (string)e.NewValue;
            }
        }

        private static void OnSearchMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
        }

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._listBox != null)
            {
                control._listBox.ItemTemplate = (DataTemplate)e.NewValue;
            }
        }

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SpreadDropDownListBox)d;
            if (control._listBox != null)
            {
                control._listBox.ItemContainerStyle = (Style)e.NewValue;
            }
        }

        public object GetItemAtIndex(int index)
        {
            if (index < 0 || ItemsSource == null) return null;

            if (ItemsSource is IList list)
            {
                if (index < list.Count)
                    return list[index];
                return null;
            }

            int curr = 0;
            foreach (var item in ItemsSource)
            {
                if (curr == index) return item;
                curr++;
            }

            return null;
        }

        public int GetIndexOfItem(object item)
        {
            if (item == null || ItemsSource == null) return -1;

            if (ItemsSource is IList list)
            {
                return list.IndexOf(item);
            }

            int curr = 0;
            foreach (var elem in ItemsSource)
            {
                if (Equals(elem, item)) return curr;
                curr++;
            }

            return -1;
        }

        public void MoveSelection(int delta)
        {
            int count = ItemsCount;
            if (count == 0) return;

            int newIndex;
            if (SelectedIndex < 0)
            {
                newIndex = delta > 0 ? 0 : count - 1;
            }
            else
            {
                newIndex = SelectedIndex + delta;
                if (newIndex < 0) newIndex = 0;
                if (newIndex >= count) newIndex = count - 1;
            }

            SelectedIndex = newIndex;
        }

        public void MoveSelectionFirst()
        {
            if (ItemsCount > 0)
                SelectedIndex = 0;
        }

        public void MoveSelectionLast()
        {
            int count = ItemsCount;
            if (count > 0)
                SelectedIndex = count - 1;
        }

        public void CommitSelection()
        {
            RaiseEvent(new RoutedEventArgs(SelectionCommittedEvent, this));
        }

        public string GetItemDisplayText(object item)
        {
            if (item == null) return string.Empty;

            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                var prop = item.GetType().GetProperty(DisplayMemberPath, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    var val = prop.GetValue(item, null);
                    return val?.ToString() ?? string.Empty;
                }
            }

            return item.ToString();
        }

        public string GetItemSearchText(object item)
        {
            if (item == null) return string.Empty;

            if (!string.IsNullOrEmpty(SearchMemberPath))
            {
                var prop = item.GetType().GetProperty(SearchMemberPath, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    var val = prop.GetValue(item, null);
                    return val?.ToString() ?? string.Empty;
                }
            }

            return GetItemDisplayText(item);
        }

        public object GetItemValue(object item)
        {
            if (item == null) return null;

            if (!string.IsNullOrEmpty(SelectedValuePath))
            {
                var prop = item.GetType().GetProperty(SelectedValuePath, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    return prop.GetValue(item, null);
                }
            }

            return item;
        }

        public bool SelectItemByValue(object value)
        {
            if (value == null)
            {
                SelectedIndex = -1;
                return true;
            }

            if (ItemsSource != null)
            {
                int index = 0;
                foreach (var item in ItemsSource)
                {
                    object itemVal = GetItemValue(item);
                    if (Equals(itemVal, value) || (itemVal != null && itemVal.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        SelectedIndex = index;
                        SelectedItem = item;
                        return true;
                    }
                    index++;
                }
            }

            return false;
        }

        public bool SelectItemByText(string text, bool exactMatch = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                SelectedIndex = -1;
                return false;
            }

            if (ItemsSource != null)
            {
                int index = 0;
                object partialMatchItem = null;
                int partialMatchIndex = -1;

                foreach (var item in ItemsSource)
                {
                    string display = GetItemDisplayText(item);
                    string search = GetItemSearchText(item);

                    if (display.Equals(text, StringComparison.OrdinalIgnoreCase) || search.Equals(text, StringComparison.OrdinalIgnoreCase))
                    {
                        SelectedIndex = index;
                        SelectedItem = item;
                        return true;
                    }
                    else if (!exactMatch && partialMatchItem == null &&
                             (display.StartsWith(text, StringComparison.OrdinalIgnoreCase) || search.StartsWith(text, StringComparison.OrdinalIgnoreCase) ||
                              display.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 || search.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        partialMatchItem = item;
                        partialMatchIndex = index;
                    }
                    index++;
                }

                if (!exactMatch && partialMatchItem != null)
                {
                    SelectedIndex = partialMatchIndex;
                    SelectedItem = partialMatchItem;
                    return true;
                }
            }

            return false;
        }

        public void ApplyFilter(string filterText)
        {
            if (ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            if (view != null && view.CanFilter)
            {
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    view.Filter = null;
                }
                else
                {
                    view.Filter = item =>
                    {
                        if (item == null) return false;
                        string display = GetItemDisplayText(item);
                        string search = GetItemSearchText(item);

                        return display.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               search.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }
            }
        }

        public void ClearFilter()
        {
            if (ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            if (view != null && view.CanFilter)
            {
                view.Filter = null;
            }
        }
    }
}
