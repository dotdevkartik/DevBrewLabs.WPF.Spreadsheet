using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class ColumnHeadersRenderer : Renderer
    {
        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = SheetView.WorkSheet;
            var workBook = (WorkBook)workSheet.WorkBook;
            var rows = (ColumnHeaderRows)workSheet.ColumnHeaders.Rows;
            var columns = (Columns)workSheet.Columns;
            var cells = (ColumnHeaderCells)workSheet.ColumnHeaders.Cells;
            var viewport = (ViewPort)SheetView.ViewPort;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var renderContext = new RenderContext(zoom, SheetView.Spread.PixelPerDip, 5.0, true);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;
                var sheetRow = rows.GetItem(row);
                var rowLocation = rows.GetLocation(row);
                var y = rowLocation * zoom;
                var scaledRowHeight = rowHeight * zoom;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = columns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;

                    var sheetColumn = columns.GetItem(col);
                    var colLocation = columns.GetLocation(col);
                    var x = (colLocation - viewport.LeftColumnLocation) * zoom;
                    var scaledColumnWidth = columnWidth * zoom;

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);

                    var baseStyle = ((ColumnHeaders)workSheet.ColumnHeaders).GetStyle(row, col) ?? workBook.PickStyle(sheetColumn, sheetRow, SheetRegion.ColumnHeader);
                    var style = baseStyle;
                    var cellValue = ((ColumnHeaders)workSheet.ColumnHeaders).GetValue(row, col);

                    DrawColumnHeaderCell(context, row, col, cellValue, style, cellRect, renderContext);
                }
            }
        }

        private void DrawColumnHeaderCell(DrawingContext context, int row, int column, object cellValue, IStyle style, Rect cellRect, RenderContext renderContext)
        {
            context.DrawRectangle(Styling.WpfResourceCache.GetBrush(style.BackColor), null, cellRect);
            if (cellValue != null)
            {
                TextRenderer.DrawText(context, cellValue.ToString(), cellRect, style, renderContext);
            }
            else
            {
                TextRenderer.DrawText(context, RenderingExtensions.GetColumnHeader(column), cellRect, style, renderContext);
            }
        }      
    }
}


