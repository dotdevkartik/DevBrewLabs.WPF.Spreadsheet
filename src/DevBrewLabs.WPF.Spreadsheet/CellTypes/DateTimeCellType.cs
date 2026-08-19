using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class DateTimeCellType : BaseCellType
    {
        public string Format { get; set; } = "d";

        internal override void DrawCell(RenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (value == null)
                return;

            var align = style.HorizontalAlignment;
            if (align == CellHorizontalAlignment.Auto)
                align = CellHorizontalAlignment.Right;

            DateTime? date = null;
            if (value is DateTime)
                date = (DateTime)value;
            else if (value is string s && DateTime.TryParse(s, out var parsed))
                date = parsed;
            else if (value is double d)
                date = DateTime.FromOADate(d);

            string textToDraw;
            if (date.HasValue)
            {
                textToDraw = date.Value.ToString(Format);
            }
            else
            {
                textToDraw = value.ToString();
            }
            
            TextRenderer.DrawText(renderContext, textToDraw, cellRect, style, align);
        }

        public override EditorBase GetEditor(IStyle style)
        {
            throw new NotImplementedException();
        }
    }
}

