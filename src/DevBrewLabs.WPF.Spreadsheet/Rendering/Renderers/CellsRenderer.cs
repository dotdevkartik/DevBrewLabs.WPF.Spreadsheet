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
            var workSheet = (WorkSheet)SheetView.WorkSheet;
            var workBook = (WorkBook)workSheet.WorkBook;
            var rows = (Rows)workSheet.Rows;
            var columns = (Columns)workSheet.Columns;
            var cells = (Cells)workSheet.Cells;
            var viewport = (ViewPort)SheetView.ViewPort;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            double penThickness = SheetView.Spread.GridLinePen.Thickness;
            double halfPenWidth = (penThickness * SheetView.Spread.PixelPerDip) / 2;
            
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

                    var columnLocation = columns.GetLocation(col);
                    var x = (columnLocation - viewport.LeftColumnLocation) * zoom;
                    var scaledColumnWidth = columnWidth * zoom;

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
                                //value = false;
                                break;
                            default:
                                continue;
                        }
                    }

                    var cellRect = new Rect(x, y, scaledColumnWidth - penThickness, scaledRowHeight - penThickness);

                    var style = workSheet.GetStyle(row, col);

                    if (style == null)
                    {
                        var styleName = workSheet.GetStyleName(row, col);
                        if (!string.IsNullOrEmpty(styleName))
                        {
                            style = workBook.GetNamedStyle(styleName);
                        }
                        else
                        {
                            style = workBook.PickStyle(sheetColumn, sheetRow, SheetRegion.Cells);
                        }
                    }

                    var formatter = workSheet.GetFormatter(row, col) ?? workSheet.PickFormatter(sheetColumn, sheetRow);
                    cellType.DrawCell(context, value, style, formatter, cellRect, renderContext);
                }
            }
        }
    }
}

