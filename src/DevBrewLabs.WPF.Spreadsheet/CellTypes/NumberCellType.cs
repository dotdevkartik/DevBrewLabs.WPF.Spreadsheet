using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class NumberCellType : BaseCellType
    {
        public string Format { get; set; }

        internal override void DrawCell(RenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (value == null)
                return;

            var align = style.HorizontalAlignment;
            if (align == CellHorizontalAlignment.Auto)
                align = CellHorizontalAlignment.Right;

            string textToDraw;
            if (!string.IsNullOrEmpty(Format))
                textToDraw = string.Format($"{{0:{Format}}}", value);
            else
                textToDraw = formatter.Format(value);

            TextRenderer.DrawText(renderContext, textToDraw, cellRect, style, align);
        }

        /// <inheritdoc/>
        public override EditorBase GetEditor(IStyle style)
        {
            var editor = new NumericEditor() { TextAlignment = TextAlignment.Right };
            editor.FontFamily = Styling.WpfResourceCache.ToWpfFontFamily(style.FontFamily);
            editor.Foreground = Styling.WpfResourceCache.GetBrush(style.ForeColor);
            editor.Background = Styling.WpfResourceCache.GetBrush(style.BackColor);
            editor.FontSize = style.FontSize;
            return editor;
        }
    }
}







