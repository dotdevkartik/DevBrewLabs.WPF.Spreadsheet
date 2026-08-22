using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class TextCellType : BaseCellType
    {
        public static BaseCellType Default { get; } = new TextCellType();

        internal override void DrawCell(RenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

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

                TextRenderer.DrawText(
                    renderContext,
                    (string)value,
                    cellRect,
                    style.FontFamily,
                    style.FontSize,
                    style.FontWeight,
                    style.FontStyle,
                    style.ForeColor,
                    align,
                    style.VerticalAlignment,
                    style.TextTrimming,
                    style.AllowMultiLineText);
            }
        }

        public override EditorBase GetEditor(IStyle style)
        {
            var editor = new TextEditor
            {
                FontFamily = WpfResourceCache.ToWpfFontFamily(style.FontFamily),
                Foreground = WpfResourceCache.GetBrush(style.ForeColor),
                Background = WpfResourceCache.GetBrush(style.BackColor),
                FontWeight = WpfResourceCache.ToWpfFontWeight(style.FontWeight),
                FontStyle = WpfResourceCache.ToWpfFontStyle(style.FontStyle),
                FontSize = style.FontSize
            };
            return editor;
        }
    }
}








