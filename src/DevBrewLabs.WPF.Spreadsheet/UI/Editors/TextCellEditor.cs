using DevBrewLabs.Spreadsheet.Utils;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// In-place text and formula editor for spreadsheet cells.
    /// </summary>
    public class TextCellEditor : TextBox, IFormulaEditor
    {
        private EventHandler _textChanged;
        private IEditorContext _context;

        public FrameworkElement Element => this;
        public TextBox TextBox => this;

        event EventHandler ITextEditor.TextChanged
        {
            add => _textChanged += value;
            remove => _textChanged -= value;
        }

        public TextCellEditor()
        {
            BorderThickness = new Thickness(0);
            AcceptsReturn = true;
            TextWrapping = TextWrapping.Wrap;
        }

        public virtual void StartEdit(IEditorContext context)
        {
            _context = context;

            var style = context.Style;
            if (style != null)
            {
                FontFamily = WpfResourceCache.ToWpfFontFamily(style.FontFamily);
                Foreground = WpfResourceCache.GetBrush(style.ForeColor);
                Background = WpfResourceCache.GetBrush(style.BackColor);
                FontWeight = WpfResourceCache.ToWpfFontWeight(style.FontWeight);
                FontStyle = WpfResourceCache.ToWpfFontStyle(style.FontStyle);
                FontSize = (style.FontSize > 0 ? style.FontSize : 14) * (context.ZoomFactor > 0 ? context.ZoomFactor : 1.0);
                AcceptsReturn = style.AllowMultiLineText;
            }

            if (context.Trigger == EditTrigger.DirectTyping)
            {
                Text = context.InitialInput ?? string.Empty;
            }
            else
            {
                if (!string.IsNullOrEmpty(context.Formula))
                {
                    Text = context.Formula;
                }
                else
                {
                    Text = context.FormattedText ?? context.Value?.ToString() ?? string.Empty;
                }
            }

            CaretIndex = Text?.Length ?? 0;
        }

        public virtual object GetValue()
        {
            return Text;
        }

        public virtual bool Validate(out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public virtual void UpdateLayout(Rect contentRect, double zoomFactor)
        {
            double availableWidth = Math.Max(0, contentRect.Width - 3);
            MinWidth = availableWidth;

            var style = _context?.Style;
            int initialLineCount = TextUtils.GetLineCount(Text);
            if (style != null && style.AllowMultiLineText && initialLineCount > 1)
            {
                double initialLineHeight = FontSize * 1.3;
                Height = Math.Max(contentRect.Height - 3, initialLineCount * initialLineHeight + 6);
            }
            else
            {
                Height = Math.Max(0, contentRect.Height - 3);
            }

            Canvas.SetLeft(this, contentRect.X + 1);
            Canvas.SetTop(this, contentRect.Y + 1);
        }

        public virtual bool HandlesKeyDown(KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.Enter && (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
            {
                if (!AcceptsReturn)
                    return true;

                e.Handled = true;
                int caretIndex = CaretIndex;
                string currentText = Text ?? string.Empty;
                if (SelectionLength > 0)
                {
                    currentText = currentText.Remove(SelectionStart, SelectionLength);
                    caretIndex = SelectionStart;
                }
                Text = currentText.Insert(caretIndex, Environment.NewLine);
                CaretIndex = caretIndex + Environment.NewLine.Length;

                if (_context?.SheetView != null)
                {
                    var sheetView = (SheetView)_context.SheetView;
                    double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;
                    var cellRect = sheetView.ViewPort.GetCellRect(_context.Row, _context.Column);
                    int lineCount = TextUtils.GetLineCount(Text);
                    double fontLineHeight = FontSize * 1.3;
                    double requiredHeight = Math.Max(cellRect.Height * zoom - 3, lineCount * fontLineHeight + 6);
                    Height = requiredHeight;
                }
                return true;
            }

            return false;
        }

        public virtual void EndEdit()
        {
            _context = null;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            _textChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
