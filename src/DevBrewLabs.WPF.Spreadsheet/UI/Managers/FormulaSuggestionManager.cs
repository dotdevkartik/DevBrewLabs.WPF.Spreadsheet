using DevBrewLabs.Evalis;
using DevBrewLabs.WPF.Spreadsheet.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class FormulaSuggestionManager : UIManager
    {
        private Popup _suggestionPopup;
        private SuggestionListBox _suggestionListBox;

        private Popup _parameterPopup;
        private FormulaParameterTooltip _parameterTooltip;

        private TextBox _attachedEditor;
        private Window _parentWindow;

        public bool IsOpen => (_suggestionPopup != null && _suggestionPopup.IsOpen) || 
                              (_parameterPopup != null && _parameterPopup.IsOpen);

        public FormulaSuggestionManager(Spread spread) : base(spread)
        {
        }

        private void EnsureComponents()
        {
            if (_suggestionPopup == null)
            {
                _suggestionPopup = new Popup
                {
                    Placement = PlacementMode.Bottom,
                    HorizontalOffset = 0,
                    VerticalOffset = 2,
                    StaysOpen = true,
                    PopupAnimation = PopupAnimation.Fade,
                    AllowsTransparency = true,
                    IsOpen = false
                };

                _suggestionListBox = new SuggestionListBox
                {
                    MinWidth = 260,
                    MaxWidth = 340
                };

                _suggestionListBox.PreviewMouseLeftButtonDown += OnSuggestionListBoxMouseLeftButtonDown;

                _suggestionPopup.Child = _suggestionListBox;
            }

            if (_parameterPopup == null)
            {
                _parameterPopup = new Popup
                {
                    Placement = PlacementMode.Bottom,
                    HorizontalOffset = 0,
                    VerticalOffset = 2,
                    StaysOpen = true,
                    PopupAnimation = PopupAnimation.Fade,
                    AllowsTransparency = true,
                    IsOpen = false
                };

                _parameterTooltip = new FormulaParameterTooltip();
                _parameterPopup.Child = _parameterTooltip;
            }
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
            _attachedEditor.SelectionChanged += OnEditorSelectionChanged;
            _attachedEditor.LostKeyboardFocus += OnEditorLostKeyboardFocus;
        }

        public void Detach()
        {
            Hide();

            if (_attachedEditor != null)
            {
                _attachedEditor.TextChanged -= OnEditorTextChanged;
                _attachedEditor.PreviewKeyDown -= OnEditorPreviewKeyDown;
                _attachedEditor.SelectionChanged -= OnEditorSelectionChanged;
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

        public void Show(IEnumerable<FormulaInfo> formulas)
        {
            ShowSuggestions(formulas);
        }

        private void ShowSuggestions(IEnumerable<FormulaInfo> formulas)
        {
            if (_attachedEditor == null || Spread == null || !Spread.ShowFormulaSuggestions)
                return;

            EnsureComponents();

            if (_parameterPopup != null && _parameterPopup.IsOpen)
            {
                _parameterPopup.IsOpen = false;
            }

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

        private void ShowParameterTooltip(FormulaInfo formulaInfo, int activeArgumentIndex)
        {
            if (_attachedEditor == null || Spread == null || !Spread.ShowFormulaSuggestions)
                return;

            EnsureComponents();

            if (_suggestionPopup != null && _suggestionPopup.IsOpen)
            {
                _suggestionPopup.IsOpen = false;
            }

            _parameterPopup.PlacementTarget = _attachedEditor;
            _parameterTooltip.Update(formulaInfo, activeArgumentIndex);
            AttachWindowEvents(_attachedEditor);
            _parameterPopup.IsOpen = true;
        }

        public void Hide()
        {
            if (_suggestionPopup != null)
                _suggestionPopup.IsOpen = false;

            if (_parameterPopup != null)
                _parameterPopup.IsOpen = false;

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

        internal class FormulaEditContext
        {
            public bool IsFormula { get; set; }
            public bool IsInsideFunction { get; set; }
            public string FunctionName { get; set; }
            public int ArgumentIndex { get; set; }
            public string IncompletePrefix { get; set; }
            public int IncompletePrefixStartIndex { get; set; }
        }

        internal static FormulaEditContext ParseFormulaAtCaret(string text, int caretIndex)
        {
            if (string.IsNullOrEmpty(text) || !text.StartsWith("="))
            {
                return new FormulaEditContext { IsFormula = false };
            }

            if (caretIndex < 0) caretIndex = 0;
            if (caretIndex > text.Length) caretIndex = text.Length;

            var functionStack = new Stack<(string FunctionName, int ArgIndex)>();
            bool inQuotes = false;
            int lastIdentifierStart = -1;
            string lastIdentifier = string.Empty;

            for (int i = 1; i < caretIndex; i++)
            {
                char c = text[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    lastIdentifierStart = -1;
                    lastIdentifier = string.Empty;
                    continue;
                }

                if (inQuotes)
                    continue;

                if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                {
                    if (lastIdentifierStart < 0)
                        lastIdentifierStart = i;
                    lastIdentifier = text.Substring(lastIdentifierStart, i - lastIdentifierStart + 1);
                }
                else if (c == '(')
                {
                    string fnName = !string.IsNullOrEmpty(lastIdentifier) ? lastIdentifier : string.Empty;
                    functionStack.Push((fnName, 0));
                    lastIdentifierStart = -1;
                    lastIdentifier = string.Empty;
                }
                else if (c == ',')
                {
                    if (functionStack.Count > 0)
                    {
                        var top = functionStack.Pop();
                        functionStack.Push((top.FunctionName, top.ArgIndex + 1));
                    }
                    lastIdentifierStart = -1;
                    lastIdentifier = string.Empty;
                }
                else if (c == ')')
                {
                    if (functionStack.Count > 0)
                    {
                        functionStack.Pop();
                    }
                    lastIdentifierStart = -1;
                    lastIdentifier = string.Empty;
                }
                else
                {
                    lastIdentifierStart = -1;
                    lastIdentifier = string.Empty;
                }
            }

            if (inQuotes)
            {
                return new FormulaEditContext { IsFormula = true, IsInsideFunction = false };
            }

            return new FormulaEditContext
            {
                IsFormula = true,
                IsInsideFunction = functionStack.Count > 0,
                FunctionName = functionStack.Count > 0 ? functionStack.Peek().FunctionName : null,
                ArgumentIndex = functionStack.Count > 0 ? functionStack.Peek().ArgIndex : 0,
                IncompletePrefix = lastIdentifier,
                IncompletePrefixStartIndex = lastIdentifierStart
            };
        }

        public void TryShowSuggestions()
        {
            if (Spread == null || !Spread.ShowFormulaSuggestions || _attachedEditor == null)
            {
                Hide();
                return;
            }

            string text = _attachedEditor.Text ?? "";
            int caretIndex = _attachedEditor.CaretIndex;

            var editContext = ParseFormulaAtCaret(text, caretIndex);
            if (!editContext.IsFormula)
            {
                Hide();
                return;
            }

            var registeredFormulas = Spread.WorkBook?.CalcEngine?.GetRegisteredFormulas()?.ToList();
            if (registeredFormulas == null || registeredFormulas.Count == 0)
            {
                Hide();
                return;
            }

            // 1. If typing an incomplete function name matching formula names (Suggestion List Mode - Image 2)
            if (!string.IsNullOrEmpty(editContext.IncompletePrefix) && char.IsLetter(editContext.IncompletePrefix[0]))
            {
                var matchingFormulas = registeredFormulas
                    .Where(fx => fx.Name.StartsWith(editContext.IncompletePrefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingFormulas.Count > 0)
                {
                    ShowSuggestions(matchingFormulas);
                    return;
                }
            }

            // 2. If inside function arguments (Parameter Tooltip Mode - Image 1)
            if (editContext.IsInsideFunction && !string.IsNullOrEmpty(editContext.FunctionName))
            {
                var activeFormula = registeredFormulas
                    .FirstOrDefault(fx => string.Equals(fx.Name, editContext.FunctionName, StringComparison.OrdinalIgnoreCase));

                if (activeFormula != null)
                {
                    ShowParameterTooltip(activeFormula, editContext.ArgumentIndex);
                    return;
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

        private void OnEditorSelectionChanged(object sender, RoutedEventArgs e)
        {
            TryShowSuggestions();
        }

        private void OnEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsOpen)
                return;

            if (_suggestionPopup != null && _suggestionPopup.IsOpen && _suggestionListBox != null)
            {
                int count = _suggestionListBox.ItemsCount;
                if (count > 0)
                {
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
            }
        }

        private void ApplySelectedSuggestion()
        {
            if (_attachedEditor == null || _suggestionListBox == null)
                return;

            string formulaName = null;

            if (_suggestionListBox.SelectedItem is FormulaInfo fi)
            {
                formulaName = fi.Name;
            }
            else if (_suggestionListBox.SelectedIndex >= 0 && _suggestionListBox.ItemsSource != null)
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

            if (!string.IsNullOrEmpty(formulaName))
            {
                string text = _attachedEditor.Text ?? "";
                int caretIndex = _attachedEditor.CaretIndex;
                var editContext = ParseFormulaAtCaret(text, caretIndex);

                if (editContext.IncompletePrefixStartIndex >= 0 && !string.IsNullOrEmpty(editContext.IncompletePrefix))
                {
                    string before = text.Substring(0, editContext.IncompletePrefixStartIndex);
                    string after = text.Substring(Math.Min(editContext.IncompletePrefixStartIndex + editContext.IncompletePrefix.Length, text.Length));
                    _attachedEditor.Text = $"{before}{formulaName}({after}";
                    _attachedEditor.CaretIndex = editContext.IncompletePrefixStartIndex + formulaName.Length + 1;
                }
                else
                {
                    _attachedEditor.Text = $"={formulaName}(";
                    _attachedEditor.CaretIndex = _attachedEditor.Text.Length;
                }

                TryShowSuggestions();
            }
        }

        private void OnSuggestionListBoxMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ApplySelectedSuggestion();
            }
        }

        public override void Dispose()
        {
            Detach();

            if (_suggestionListBox != null)
            {
                _suggestionListBox.PreviewMouseLeftButtonDown -= OnSuggestionListBoxMouseLeftButtonDown;
                _suggestionListBox = null;
            }

            _suggestionPopup = null;
            _parameterPopup = null;
            _parameterTooltip = null;

            base.Dispose();
        }
    }
}
