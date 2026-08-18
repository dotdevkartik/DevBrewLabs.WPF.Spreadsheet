using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class CellsRenderer : Renderer
    {
        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = (Worksheet)SheetView.WorkSheet;
            var workBook = (Workbook)workSheet.WorkBook;
            var rows = (Rows)workSheet.Rows;
            var columns = (Columns)workSheet.Columns;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            double penThickness = SheetView.Spread.GridLinePen.Thickness;
            
            var renderContext = new RenderContext(zoom, SheetView.Spread.PixelPerDip, 5.0, true);
            var renderedSkippedAnchors = new System.Collections.Generic.HashSet<(int Row, int Col)>();
            
            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = rows.GetRowHeight(row);
                if (rowHeight == 0) continue;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = columns.GetColumnWidth(col);
                    if (columnWidth == 0) continue;

                    var anchor = workSheet.GetSpanCellRange(row, col);
                    if (anchor != default)
                    {
                        if (!renderedSkippedAnchors.Add((anchor.TopRow, anchor.LeftColumn)))
                            continue; // Already rendered

                        RenderSingleCell(context, workSheet, workBook, rows, columns, anchor.TopRow, anchor.LeftColumn, zoom, penThickness, renderContext);
                        continue;
                    }

                    RenderSingleCell(context, workSheet, workBook, rows, columns, row, col, zoom, penThickness, renderContext);
                }
            }
        }

        private void RenderSingleCell(DrawingContext context, Worksheet workSheet, Workbook workBook, Rows rows, Columns columns, int row, int col, double zoom, double penThickness, RenderContext renderContext)
        {
            var sheetRow = rows.GetItem(row);
            var sheetColumn = columns.GetItem(col);
            var cellType = (BaseCellType)(workSheet.GetCellType(row, col) ?? sheetColumn?.CellType ?? TextCellType.Default);
            object value = workSheet.GetValue(row, col);

            if (value == null && sheetColumn == null && sheetRow == null)
            {
                switch (cellType)
                {
                    case ButtonCellType buttonCellType:
                        value = buttonCellType.Text;
                        break;
                    case CheckBoxCellType checkBoxCellType:
                        break;
                    default:
                        return;
                }
            }

            var unzoomedRect = SheetView.ViewPort.GetCellRect(row, col);
            var x = (unzoomedRect.X - SheetView.ViewPort.LeftColumnLocation) * zoom;
            var y = (unzoomedRect.Y - SheetView.ViewPort.TopRowLocation) * zoom;
            var width = unzoomedRect.Width * zoom;
            var height = unzoomedRect.Height * zoom;

            var cellRect = new Rect(x, y, width - penThickness, height - penThickness);
            var style = workSheet.GetCellStyle(row, col, sheetRow, sheetColumn);
            var formatter = workSheet.GetCellFormatter(row, col, sheetRow, sheetColumn);
            cellType.DrawCell(context, value, style, formatter, cellRect, renderContext);
        }
    }
}

