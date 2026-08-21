using DevBrewLabs.Spreadsheet;
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
                int? tempHeight = context.ViewPort.GetTemporaryRowHeight(row);
                if (tempHeight == 0) continue;
                if (tempHeight == null && !context.Rows.IsRowVisible(row)) continue;

                var rowHeight = tempHeight ?? context.Rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                var rowLocation = context.ViewPort.GetTemporaryRowLocation(row) ?? context.ViewPort.GetRowLocation(row);
                double y = (rowLocation - context.ViewPort.TopRowLocation + rowHeight) * context.Zoom;
                guidelines.GuidelinesY.Add(y + context.HalfPenWidth);

                bool drawing = false;
                double startX = 0;
                double currentX = 0;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    int? tempWidth = context.ViewPort.GetTemporaryColumnWidth(col);
                    if (tempWidth == 0) continue;
                    if (tempWidth == null && !context.Columns.IsColumnVisible(col)) continue;

                    var colWidth = tempWidth ?? context.Columns.GetColumnWidth(col);
                    if (colWidth == 0) continue;

                    double x = ((context.ViewPort.GetTemporaryColumnLocation(col) ?? context.ViewPort.GetColumnLocation(col)) - context.ViewPort.LeftColumnLocation) * context.Zoom;
                    double nextX = x + colWidth * context.Zoom;

                    var anchor1 = context.Worksheet.GetSpanCellRange(row, col);
                    
                    bool skip = false;
                    if (anchor1 != default && row < anchor1.BottomRow)
                    {
                        int nextRow = row + 1;
                        while (nextRow <= anchor1.BottomRow && !context.Rows.IsRowVisible(nextRow))
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
                int? tempWidth = context.ViewPort.GetTemporaryColumnWidth(col);
                if (tempWidth == 0) continue;
                if (tempWidth == null && !context.Columns.IsColumnVisible(col)) continue;

                var columnWidth = tempWidth ?? context.Columns.GetColumnWidth(col);
                if (columnWidth == 0)
                    continue;

                var colLocation = context.ViewPort.GetTemporaryColumnLocation(col) ?? context.ViewPort.GetColumnLocation(col);
                double x = (colLocation - context.ViewPort.LeftColumnLocation + columnWidth) * context.Zoom;
                guidelines.GuidelinesX.Add(x + context.HalfPenWidth);

                bool drawing = false;
                double startY = 0;
                double currentY = 0;

                for (int row = topRow; row <= bottomRow; row++)
                {
                    int? tempHeight = context.ViewPort.GetTemporaryRowHeight(row);
                    if (tempHeight == 0) continue;
                    if (tempHeight == null && !context.Rows.IsRowVisible(row)) continue;

                    var rowHeight = tempHeight ?? context.Rows.GetRowHeight(row);
                    if (rowHeight == 0) continue;

                    double y = ((context.ViewPort.GetTemporaryRowLocation(row) ?? context.ViewPort.GetRowLocation(row)) - context.ViewPort.TopRowLocation) * context.Zoom;
                    double nextY = y + rowHeight * context.Zoom;

                    var anchor1 = context.Worksheet.GetSpanCellRange(row, col);
                    
                    bool skip = false;
                    if (anchor1 != default && col < anchor1.RightColumn)
                    {
                        int nextCol = col + 1;
                        while (nextCol <= anchor1.RightColumn && !context.Columns.IsColumnVisible(nextCol))
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
