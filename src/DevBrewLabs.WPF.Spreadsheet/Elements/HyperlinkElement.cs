using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using System;
using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.Elements
{
    /// <summary>
    /// Interactive cell element that provides text-constrained hit-testing, Hand cursor, hover rendering, and click routing for hyperlinks.
    /// </summary>
    public class HyperlinkElement : CellElement
    {
        private readonly HyperlinkCellType _cellType;

        public ISheetView View { get; set; }
        public int Row { get; set; } = -1;
        public int Column { get; set; } = -1;

        public HyperlinkElement(HyperlinkCellType cellType)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
        }

        public override Cursor Cursor => Cursors.Hand;

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            if (cellRect.Width <= 0 || cellRect.Height <= 0)
                return cellRect;

            var ws = View?.WorkSheet;
            var value = (Row >= 0 && Column >= 0) ? ws?.GetValue(Row, Column) : null;
            string displayText = _cellType?.ResolveDisplayText(value);

            if (string.IsNullOrEmpty(displayText))
            {
                displayText = _cellType?.Text ?? _cellType?.LinkAddress;
            }

            if (string.IsNullOrEmpty(displayText))
            {
                return cellRect;
            }

            var sheetRow = (Row >= 0) ? ws?.Rows?.GetItem(Row) : null;
            var sheetColumn = (Column >= 0) ? ws?.Columns?.GetItem(Column) : null;
            var style = (Row >= 0 && Column >= 0) ? ws?.GetCellStyle(Row, Column, sheetRow, sheetColumn) : null;

            var fontFamily = style?.FontFamily ?? new DrawingFontFamily("Calibri");
            double fontSize = style != null ? style.FontSize : 11;
            var fontWeight = style != null ? style.FontWeight : DrawingFontWeight.Normal;
            var fontStyle = style != null ? style.FontStyle : DrawingFontStyle.Normal;

            var fontResources = WpfResourceCache.GetFontResources(fontFamily, fontWeight, fontStyle);
            double scaledFontSize = fontSize * zoom;
            double textWidth = TextMeasurer.MeasureWidth(displayText, scaledFontSize, fontResources.GlyphMetrics);
            double totalHeight = fontResources.GlyphMetrics.Height * scaledFontSize;

            double textPadding = 5.0 * zoom;
            double availableWidth = Math.Max(0, cellRect.Width - (2 * textPadding));
            double clampedWidth = Math.Min(textWidth, availableWidth);

            var hAlign = style?.HorizontalAlignment ?? CellHorizontalAlignment.Left;
            if (hAlign == CellHorizontalAlignment.Auto) hAlign = CellHorizontalAlignment.Left;

            double x;
            switch (hAlign)
            {
                case CellHorizontalAlignment.Center:
                    x = cellRect.Left + (cellRect.Width - clampedWidth) / 2;
                    break;
                case CellHorizontalAlignment.Right:
                    x = cellRect.Right - textPadding - clampedWidth;
                    break;
                default: // Left
                    x = cellRect.Left + textPadding;
                    break;
            }

            var vAlign = style?.VerticalAlignment ?? CellVerticalAlignment.Center;
            if (vAlign == CellVerticalAlignment.Auto) vAlign = CellVerticalAlignment.Center;

            double y;
            switch (vAlign)
            {
                case CellVerticalAlignment.Top:
                    y = cellRect.Top + textPadding;
                    break;
                case CellVerticalAlignment.Center:
                    y = cellRect.Top + (cellRect.Height - totalHeight) / 2;
                    break;
                default: // Bottom
                    y = cellRect.Bottom - textPadding - totalHeight;
                    break;
            }

            double left = Math.Max(cellRect.Left, x);
            double top = Math.Max(cellRect.Top, y);
            double width = Math.Min(cellRect.Right - left, clampedWidth);
            double height = Math.Min(cellRect.Bottom - top, totalHeight);

            if (width <= 0 || height <= 0)
                return Rect.Empty;

            // Comfortable 2px hit-testing padding around the text glyphs and underline
            double marginX = 2.0 * zoom;
            double marginY = 2.0 * zoom;

            double boundsLeft = Math.Max(cellRect.Left, left - marginX);
            double boundsTop = Math.Max(cellRect.Top, top - marginY);
            double boundsRight = Math.Min(cellRect.Right, left + width + marginX);
            double boundsBottom = Math.Min(cellRect.Bottom, top + height + marginY);

            return new Rect(boundsLeft, boundsTop, Math.Max(0, boundsRight - boundsLeft), Math.Max(0, boundsBottom - boundsTop));
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            if (state == CellElementState.Normal) return;

            _cellType?.DrawHoverOrPressed(context, bounds, state, row, col);
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            if (_cellType != null)
            {
                _cellType.OnClick(view, row, col);
            }
            else
            {
                base.OnClick(view, row, col);
            }
        }
    }
}
