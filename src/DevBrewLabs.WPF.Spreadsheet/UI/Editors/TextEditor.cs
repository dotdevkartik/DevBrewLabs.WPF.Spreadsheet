using DevBrewLabs.Spreadsheet.Utils;
using System;
using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    internal class TextEditor : EditorBase
    {
        public bool IsShowingFormulaSuggestion => SheetView?.Spread?.FormulaSuggestionManager?.IsOpen == true;

        public TextEditor()
        {
            BorderThickness = new Thickness();
            AcceptsReturn = true;
            TextWrapping = TextWrapping.Wrap;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.Enter && (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
            {
                if (!AcceptsReturn)
                    return;

                e.Handled = true;
                int caretIndex = CaretIndex;
                string currentText = Text ?? "";
                if (SelectionLength > 0)
                {
                    currentText = currentText.Remove(SelectionStart, SelectionLength);
                    caretIndex = SelectionStart;
                }
                Text = currentText.Insert(caretIndex, Environment.NewLine);
                CaretIndex = caretIndex + Environment.NewLine.Length;

                if (SheetView != null)
                {
                    double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
                    var cellRect = ((SheetView)SheetView).ViewPort.GetCellRect(Row, Column);
                    int lineCount = TextUtils.GetLineCount(Text);
                    double fontLineHeight = FontSize * 1.3;
                    double requiredHeight = Math.Max(cellRect.Height * zoom - 3, lineCount * fontLineHeight + 6);
                    Height = requiredHeight;
                }
                return;
            }
        }
    }
}
