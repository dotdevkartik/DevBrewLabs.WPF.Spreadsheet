using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using System.Data.Common;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RowHeadersRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            AdjustHeaderWidth(context, topRow, leftColumn, bottomRow, rightColumn);

            for (int row = topRow; row <= bottomRow; row++)
            {
                if (!context.Rows.IsRowVisible(row)) continue;
                
                int rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                var sheetRow = context.Rows.GetItem(row);
                double rowLocation = context.ViewPort.GetRowLocation(row);

                var y = (rowLocation - context.ViewPort.TopRowLocation) * context.ZoomFactor;
                var scaledRowHeight = rowHeight * context.ZoomFactor;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    var sheetColumn = context.RowHeaderColumns.GetItem(col);
                    var colLocation = context.ViewPort.GetHeaderColumnLocation(col);
                    var x = colLocation * context.ZoomFactor;
                    var scaledColumnWidth = columnWidth * context.ZoomFactor;

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
                    var style = context.Worksheet.GetRowHeaderCellStyle(row, col, sheetRow, sheetColumn);
                    var cellValue = context.RowHeaders.GetValue(row, col);

                    DrawRowHeaderCell(context, row, cellValue, style, cellRect);
                }
            }
        }

        private void AdjustHeaderWidth(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            for (int col = leftColumn; col <= rightColumn; col++)
            {
                int headerWidth = context.RowHeaderColumns.GetColumnWidth(col);
                int defaultColumnWidth = context.RowHeaders.DefaultColumnWidth;

                for (int row = topRow; row <= bottomRow; row++)
                {
                    if (!context.Rows.IsRowVisible(row)) continue;

                    var sheetColumn = context.RowHeaderColumns.GetItem(col);
                    var sheetRow = context.Rows.GetItem(row);
                    var style = context.Worksheet.GetRowHeaderCellStyle(row, col, sheetRow, sheetColumn);
                    var cellValue = context.RowHeaders.GetValue(row, col);
                    var textWidth = TextMeasurer
                        .MeasureWidth(cellValue != null ? cellValue.ToString() : (row + 1).ToString(), style.FontSize, Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics);
                    textWidth += 10;

                    if (textWidth > headerWidth || (textWidth < headerWidth && textWidth > defaultColumnWidth))
                        headerWidth = (int)System.Math.Ceiling(textWidth);
                }

                if (headerWidth != context.RowHeaderColumns.GetColumnWidth(col))
                {
                    context.RowHeaderColumns[col].Width = headerWidth;
                    context.View.Spread.UpdateHeadersSize();
                }
            }
        }

        private void DrawRowHeaderCell(RenderContext context, int row, object cellValue, IStyle style, Rect cellRect)
        {
            Brush backGroundBrush = WpfResourceCache.GetBrush(style.BackColor);
            DrawingFontWeight fontWeight = style.FontWeight;
            DrawingColor foreColor = style.ForeColor;

            if (context.Selection.ContainsRow(row))
            {
                var selection = context.Selection;
                bool isFullRow = selection.ColumnCount == context.Worksheet.ColumnCount;

                var headerBrush = isFullRow
                    ? context.SelectedHeaderBackground
                    : context.RangeSelectedHeaderBackground;

                if (headerBrush != null)
                {
                    backGroundBrush = headerBrush;
                }

                if (isFullRow)
                {
                    fontWeight = DrawingFontWeight.Bold;

                    var selectedForeground = context.SelectedHeaderForeground;
                    if (selectedForeground is SolidColorBrush scb)
                    {
                        foreColor = DrawingColor.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
                    }
                }
            }

            if (context.HeaderHoverManager.HoveredRow == row)
            {
                var hoverBrush = context.View.Spread?.HeaderHoverManager?.HoveredRow == row
                    ? context.View.Spread?.MouseHoverHeaderBackground
                    : null;

                if (hoverBrush != null)
                {
                    backGroundBrush = hoverBrush;
                }
            }

            context.DrawRectangle(backGroundBrush, null, cellRect);

            string text = cellValue != null ? cellValue.ToString() : (row + 1).ToString();
            context.DrawText(
                text,
                cellRect,
                style.FontFamily,
                style.FontSize,
                fontWeight,
                style.FontStyle,
                foreColor,
                style.HorizontalAlignment,
                style.VerticalAlignment,
                style.TextTrimming,
                style.AllowMultiLineText);
        }
    }
}
