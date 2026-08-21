using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    /// <summary>
    /// Represents the suggestion list control for formula auto-complete.
    /// </summary>
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    public class SuggestionListBox : Control
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(SuggestionListBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(SuggestionListBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(SuggestionListBox),
                new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                nameof(SelectedValue),
                typeof(object),
                typeof(SuggestionListBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(
                nameof(SelectedValuePath),
                typeof(string),
                typeof(SuggestionListBox),
                new PropertyMetadata(string.Empty, OnSelectedValuePathChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(SuggestionListBox),
                new PropertyMetadata(string.Empty, OnDisplayMemberPathChanged));

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(
                nameof(ItemContainerStyle),
                typeof(Style),
                typeof(SuggestionListBox),
                new PropertyMetadata(null, OnItemContainerStyleChanged));

        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SelectionChanged),
                RoutingStrategy.Bubble,
                typeof(SelectionChangedEventHandler),
                typeof(SuggestionListBox));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        static SuggestionListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SuggestionListBox),
                new FrameworkPropertyMetadata(typeof(SuggestionListBox)));
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

        public ItemContainerGenerator ItemContainerGenerator => _listBox?.ItemContainerGenerator;

        public SuggestionListBox()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_listBox != null)
            {
                _listBox.SelectionChanged -= OnListBoxSelectionChanged;
            }

            _listBox = GetTemplateChild("PART_ListBox") as ListBox;

            if (_listBox != null)
            {
                _listBox.ItemsSource = ItemsSource;
                _listBox.DisplayMemberPath = DisplayMemberPath;
                _listBox.SelectedValuePath = SelectedValuePath;
                if (ItemContainerStyle != null)
                    _listBox.ItemContainerStyle = ItemContainerStyle;

                if (SelectedIndex >= 0)
                    _listBox.SelectedIndex = SelectedIndex;
                else if (SelectedItem != null)
                    _listBox.SelectedItem = SelectedItem;

                _listBox.SelectionChanged += OnListBoxSelectionChanged;
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

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null)
            {
                control._listBox.ItemsSource = (IEnumerable)e.NewValue;
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null && !control._isSyncingSelection)
            {
                control._isSyncingSelection = true;
                try
                {
                    control._listBox.SelectedIndex = (int)e.NewValue;
                    control.SelectedItem = control._listBox.SelectedItem;
                    control.SelectedValue = control._listBox.SelectedValue;
                    if (control._listBox.SelectedItem != null)
                        control._listBox.ScrollIntoView(control._listBox.SelectedItem);
                }
                finally
                {
                    control._isSyncingSelection = false;
                }
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null && !control._isSyncingSelection)
            {
                control._isSyncingSelection = true;
                try
                {
                    control._listBox.SelectedItem = e.NewValue;
                    control.SelectedIndex = control._listBox.SelectedIndex;
                    control.SelectedValue = control._listBox.SelectedValue;
                    if (control._listBox.SelectedItem != null)
                        control._listBox.ScrollIntoView(control._listBox.SelectedItem);
                }
                finally
                {
                    control._isSyncingSelection = false;
                }
            }
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null && !control._isSyncingSelection)
            {
                control._isSyncingSelection = true;
                try
                {
                    control._listBox.SelectedValue = e.NewValue;
                }
                finally
                {
                    control._isSyncingSelection = false;
                }
            }
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null)
            {
                control._listBox.DisplayMemberPath = (string)e.NewValue;
            }
        }

        private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null)
            {
                control._listBox.SelectedValuePath = (string)e.NewValue;
            }
        }

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SuggestionListBox)d;
            if (control._listBox != null)
            {
                control._listBox.ItemContainerStyle = (Style)e.NewValue;
            }
        }
    }
}
