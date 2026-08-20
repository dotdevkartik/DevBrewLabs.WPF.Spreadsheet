using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RowHeadersRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            if (context.SheetView.HeadersVisibility != HeadersVisibility.Row
                 && context.SheetView.HeadersVisibility != HeadersVisibility.Both)
            {
                return;
            }

            AdjustHeaderWidth(context, topRow, leftColumn, bottomRow, rightColumn);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var sheetRowObj = context.Rows.GetItem(row) as Row;
                if (sheetRowObj != null && !sheetRowObj.Visible) continue;
                
                int rowHeight = context.ViewPort.GetTemporaryRowHeight(row) ?? context.Rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                var sheetRow = context.Rows.GetItem(row);
                double rowLocation = context.ViewPort.GetTemporaryRowLocation(row) ?? context.ViewPort.GetRowLocation(row);

                var y = (rowLocation - context.ViewPort.TopRowLocation) * context.Zoom;
                var scaledRowHeight = rowHeight * context.Zoom;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    var sheetColumn = context.RowHeaderColumns.GetItem(col);
                    var colLocation = context.ViewPort.GetHeaderColumnLocation(col);
                    var x = colLocation * context.Zoom;
                    var scaledColumnWidth = columnWidth * context.Zoom;

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
                    context.SheetView.Spread.SheetViewHost.UpdateHeadersSize();
                }
            }
        }

        private void DrawRowHeaderCell(RenderContext context, int row, object cellValue, IStyle style, Rect cellRect)
        {
            context.DrawRectangle(style.BackColor, null, cellRect);

            if (cellValue != null)
            {
                TextRenderer.DrawText(context, cellValue.ToString(), cellRect, style);
            }
            else
            {
                TextRenderer.DrawText(context, (row + 1).ToString(), cellRect, style);
            }
        }
    }
}



