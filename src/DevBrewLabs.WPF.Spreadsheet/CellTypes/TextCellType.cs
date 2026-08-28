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

                renderContext.DrawText(
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

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new TextCellEditor();
        }
    }
}