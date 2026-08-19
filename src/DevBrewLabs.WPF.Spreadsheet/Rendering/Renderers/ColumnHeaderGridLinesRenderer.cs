using DevBrewLabs.Spreadsheet;
using System;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Renderers
{
    internal class ColumnHeaderGridLinesRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            if (context.SheetView.HeadersVisibility != HeadersVisibility.Column 
                && context.SheetView.HeadersVisibility != HeadersVisibility.Both)
            {
                return;
            }

            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = context.ColumnHeaderRows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;
                
                var rowLocation = context.ViewPort.GetHeaderRowLocation(row);
                var y = rowLocation * context.Zoom;
                var scaledRowHeight = rowHeight * context.Zoom;

                guidelines.GuidelinesY.Add(y + context.HalfPenWidth);
                guidelines.GuidelinesY.Add(y + scaledRowHeight + context.HalfPenWidth);

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    int columnWidth = context.ViewPort.GetTemporaryColumnWidth(col) ?? context.Columns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    double colLocation = context.ViewPort.GetTemporaryColumnLocation(col) ?? context.ViewPort.GetColumnLocation(col);

                    var x = (colLocation - context.ViewPort.LeftColumnLocation) * context.Zoom;
                    var scaledColumnWidth = columnWidth * context.Zoom;

                    if (row == topRow)
                    {
                        guidelines.GuidelinesX.Add(x + context.HalfPenWidth);
                        guidelines.GuidelinesX.Add(x + scaledColumnWidth + context.HalfPenWidth);
                    }

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
                    context.DrawRectangle(null, context.GridLinePen, cellRect);
                }

                // Render double vertical lines for hidden columns
                int minCol = Math.Max(0, leftColumn);
                int maxCol = Math.Min(context.Worksheet.ColumnCount - 1, rightColumn + 1);

                for (int col = minCol; col <= maxCol; col++)
                {
                    int currentWidth = context.ViewPort.GetTemporaryColumnWidth(col) ?? context.Columns.GetColumnWidth(col);
                    if (currentWidth == 0)
                    {
                        // Draw double line indicator only for the first hidden column in a contiguous block
                        int prevWidth = col == 0 ? 0 : context.ViewPort.GetTemporaryColumnWidth(col - 1) ?? context.Columns.GetColumnWidth(col - 1);
                        
                        if (col == 0 || prevWidth > 0)
                        {
                            double colLocation = context.ViewPort.GetTemporaryColumnLocation(col) ?? context.ViewPort.GetColumnLocation(col);
                            var x = (colLocation - context.ViewPort.LeftColumnLocation) * context.Zoom;
                            DrawHiddenColumnIndicator(context, x, y, scaledRowHeight);
                        }
                    }
                }
            }

            context.Pop();
        }

        private void DrawHiddenColumnIndicator(RenderContext context, double x, double rowLocation, double rowHeight)
        {
            var defaultStyle = context.Worksheet.WorkBook.GetNamedStyle(StyleKeys.DefaultColumnHeaderStyleKey);

            if (x <= 0)
            {
                context.DrawLine(context.GridLinePen, new Point(x + 3.0, rowLocation), new Point(x + 3.0, rowLocation + rowHeight));
                return;
            }

            double line1X = x - 1.5;
            double line2X = x + 1.5;

            var rectLeft = Math.Min(line1X, line2X) - 0.5;
            var rectWidth = Math.Abs(line2X - line1X) + 1.0;
            var gapRect = new Rect(rectLeft, rowLocation, rectWidth, rowHeight);

            if (defaultStyle != null && defaultStyle.BackColor != DrawingColor.Transparent)
            {
                context.DrawRectangle(defaultStyle.BackColor, null, gapRect);
            }

            context.DrawLine(context.GridLinePen, new Point(line1X, rowLocation), new Point(line1X, rowLocation + rowHeight));
            context.DrawLine(context.GridLinePen, new Point(line2X, rowLocation), new Point(line2X, rowLocation + rowHeight));
        }
    }
}



