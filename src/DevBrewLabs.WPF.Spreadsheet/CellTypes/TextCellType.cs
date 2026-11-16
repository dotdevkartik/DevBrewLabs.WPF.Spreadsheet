using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class TextCellType : BaseCellType
    {
        public static BaseCellType Default { get; } = new TextCellType();

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (value != null)
            {
                bool isString = value is string;
                string text = isString ? (string)value : (formatter != null ? formatter.Format(value) : value.ToString());
                if (!string.IsNullOrEmpty(text))
                {
                    var align = style.HorizontalAlignment;
                    if (align == CellHorizontalAlignment.Auto)
                        align = isString ? CellHorizontalAlignment.Left : CellHorizontalAlignment.Right;

                    renderContext.DrawText(
                        text,
                        cellRect,
                        style.FontFamily,
                        style.FontSize,
                        style.FontWeight,
                        style.FontStyle,
                        style.ForeColor,
                        align,
                        style.VerticalAlignment,
                        style.TextTrimming,
                        style.AllowMultiLineText,
                        padding: style.Padding);
                }
            }
        }

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new TextCellEditor();
        }
    }
}