using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RowHeadersRenderer : Renderer
    {
        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = SheetView.WorkSheet;
            var rows = (Rows)workSheet.Rows;
            var columns = (RowHeaderColumns)workSheet.RowHeaders.Columns;
            var cells = workSheet.RowHeaders.Cells;
            var viewport = (ViewPort)SheetView.ViewPort;
            
            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            AdjustHeaderWidth(workSheet, rows, columns, topRow, leftColumn, bottomRow, rightColumn);

            var renderContext = new RenderContext(zoom, SheetView.Spread.PixelPerDip, 5.0, true);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = rows.GetRowHeight(row);

                if (rowHeight == 0)
                    continue;

                var sheetRow = rows.GetItem(row);
                var rowLocation = rows.GetLocation(row);
                var y = (rowLocation - viewport.TopRowLocation) * zoom;
                var scaledRowHeight = rowHeight * zoom;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = columns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    var sheetColumn = columns.GetItem(col);
                    var colLocation = columns.GetLocation(col);
                    var x = colLocation * zoom;
                    var scaledColumnWidth = columnWidth * zoom;

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
                    var style = workSheet.GetRowHeaderCellStyle(row, col, sheetRow, sheetColumn);
                    var cellValue = workSheet.RowHeaders.GetValue(row, col);

                    DrawRowHeaderCell(context, row, cellValue, style, cellRect, renderContext);
                }
            }
        }

        private void AdjustHeaderWidth(IWorkSheet workSheet, Rows rows, RowHeaderColumns columns, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            for (int col = leftColumn; col <= rightColumn; col++)
            {
                int headerWidth = workSheet.RowHeaders.Columns[col].Width;
                int defaultColumnWidth = workSheet.RowHeaders.DefaultColumnWidth;

                for (int row = topRow; row <= bottomRow; row++)
                {
                    var sheetColumn = columns.GetItem(col);
                    var sheetRow = rows.GetItem(row);
                    var style = workSheet.GetRowHeaderCellStyle(row, col, sheetRow, sheetColumn);
                    var cellValue = ((RowHeaders)workSheet.RowHeaders).GetValue(row, col);
                    var textWidth = TextMeasurer
                        .MeasureWidth(cellValue != null ? cellValue.ToString() : (row + 1).ToString(), style.FontSize, Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics);
                    textWidth += 10;

                    if (textWidth > headerWidth || (textWidth < headerWidth && textWidth > defaultColumnWidth))
                        headerWidth = (int)System.Math.Ceiling(textWidth);
                }

                if (headerWidth != workSheet.RowHeaders.Columns[col].Width)
                {
                    workSheet.RowHeaders.Columns[col].Width = headerWidth;
                    SheetView.Spread.SheetViewPane.UpdateHeadersSize();
                }
            }
        }

        private void DrawRowHeaderCell(DrawingContext context, int row, object cellValue, IStyle style, Rect cellRect, RenderContext renderContext)
        {
            context.DrawRectangle(Styling.WpfResourceCache.GetBrush(style.BackColor), null, cellRect);

            if (cellValue != null)
            {
                TextRenderer.DrawText(context, cellValue.ToString(), cellRect, style, renderContext);
            }
            else
            {
                TextRenderer.DrawText(context, (row + 1).ToString(), cellRect, style, renderContext);
            }
        }
    }
}



