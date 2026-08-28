using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// In-place numeric editor for spreadsheet cells.
    /// </summary>
    public class NumericCellEditor : TextCellEditor
    {
        public NumericCellEditor()
        {
            TextAlignment = TextAlignment.Right;
        }

        public override void StartEdit(IEditorContext context)
        {
            base.StartEdit(context);
            TextAlignment = TextAlignment.Right;
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if (!string.IsNullOrEmpty(e.Text))
            {
                var character = e.Text[0];
                var ascii = (int)character;

                if (ascii == 46 && Text.Contains("."))
                {
                    e.Handled = true;
                }
                else if (ascii == 45 && (CaretIndex != 0 || Text.Contains("-")))
                {
                    e.Handled = true;
                }
                else if ((ascii < 48 || ascii > 57) && ascii != 46 && ascii != 45)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
