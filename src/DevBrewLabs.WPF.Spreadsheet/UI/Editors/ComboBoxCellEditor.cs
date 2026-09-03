using DevBrewLabs.WPF.Spreadsheet.Components;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// In-place dropdown ComboBox editor for spreadsheet cells supporting type-ahead, 
    /// search filtering, suggestions, and editable/non-editable modes without forced auto-selection.
    /// </summary>
    public class ComboBoxCellEditor : TextCellEditor
    {
        private SpreadDropDownListBox _dropDownList;
        private bool _isInitializing;
        private bool _isSyncingFromSelection;
        private SheetView _sheetView;
        private int _row;
        private int _column;
        private string _typeAheadBuffer = string.Empty;
        private DateTime _lastTypeAheadTime = DateTime.MinValue;

        public IEnumerable ItemsSource { get; set; }
        public string DisplayMemberPath { get; set; }
        public string SelectedValuePath { get; set; }
        public string SearchMemberPath { get; set; }
        public bool ShowSuggestions { get; set; } = true;
        public bool IsEditable { get; set; }
        public double MaxDropDownHeight { get; set; } = 220;
        public double? MaxDropDownWidth { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        public SpreadDropDownListBox DropDownList => _dropDownList;

        public bool IsPopupOpen => _sheetView?.Spread?.PopupManager != null &&
                                   _sheetView.Spread.PopupManager.IsPopupOpen &&
                                   _sheetView.Spread.PopupManager.CurrentContent == _dropDownList;

        public ComboBoxCellEditor()
        {
            InitializeDropDownList();
        }

        private void InitializeDropDownList()
        {
            _dropDownList = new SpreadDropDownListBox();
            _dropDownList.SelectionChanged += OnListSelectionChanged;
            _dropDownList.SelectionCommitted += OnListSelectionCommitted;
        }

        public void TogglePopup()
        {
            if (IsPopupOpen)
            {
                ClosePopup();
            }
            else
            {
                OpenPopup();
            }
        }

        public void OpenPopup()
        {
            if (_sheetView?.Spread?.PopupManager == null)
                return;

            _dropDownList.ItemsSource = ItemsSource;
            _dropDownList.DisplayMemberPath = DisplayMemberPath;
            _dropDownList.SelectedValuePath = SelectedValuePath;
            _dropDownList.SearchMemberPath = SearchMemberPath;
            _dropDownList.ItemTemplate = ItemTemplate;
            _dropDownList.MaxHeight = MaxDropDownHeight;

            var unzoomedRect = _sheetView.ViewPort.GetCellRect(_row, _column);
            double zoom = _sheetView.ZoomFactor > 0 ? _sheetView.ZoomFactor : 1.0;
            double cellWidth = unzoomedRect.Width * zoom;

            _dropDownList.MinWidth = Math.Max(cellWidth, 120);
            if (MaxDropDownWidth.HasValue)
            {
                _dropDownList.MaxWidth = MaxDropDownWidth.Value;
            }

            _sheetView.Spread.PopupManager.ShowForCell(
                _sheetView,
                _row,
                _column,
                _dropDownList,
                new PopupPlacementOptions
                {
                    Alignment = PopupAlignment.Left,
                    AutoFlip = true,
                    UseStandardContainer = true,
                    RestoreFocusTarget = this
                });
        }

        public void ClosePopup()
        {
            if (IsPopupOpen)
            {
                _sheetView?.Spread?.PopupManager?.ClosePopup();
            }
            _dropDownList?.ClearFilter();
            _typeAheadBuffer = string.Empty;
        }

        private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            if (_dropDownList.SelectedItem != null)
            {
                _isSyncingFromSelection = true;
                try
                {
                    Text = _dropDownList.GetItemDisplayText(_dropDownList.SelectedItem);
                }
                finally
                {
                    _isSyncingFromSelection = false;
                }
            }
        }

        private void OnListSelectionCommitted(object sender, RoutedEventArgs e)
        {
            ClosePopup();
            if (_dropDownList.SelectedItem != null)
            {
                _isSyncingFromSelection = true;
                try
                {
                    Text = _dropDownList.GetItemDisplayText(_dropDownList.SelectedItem);
                }
                finally
                {
                    _isSyncingFromSelection = false;
                }
            }

            if (!IsEditable)
            {
                _sheetView?.Spread?.EditingManager?.EndEdit(true);
            }
            else
            {
                Focus();
                CaretIndex = Text?.Length ?? 0;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (_isInitializing || _isSyncingFromSelection)
                return;

            string currentText = Text ?? string.Empty;

            if (IsEditable)
            {
                if (ShowSuggestions && !string.IsNullOrEmpty(currentText))
                {
                    if (!IsPopupOpen && _sheetView != null)
                    {
                        OpenPopup();
                    }

                    _dropDownList.ApplyFilter(currentText);
                }
                else if (string.IsNullOrEmpty(currentText))
                {
                    _dropDownList.ClearFilter();
                }
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!IsEditable && !string.IsNullOrEmpty(e.Text))
            {
                e.Handled = true;
                HandleNonEditableTypeAhead(e.Text);
                return;
            }

            base.OnPreviewTextInput(e);
        }

        private void HandleNonEditableTypeAhead(string input)
        {
            var now = DateTime.Now;
            if ((now - _lastTypeAheadTime).TotalMilliseconds > 1000)
            {
                _typeAheadBuffer = input;
            }
            else
            {
                _typeAheadBuffer += input;
            }
            _lastTypeAheadTime = now;

            if (ShowSuggestions && !IsPopupOpen)
            {
                OpenPopup();
            }

            // In non-editable mode, type-ahead selects and scrolls to the matching item without filtering out other items
            bool matched = _dropDownList.SelectItemByText(_typeAheadBuffer, exactMatch: false);
            if (!matched && _typeAheadBuffer.Length > 1)
            {
                _typeAheadBuffer = input;
                matched = _dropDownList.SelectItemByText(_typeAheadBuffer, exactMatch: false);
            }

            if (matched && _dropDownList.SelectedItem != null)
            {
                _isSyncingFromSelection = true;
                try
                {
                    Text = _dropDownList.GetItemDisplayText(_dropDownList.SelectedItem);
                }
                finally
                {
                    _isSyncingFromSelection = false;
                }
            }
        }

        public override void StartEdit(IEditorContext context)
        {
            base.StartEdit(context);

            _sheetView = context.SheetView as SheetView;
            _row = context.Row;
            _column = context.Column;
            _typeAheadBuffer = string.Empty;

            IsReadOnly = !IsEditable;
            if (!IsEditable)
            {
                Cursor = Cursors.Arrow;
            }

            _isInitializing = true;
            try
            {
                _dropDownList.ClearFilter();
                _dropDownList.ItemsSource = ItemsSource;
                _dropDownList.DisplayMemberPath = DisplayMemberPath;
                _dropDownList.SelectedValuePath = SelectedValuePath;
                _dropDownList.SearchMemberPath = SearchMemberPath;
                _dropDownList.ItemTemplate = ItemTemplate;

                object cellVal = context.Value;
                bool matched = false;

                if (cellVal != null)
                {
                    matched = _dropDownList.SelectItemByValue(cellVal);
                }

                if (!matched && !string.IsNullOrEmpty(context.FormattedText))
                {
                    matched = _dropDownList.SelectItemByText(context.FormattedText, exactMatch: true);
                }

                if (matched && _dropDownList.SelectedItem != null)
                {
                    Text = _dropDownList.GetItemDisplayText(_dropDownList.SelectedItem);
                }
                else if (context.Trigger != EditTrigger.DirectTyping)
                {
                    Text = context.FormattedText ?? cellVal?.ToString() ?? string.Empty;
                }
                else if (!IsEditable)
                {
                    Text = context.FormattedText ?? cellVal?.ToString() ?? string.Empty;
                }
            }
            finally
            {
                _isInitializing = false;
            }

            if (context.Trigger == EditTrigger.DropdownClick)
            {
                OpenPopup();
            }
            else if (context.Trigger == EditTrigger.DirectTyping)
            {
                if (IsEditable)
                {
                    if (context.InitialInput != null)
                    {
                        Text = context.InitialInput;
                        CaretIndex = Text.Length;
                    }
                    if (ShowSuggestions)
                    {
                        OpenPopup();
                        if (!string.IsNullOrEmpty(Text))
                        {
                            _dropDownList.ApplyFilter(Text);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(context.InitialInput))
                    {
                        HandleNonEditableTypeAhead(context.InitialInput);
                    }
                    else if (ShowSuggestions)
                    {
                        OpenPopup();
                    }
                }
            }
        }

        public override object GetValue()
        {
            if (!IsEditable)
            {
                if (_dropDownList?.SelectedItem != null)
                {
                    object val = _dropDownList.GetItemValue(_dropDownList.SelectedItem);
                    return val ?? _dropDownList.SelectedItem;
                }
                return Context?.Value;
            }

            string currentText = Text ?? string.Empty;

            if (_dropDownList != null && ItemsSource != null)
            {
                // 1. If an item was selected, and its display/search text matches current text
                if (_dropDownList.SelectedItem != null)
                {
                    string selectedDisplay = _dropDownList.GetItemDisplayText(_dropDownList.SelectedItem);
                    string selectedSearch = _dropDownList.GetItemSearchText(_dropDownList.SelectedItem);

                    if (selectedDisplay.Equals(currentText, StringComparison.OrdinalIgnoreCase) ||
                        selectedSearch.Equals(currentText, StringComparison.OrdinalIgnoreCase))
                    {
                        object val = _dropDownList.GetItemValue(_dropDownList.SelectedItem);
                        return val ?? _dropDownList.SelectedItem;
                    }
                }

                // 2. Check if the typed text matches an item in ItemsSource
                foreach (var item in ItemsSource)
                {
                    if (item == null) continue;

                    string display = _dropDownList.GetItemDisplayText(item);
                    string search = _dropDownList.GetItemSearchText(item);

                    if (display.Equals(currentText, StringComparison.OrdinalIgnoreCase) ||
                        search.Equals(currentText, StringComparison.OrdinalIgnoreCase))
                    {
                        object val = _dropDownList.GetItemValue(item);
                        return val ?? item;
                    }
                }
            }

            // 3. In editable mode, return custom typed text if no match
            return currentText;
        }

        public override bool HandlesKeyDown(KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Alt + Down Arrow or F4 toggles dropdown popup
            if ((key == Key.Down && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) || key == Key.F4)
            {
                TogglePopup();
                return true;
            }

            if (IsPopupOpen)
            {
                if (key == Key.Escape)
                {
                    ClosePopup();
                    return true;
                }

                if (key == Key.Enter || key == Key.Tab)
                {
                    if (_dropDownList.SelectedItem != null)
                    {
                        _isSyncingFromSelection = true;
                        try
                        {
                            Text = _dropDownList.GetItemDisplayText(_dropDownList.SelectedItem);
                        }
                        finally
                        {
                            _isSyncingFromSelection = false;
                        }
                    }
                    ClosePopup();
                    return false; // Allow EditingManager to commit
                }

                if (key == Key.Up)
                {
                    _dropDownList.MoveSelection(-1);
                    return true;
                }

                if (key == Key.Down)
                {
                    _dropDownList.MoveSelection(1);
                    return true;
                }

                if (key == Key.PageUp)
                {
                    _dropDownList.MoveSelection(-5);
                    return true;
                }

                if (key == Key.PageDown)
                {
                    _dropDownList.MoveSelection(5);
                    return true;
                }

                if (key == Key.Home)
                {
                    _dropDownList.MoveSelectionFirst();
                    return true;
                }

                if (key == Key.End)
                {
                    _dropDownList.MoveSelectionLast();
                    return true;
                }
            }
            else if (!IsEditable)
            {
                if (key == Key.Down || key == Key.Space)
                {
                    OpenPopup();
                    return true;
                }
                else if (key == Key.Up)
                {
                    _dropDownList.MoveSelection(-1);
                    return true;
                }
            }

            return base.HandlesKeyDown(e);
        }

        public override void EndEdit()
        {
            ClosePopup();
            _typeAheadBuffer = string.Empty;
            _sheetView = null;
            base.EndEdit();
        }
    }
}
