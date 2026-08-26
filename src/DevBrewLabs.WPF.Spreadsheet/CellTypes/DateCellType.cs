using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Cell type for date values supporting formatted display, inline text editing, and calendar dropdown selection.
    /// </summary>
    public class DateCellType : BaseCellType
    {
        public string Format { get; set; } = "d";

        /// <summary>
        /// Gets or sets whether the calendar dropdown button is displayed on the cell.
        /// </summary>
        public bool ShowDropDownButton { get; set; } = true;

        /// <summary>
        /// Gets the width of the dropdown button in device-independent units (before zoom scaling).
        /// </summary>
        public virtual double DropDownButtonWidth => 18.0;

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (ShowDropDownButton)
            {
                yield return DatePickerButton.Instance;
            }
        }

        public override Rect GetContentRect(ISheetView view, int row, int col, Rect cellRect, double zoom)
        {
            var rect = base.GetContentRect(view, row, col, cellRect, zoom);
            if (ShowDropDownButton)
            {
                double width = DropDownButtonWidth * zoom;
                return new Rect(rect.X, rect.Y, Math.Max(0, rect.Width - width), rect.Height);
            }

            return rect;
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (value == null)
                return;

            var contentRect = GetContentRect(renderContext.SheetView, -1, -1, cellRect, renderContext.Zoom);

            var align = style.HorizontalAlignment;
            if (align == CellHorizontalAlignment.Auto)
                align = CellHorizontalAlignment.Right;

            DateTime? date = null;
            if (value is DateTime dt)
                date = dt;
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

            renderContext.DrawText(
                textToDraw,
                contentRect,
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

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new DateCellEditor { Format = Format };
        }
    }
}
