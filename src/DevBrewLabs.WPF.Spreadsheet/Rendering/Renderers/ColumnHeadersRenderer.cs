using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class ColumnHeadersRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = context.ColumnHeaderRows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;
                var headerRow = context.ColumnHeaderRows.GetItem(row);
                var rowLocation = context.ViewPort.GetHeaderRowLocation(row);
                var y = rowLocation * context.Zoom;
                var scaledRowHeight = rowHeight * context.Zoom;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    if (!context.Columns.IsColumnVisible(col)) continue;

                    int columnWidth = context.Columns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    var headerColumn = context.Columns.GetItem(col);
                    double colLocation = context.ViewPort.GetColumnLocation(col);

                    var x = (colLocation - context.ViewPort.LeftColumnLocation) * context.Zoom;
                    var scaledColumnWidth = columnWidth * context.Zoom;

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);

                    var style = context.Worksheet.GetColumnHeaderCellStyle(row, col, headerRow, headerColumn);
                    var cellValue = context.Worksheet.ColumnHeaders.GetValue(row, col);

                    DrawColumnHeaderCell(context, row, col, cellValue, style, cellRect);
                }
            }
        }

        private void DrawColumnHeaderCell(RenderContext context, int row, int column, object cellValue, IStyle style, Rect cellRect)
        {
            context.DrawRectangle(style.BackColor, null, cellRect);

            if (cellValue != null)
            {
                TextRenderer.DrawText(context, cellValue.ToString(), cellRect, style);
            }
            else
            {
                TextRenderer.DrawText(context, RenderingExtensions.GetColumnHeader(column), cellRect, style);
            }
        }      
    }
}
