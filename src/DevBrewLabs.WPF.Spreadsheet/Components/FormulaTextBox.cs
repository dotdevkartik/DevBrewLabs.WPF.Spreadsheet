using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Commands;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    /// <summary>
    /// Represents the formula bar control for a spreadsheet.
    /// </summary>
    [TemplatePart(Name = "PART_Editor", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CommitButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_FunctionButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ExpandButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ExpandIcon", Type = typeof(Path))]
    public class FormulaTextBox : Control
    {
        public static readonly DependencyProperty SpreadProperty =
            DependencyProperty.Register(
                nameof(Spread),
                typeof(Spread),
                typeof(FormulaTextBox),
                new PropertyMetadata(OnSpreadAttached));

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(FormulaTextBox),
                new PropertyMetadata(true, OnIsExpandedChanged));

        public static readonly DependencyProperty CommitCommandProperty =
            DependencyProperty.Register(
                nameof(CommitCommand),
                typeof(ICommand),
                typeof(FormulaTextBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
                nameof(CancelCommand),
                typeof(ICommand),
                typeof(FormulaTextBox),
                new PropertyMetadata(null));

        static FormulaTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FormulaTextBox),
                new FrameworkPropertyMetadata(typeof(FormulaTextBox)));
        }

        internal TextBox _txtEditor;
        private Button _btnExpand;
        private Path _expandIcon;
        private Button _btnCancel;
        private Button _btnCommit;
        private Button _btnFunction;

        public Spread Spread
        {
            get => (Spread)GetValue(SpreadProperty);
            set => SetValue(SpreadProperty, value);
        }

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public ICommand CommitCommand
        {
            get => (ICommand)GetValue(CommitCommandProperty);
            set => SetValue(CommitCommandProperty, value);
        }

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public TextBox Editor => _txtEditor;

        public string Text
        {
            get => _txtEditor?.Text;
            set
            {
                if (_txtEditor != null)
                {
                    _txtEditor.Text = value;
                    _txtEditor.ScrollToEnd();
                }
            }
        }

        public FormulaTextBox()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_txtEditor != null)
            {
                _txtEditor.TextChanged -= OnTextChanged;
                _txtEditor.KeyDown -= OnTextBoxKeyDown;
            }

            if (_btnExpand != null)
            {
                _btnExpand.Click -= OnExpandToggleClick;
            }

            _txtEditor = GetTemplateChild("PART_Editor") as TextBox;
            _btnExpand = GetTemplateChild("PART_ExpandButton") as Button;
            _expandIcon = GetTemplateChild("PART_ExpandIcon") as Path;
            _btnCancel = GetTemplateChild("PART_CancelButton") as Button;
            _btnCommit = GetTemplateChild("PART_CommitButton") as Button;
            _btnFunction = GetTemplateChild("PART_FunctionButton") as Button;

            if (_txtEditor != null)
            {
                _txtEditor.TextChanged += OnTextChanged;
                _txtEditor.KeyDown += OnTextBoxKeyDown;
            }

            if (_btnExpand != null)
            {
                _btnExpand.Click += OnExpandToggleClick;
            }

            UpdateExpandState();
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FormulaTextBox formulaTextBox)
            {
                formulaTextBox.UpdateExpandState();
            }
        }

        private void OnExpandToggleClick(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
        }

        private void UpdateExpandState()
        {
            if (_txtEditor == null)
                return;

            if (IsExpanded)
            {
                _txtEditor.MinHeight = 64;
                _txtEditor.MaxHeight = 64;
                _txtEditor.Height = 64;
                if (_expandIcon != null)
                    _expandIcon.Data = System.Windows.Media.Geometry.Parse("M 2 7 L 6 3 L 10 7");
                if (_btnExpand != null)
                    _btnExpand.ToolTip = "Collapse Formula Bar (Ctrl+Shift+U)";
            }
            else
            {
                _txtEditor.MinHeight = 26;
                _txtEditor.MaxHeight = 26;
                _txtEditor.Height = 26;
                if (_expandIcon != null)
                    _expandIcon.Data = System.Windows.Media.Geometry.Parse("M 2 3 L 6 7 L 10 3");
                if (_btnExpand != null)
                    _btnExpand.ToolTip = "Expand Formula Bar (Ctrl+Shift+U)";
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_txtEditor == null)
                return;

            _txtEditor.ScrollToEnd();

            if (Spread == null || !_txtEditor.IsFocused)
                return;

            var activeSheetView = Spread.SheetViews?.ActiveSheetView;
            if (activeSheetView == null)
                return;

            if (Spread.EditingManager.IsEditing)
            {
                var editor = Spread.EditingManager.ActiveEditor as IEditorInfo;
                editor?.SetValue(_txtEditor.Text);
                return;
            }

            Spread.BeginEdit(activeSheetView.ActiveRow, activeSheetView.ActiveColumn);
            (Spread.EditingManager.ActiveEditor as IEditorInfo)?.SetValue(_txtEditor.Text);
            _txtEditor.Focus();
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (Spread == null || _txtEditor == null)
                return;

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.U && (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift)))
            {
                e.Handled = true;
                IsExpanded = !IsExpanded;
                return;
            }

            if (key == Key.Enter && (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
            {
                e.Handled = true;
                int caretIndex = _txtEditor.CaretIndex;
                string currentText = _txtEditor.Text ?? "";
                if (_txtEditor.SelectionLength > 0)
                {
                    currentText = currentText.Remove(_txtEditor.SelectionStart, _txtEditor.SelectionLength);
                    caretIndex = _txtEditor.SelectionStart;
                }
                _txtEditor.Text = currentText.Insert(caretIndex, System.Environment.NewLine);
                _txtEditor.CaretIndex = caretIndex + System.Environment.NewLine.Length;
                _txtEditor.ScrollToEnd();
                return;
            }

            if (key == Key.Enter)
            {
                e.Handled = true;
                CommitCommand?.Execute(null);
                var activeSheetView = Spread.SheetViews?.ActiveSheetView;
                if (activeSheetView != null)
                {
                    Spread.SelectCell(activeSheetView.ActiveRow + 1, activeSheetView.ActiveColumn);
                }
            }
        }

        private void OnCellsSelectionChanged(object sender, CellsSelectionEventArgs e)
        {
            if (_txtEditor == null || e.SheetView?.WorkSheet == null)
                return;

            var workSheet = e.SheetView.WorkSheet;

            var formula = workSheet.GetFormula(e.SheetView.ActiveRow, e.SheetView.ActiveColumn);
            if (!string.IsNullOrEmpty(formula))
            {
                _txtEditor.Text = "=" + formula;
            }
            else
            {
                var value = workSheet.GetValue(e.SheetView.ActiveRow, e.SheetView.ActiveColumn);
                _txtEditor.Text = value?.ToString();
            }

            _txtEditor.ScrollToEnd();
        }

        private static void OnSpreadAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fTextBox = d as FormulaTextBox;
            if (fTextBox == null)
                return;

            if (e.OldValue is Spread oldSpread)
            {
                oldSpread.CellsSelectionChanged -= fTextBox.OnCellsSelectionChanged;
                if (oldSpread.FormulaTextBox == fTextBox)
                    oldSpread.FormulaTextBox = null;
            }

            if (e.NewValue is Spread newSpread)
            {
                newSpread.FormulaTextBox = fTextBox;
                newSpread.CellsSelectionChanged += fTextBox.OnCellsSelectionChanged;
                fTextBox.CommitCommand = new CommitEditCommand(newSpread);
                fTextBox.CancelCommand = new CancelEditCommand(newSpread);
            }
            else
            {
                fTextBox.CommitCommand = null;
                fTextBox.CancelCommand = null;
            }
        }
    }
}
