using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class ButtonCellType : BaseCellType
    {
        public ICellTypeCommand Command { get; set; }
        public string Text { get; set; }

        internal override void DrawCell(RenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            cellRect.Inflate(-3 * renderContext.Zoom, -3 * renderContext.Zoom);
            renderContext.DrawRectangle(DrawingColor.LightGray, null, cellRect);

            if(!string.IsNullOrEmpty(Text))
            {
                var align = style.HorizontalAlignment;
                if (align == CellHorizontalAlignment.Auto)
                    align = CellHorizontalAlignment.Center;

                TextRenderer.DrawText(
                    renderContext,
                    Text,
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
            throw new NotImplementedException();
        }
    }
}

