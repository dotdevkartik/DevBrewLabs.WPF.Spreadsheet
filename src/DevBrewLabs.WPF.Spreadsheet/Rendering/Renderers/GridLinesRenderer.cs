
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class GridLinesRenderer : Renderer
    {
        private void DrawHorizontalGridlines(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = (WorkSheet)SheetView.WorkSheet;
            var rows = (Rows)workSheet.Rows;
            var columns = (Columns)workSheet.Columns;
            var viewport = SheetView.ViewPort;
            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;

            double halfPenWidth = (SheetView.Spread.GridLinePen.Thickness * SheetView.Spread.PixelPerDip) / 2;
            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                var rowLocation = viewport.GetRowLocation(row);
                double y = (rowLocation - viewport.TopRowLocation + rowHeight) * zoom;
                guidelines.GuidelinesY.Add(y + halfPenWidth);

                bool drawing = false;
                double startX = 0;
                double currentX = 0;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var colWidth = columns.GetColumnWidth(col);
                    if (colWidth == 0) continue;

                    double x = (viewport.GetColumnLocation(col) - viewport.LeftColumnLocation) * zoom;
                    double nextX = x + colWidth * zoom;

                    var anchor1 = workSheet.GetSpanCellRange(row, col);
                    var anchor2 = workSheet.GetSpanCellRange(row + 1, col);
                    
                    bool skip = anchor1 != default && anchor2 != default && anchor1.TopRow == anchor2.TopRow && anchor1.LeftColumn == anchor2.LeftColumn;

                    if (!skip)
                    {
                        if (!drawing)
                        {
                            drawing = true;
                            startX = (col == leftColumn) ? System.Math.Min(0, x) : x;
                        }
                    }
                    else
                    {
                        if (drawing)
                        {
                            drawing = false;
                            context.DrawLine(SheetView.Spread.GridLinePen, new Point(startX, y), new Point(x, y));
                        }
                    }
                    currentX = nextX;
                }

                if (drawing)
                {
                    context.DrawLine(SheetView.Spread.GridLinePen, new Point(startX, y), new Point(currentX, y));
                }
            }

            context.Pop();
        }

        private void DrawVerticalGridlines(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = (WorkSheet)SheetView.WorkSheet;
            var rows = (Rows)workSheet.Rows;
            var columns = (Columns)workSheet.Columns;
            var viewport = SheetView.ViewPort;
            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;

            double halfPenWidth = (SheetView.Spread.GridLinePen.Thickness * SheetView.Spread.PixelPerDip) / 2;
            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int col = leftColumn; col <= rightColumn; col++)
            {
                var columnWidth = columns.GetColumnWidth(col);
                if (columnWidth == 0)
                    continue;

                var colLocation = viewport.GetColumnLocation(col);
                double x = (colLocation - viewport.LeftColumnLocation + columnWidth) * zoom;
                guidelines.GuidelinesX.Add(x + halfPenWidth);

                bool drawing = false;
                double startY = 0;
                double currentY = 0;

                for (int row = topRow; row <= bottomRow; row++)
                {
                    var rowHeight = rows.GetRowHeight(row);
                    if (rowHeight == 0) continue;

                    double y = (viewport.GetRowLocation(row) - viewport.TopRowLocation) * zoom;
                    double nextY = y + rowHeight * zoom;

                    var anchor1 = workSheet.GetSpanCellRange(row, col);
                    var anchor2 = workSheet.GetSpanCellRange(row, col + 1);
                    
                    bool skip = anchor1 != default && anchor2 != default && anchor1.TopRow == anchor2.TopRow && anchor1.LeftColumn == anchor2.LeftColumn;

                    if (!skip)
                    {
                        if (!drawing)
                        {
                            drawing = true;
                            startY = (row == topRow) ? System.Math.Min(0, y) : y;
                        }
                    }
                    else
                    {
                        if (drawing)
                        {
                            drawing = false;
                            context.DrawLine(SheetView.Spread.GridLinePen, new Point(x, startY), new Point(x, y));
                        }
                    }
                    currentY = nextY;
                }

                if (drawing)
                {
                    context.DrawLine(SheetView.Spread.GridLinePen, new Point(x, startY), new Point(x, currentY));
                }
            }

            context.Pop();
        }

        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            switch (SheetView.GridLineVisibility)
            {
                case GridLineVisibility.Vertical:
                    DrawVerticalGridlines(context, topRow, leftColumn, bottomRow, rightColumn);
                    break;

                case GridLineVisibility.Horizontal:
                    DrawHorizontalGridlines(context, topRow, leftColumn, bottomRow, rightColumn);
                    break;

                case GridLineVisibility.Both:
                    DrawVerticalGridlines(context, topRow, leftColumn, bottomRow, rightColumn);
                    DrawHorizontalGridlines(context, topRow, leftColumn, bottomRow, rightColumn);
                    break;
            }
        }
    }
}
