using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class TextCellType : BaseCellType
    {
        public static BaseCellType Default { get; } = new TextCellType();

        internal override void DrawCell(DrawingContext context, object value, IStyle style, IFormatter formatter, Rect cellRect, RenderContext renderContext)
        {
            base.DrawCell(context, value, style, formatter, cellRect, renderContext);

            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                var align = style.HorizontalAlignment;
                if (value is string)
                {
                    if (align == CellHorizontalAlignment.Auto)
                        align = CellHorizontalAlignment.Left;           
                }
                else
                {
                    if (align == CellHorizontalAlignment.Auto)
                        align = CellHorizontalAlignment.Right;

                    value = formatter.Format(value);
                }

                TextRenderer.DrawText(context, (string)value, cellRect, style, renderContext, align);
            }
        }

        public override EditorBase GetEditor(IStyle style)
        {
            var editor = new TextEditor();
            editor.FontFamily = Styling.WpfResourceCache.ToWpfFontFamily(style.FontFamily);
            editor.Foreground = Styling.WpfResourceCache.GetBrush(style.ForeColor);
            editor.Background = Styling.WpfResourceCache.GetBrush(style.BackColor);
            editor.FontWeight = Styling.WpfResourceCache.ToWpfFontWeight(style.FontWeight);
            editor.FontStyle = Styling.WpfResourceCache.ToWpfFontStyle(style.FontStyle);
            editor.FontSize = style.FontSize;
            return editor;
        }
    }
}








