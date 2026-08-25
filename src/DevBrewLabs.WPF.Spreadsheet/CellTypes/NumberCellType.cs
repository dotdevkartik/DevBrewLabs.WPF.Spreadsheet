using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class NumberCellType : SpinnerCellType
    {
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed value for spinner stepping.
        /// </summary>
        public double Minimum { get; set; } = double.MinValue;

        /// <summary>
        /// Gets or sets the maximum allowed value for spinner stepping.
        /// </summary>
        public double Maximum { get; set; } = double.MaxValue;

        /// <summary>
        /// Gets or sets the stepping increment/decrement for spinner buttons.
        /// </summary>
        public double Step { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets whether spinner values wrap around when exceeding Minimum or Maximum.
        /// </summary>
        public bool SpinWrap { get; set; } = false;

        public override void OnSpin(ISheetView view, int row, int col, SpinDirection direction)
        {
            var worksheet = view?.WorkSheet as Worksheet;
            if (worksheet == null) return;

            object currentObj = worksheet.GetValue(row, col);
            decimal currentVal = 0m;
            if (currentObj != null)
            {
                if (!decimal.TryParse(currentObj.ToString(), out currentVal))
                {
                    currentVal = 0m;
                }
            }

            decimal step = (decimal)Step;
            decimal min = (decimal)Minimum;
            decimal max = (decimal)Maximum;

            decimal nextVal;
            if (direction == SpinDirection.Up)
            {
                nextVal = currentVal + step;
                if (nextVal > max)
                {
                    nextVal = SpinWrap ? min : max;
                }
            }
            else
            {
                nextVal = currentVal - step;
                if (nextVal < min)
                {
                    nextVal = SpinWrap ? max : min;
                }
            }

            worksheet.SetValue(row, col, (double)nextVal);
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

            string textToDraw;
            if (!string.IsNullOrEmpty(Format))
                textToDraw = string.Format($"{{0:{Format}}}", value);
            else
                textToDraw = formatter.Format(value);

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