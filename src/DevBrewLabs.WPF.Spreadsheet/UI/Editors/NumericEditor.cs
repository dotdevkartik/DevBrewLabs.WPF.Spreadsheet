using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    internal class NumericEditor : EditorBase
    {
        public NumericEditor()
        {
            BorderThickness = new Thickness();
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if(!string.IsNullOrEmpty(e.Text))
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
