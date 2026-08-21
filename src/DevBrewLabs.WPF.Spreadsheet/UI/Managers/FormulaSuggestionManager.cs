using DevBrewLabs.Evalis;
using DevBrewLabs.WPF.Spreadsheet.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class FormulaSuggestionManager : UIManager
    {
        private Popup _suggestionPopup;
        private Popup _descriptionPopup;
        private TextBlock _descriptionTextBlock;
        private SuggestionListBox _suggestionListBox;
        private TextBox _attachedEditor;

        public bool IsOpen => _suggestionPopup != null && _suggestionPopup.IsOpen;

        public FormulaSuggestionManager(Spread spread) : base(spread)
        {
        }

        private void EnsureComponents()
        {
            if (_suggestionPopup != null)
                return;

            _suggestionPopup = new Popup
            {
                Placement = PlacementMode.Bottom,
                HorizontalOffset = 7,
                VerticalOffset = 5,
                StaysOpen = true,
                PopupAnimation = PopupAnimation.Fade,
                AllowsTransparency = true,
                IsOpen = false
            };

            _descriptionPopup = new Popup
            {
                Placement = PlacementMode.Right,
                IsOpen = false,
                HorizontalOffset = 10,
                StaysOpen = true,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            _descriptionTextBlock = new TextBlock
            {
                Foreground = Brushes.Black,
                Padding = new Thickness(2, 0, 2, 0)
            };

            _descriptionPopup.Child = new Border
            {
                Child = _descriptionTextBlock,
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Black,
                Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240))
            };

            _suggestionListBox = new SuggestionListBox
            {
                Width = 100,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Name"
            };

            _suggestionListBox.PreviewMouseLeftButtonDown += OnSuggestionListBoxMouseLeftButtonDown;
            _suggestionListBox.SelectionChanged += OnFormulaSelected;

            _suggestionPopup.Child = _suggestionListBox;
        }

        public void Attach(TextBox editor)
        {
            if (_attachedEditor != null)
                Detach();

            _attachedEditor = editor;
            if (_attachedEditor == null)
                return;

            EnsureComponents();

            _attachedEditor.TextChanged += OnEditorTextChanged;
            _attachedEditor.PreviewKeyDown += OnEditorPreviewKeyDown;
            _attachedEditor.LostKeyboardFocus += OnEditorLostKeyboardFocus;
        }

        public void Detach()
        {
            Hide();

            if (_attachedEditor != null)
            {
                _attachedEditor.TextChanged -= OnEditorTextChanged;
                _attachedEditor.PreviewKeyDown -= OnEditorPreviewKeyDown;
                _attachedEditor.LostKeyboardFocus -= OnEditorLostKeyboardFocus;
                _attachedEditor = null;
            }
        }

        private void OnEditorLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_suggestionListBox != null && (_suggestionListBox.IsKeyboardFocusWithin || _suggestionListBox.IsFocused))
                return;

            Hide();
        }

        private Window _parentWindow;

        public void Show(IEnumerable<FormulaInfo> formulas)
        {
            if (_attachedEditor == null || Spread == null || !Spread.ShowFormulaSuggestions)
                return;

            EnsureComponents();

            _suggestionPopup.PlacementTarget = _attachedEditor;
            _suggestionListBox.ItemsSource = formulas;
            _suggestionListBox.ApplyTemplate();
            AttachWindowEvents(_attachedEditor);
            _suggestionPopup.IsOpen = true;
            _suggestionListBox.SelectedIndex = 0;
            if (_suggestionListBox.SelectedItem == null && formulas != null)
            {
                _suggestionListBox.SelectedItem = formulas.FirstOrDefault();
            }
        }

        public void Hide()
        {
            if (_suggestionPopup != null)
                _suggestionPopup.IsOpen = false;

            if (_descriptionPopup != null)
                _descriptionPopup.IsOpen = false;

            DetachWindowEvents();
        }

        private void AttachWindowEvents(UIElement element)
        {
            DetachWindowEvents();

            _parentWindow = Window.GetWindow(element ?? Spread);
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
            Hide();
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            Hide();
        }

        public void TryShowSuggestions()
        {
            if (Spread == null || !Spread.ShowFormulaSuggestions || _attachedEditor == null)
            {
                Hide();
                return;
            }

            string text = _attachedEditor.Text ?? "";
            if (text.Length > 1 && text.StartsWith("="))
            {
                var searchString = text.Substring(1);
                var formulas = Spread.WorkBook?.CalcEngine?.GetRegisteredFormulas();
                if (formulas != null)
                {
                    var searchedFormulas = formulas.Where(fx => fx.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (searchedFormulas.Count > 0)
                    {
                        Show(searchedFormulas);
                        return;
                    }
                }
            }

            Hide();
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Spread?.FormulaTextBox != null && _attachedEditor != null)
            {
                Spread.FormulaTextBox._txtEditor.Text = _attachedEditor.Text;
                Spread.FormulaTextBox._txtEditor.ScrollToEnd();
            }

            TryShowSuggestions();
        }

        private void OnEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsOpen || _suggestionListBox == null)
                return;

            int count = _suggestionListBox.ItemsCount;
            if (count == 0)
                return;

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.Down && _suggestionListBox.SelectedIndex < count - 1)
            {
                e.Handled = true;
                _suggestionListBox.SelectedIndex++;
            }
            else if (key == Key.Up && _suggestionListBox.SelectedIndex > 0)
            {
                e.Handled = true;
                _suggestionListBox.SelectedIndex--;
            }
            else if (key == Key.Tab)
            {
                e.Handled = true;
                ApplySelectedSuggestion();
            }
        }

        private void ApplySelectedSuggestion()
        {
            if (_attachedEditor == null || _suggestionListBox == null)
                return;

            string formulaName = null;

            if (_suggestionListBox.SelectedIndex >= 0 && _suggestionListBox.ItemsSource != null)
            {
                int index = 0;
                foreach (var item in _suggestionListBox.ItemsSource)
                {
                    if (index == _suggestionListBox.SelectedIndex)
                    {
                        if (item is FormulaInfo itemFi)
                            formulaName = itemFi.Name;
                        else
                            formulaName = item?.ToString();
                        break;
                    }
                    index++;
                }
            }

            if (string.IsNullOrEmpty(formulaName))
            {
                if (_suggestionListBox.SelectedItem is FormulaInfo fi)
                    formulaName = fi.Name;
                else if (_suggestionListBox.SelectedValue != null)
                    formulaName = _suggestionListBox.SelectedValue.ToString();
                else if (_suggestionListBox.SelectedItem != null)
                    formulaName = _suggestionListBox.SelectedItem.ToString();
            }

            if (!string.IsNullOrEmpty(formulaName))
            {
                _attachedEditor.Text = $"={formulaName}(";
                _attachedEditor.CaretIndex = _attachedEditor.Text.Length;
                Hide();
            }
        }

        private void OnSuggestionListBoxMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ApplySelectedSuggestion();
            }
        }

        private void OnFormulaSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedFormula = _suggestionListBox?.SelectedItem as FormulaInfo;
            if (selectedFormula == null && _suggestionListBox?.SelectedIndex >= 0 && _suggestionListBox?.ItemsSource != null)
            {
                int index = 0;
                foreach (var item in _suggestionListBox.ItemsSource)
                {
                    if (index == _suggestionListBox.SelectedIndex)
                    {
                        selectedFormula = item as FormulaInfo;
                        break;
                    }
                    index++;
                }
            }

            if (selectedFormula == null)
            {
                if (_descriptionPopup != null)
                    _descriptionPopup.IsOpen = false;
                return;
            }

            var listBoxItem = _suggestionListBox?.ItemContainerGenerator?.ContainerFromItem(selectedFormula) as ListBoxItem;
            if (listBoxItem != null && _descriptionPopup != null && _descriptionTextBlock != null)
            {
                _descriptionPopup.PlacementTarget = listBoxItem;
                _descriptionTextBlock.Text = selectedFormula.Description;
                _descriptionPopup.IsOpen = true;
            }
        }

        public override void Dispose()
        {
            Detach();

            if (_suggestionListBox != null)
            {
                _suggestionListBox.PreviewMouseLeftButtonDown -= OnSuggestionListBoxMouseLeftButtonDown;
                _suggestionListBox.SelectionChanged -= OnFormulaSelected;
                _suggestionListBox = null;
            }

            _suggestionPopup = null;
            _descriptionPopup = null;
            _descriptionTextBlock = null;

            base.Dispose();
        }
    }
}
