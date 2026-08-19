
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class GridLinesRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            switch (context.SheetView.GridLineVisibility)
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

        private void DrawHorizontalGridlines(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                var rowLocation = context.ViewPort.GetRowLocation(row);
                double y = (rowLocation - context.ViewPort.TopRowLocation + rowHeight) * context.Zoom;
                guidelines.GuidelinesY.Add(y + context.HalfPenWidth);

                bool drawing = false;
                double startX = 0;
                double currentX = 0;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var colWidth = context.Columns.GetColumnWidth(col);
                    if (colWidth == 0) continue;

                    double x = (context.ViewPort.GetColumnLocation(col) - context.ViewPort.LeftColumnLocation) * context.Zoom;
                    double nextX = x + colWidth * context.Zoom;

                    var anchor1 = context.Worksheet.GetSpanCellRange(row, col);
                    
                    bool skip = false;
                    if (anchor1 != default && row < anchor1.BottomRow)
                    {
                        int nextRow = row + 1;
                        while (nextRow <= anchor1.BottomRow && context.Rows.GetRowHeight(nextRow) == 0)
                            nextRow++;
                            
                        if (nextRow <= anchor1.BottomRow)
                            skip = true;
                    }

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
                            context.DrawLine(context.GridLinePen, new Point(startX, y), new Point(x, y));
                        }
                    }
                    currentX = nextX;
                }

                if (drawing)
                {
                    context.DrawLine(context.GridLinePen, new Point(startX, y), new Point(currentX, y));
                }
            }

            context.Pop();
        }

        private void DrawVerticalGridlines(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int col = leftColumn; col <= rightColumn; col++)
            {
                var columnWidth = context.Columns.GetColumnWidth(col);
                if (columnWidth == 0)
                    continue;

                var colLocation = context.ViewPort.GetColumnLocation(col);
                double x = (colLocation - context.ViewPort.LeftColumnLocation + columnWidth) * context.Zoom;
                guidelines.GuidelinesX.Add(x + context.HalfPenWidth);

                bool drawing = false;
                double startY = 0;
                double currentY = 0;

                for (int row = topRow; row <= bottomRow; row++)
                {
                    var rowHeight = context.Rows.GetRowHeight(row);
                    if (rowHeight == 0) continue;

                    double y = (context.ViewPort.GetRowLocation(row) - context.ViewPort.TopRowLocation) * context.Zoom;
                    double nextY = y + rowHeight * context.Zoom;

                    var anchor1 = context.Worksheet.GetSpanCellRange(row, col);
                    
                    bool skip = false;
                    if (anchor1 != default && col < anchor1.RightColumn)
                    {
                        int nextCol = col + 1;
                        while (nextCol <= anchor1.RightColumn && context.Columns.GetColumnWidth(nextCol) == 0)
                            nextCol++;
                            
                        if (nextCol <= anchor1.RightColumn)
                            skip = true;
                    }

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
                            context.DrawLine(context.GridLinePen, new Point(x, startY), new Point(x, y));
                        }
                    }
                    currentY = nextY;
                }

                if (drawing)
                {
                    context.DrawLine(context.GridLinePen, new Point(x, startY), new Point(x, currentY));
                }
            }

            context.Pop();
        }
    }
}
