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
            double dpi = context.PixelPerDip > 0 ? context.PixelPerDip : 1.0;
            double penThickness = context.GridLinePen != null ? context.GridLinePen.Thickness : 1.0;
            double halfPenDip = penThickness / 2.0;
            double halfPenPx = halfPenDip * dpi;
            double invDpi = 1.0 / dpi;

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = context.ColumnHeaderRows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;
                
                var rowLocation = context.ViewPort.GetHeaderRowLocation(row);
                var rawY = rowLocation * context.ZoomFactor;
                var scaledRowHeight = rowHeight * context.ZoomFactor;

                double y1 = (Math.Round((rawY + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                double y2 = (Math.Round((rawY + scaledRowHeight + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                double snappedRowHeight = y2 - y1;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    if (!context.Columns.IsColumnVisible(col)) continue;

                    int columnWidth = context.Columns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    double colLocation = context.ViewPort.GetColumnLocation(col);

                    var rawX = (colLocation - context.ViewPort.LeftColumnLocation) * context.ZoomFactor;
                    var scaledColumnWidth = columnWidth * context.ZoomFactor;

                    double x1 = (Math.Round((rawX + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                    double x2 = (Math.Round((rawX + scaledColumnWidth + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                    double snappedColWidth = x2 - x1;

                    var cellRect = new Rect(x1, y1, snappedColWidth, snappedRowHeight);
                    context.DrawRectangle(null, context.GridLinePen, cellRect);
                }

                // Render double vertical lines for hidden columns
                int minCol = Math.Max(0, leftColumn);
                int maxCol = Math.Min(context.Worksheet.ColumnCount - 1, rightColumn + 1);

                for (int col = minCol; col <= maxCol; col++)
                {
                    bool isHidden = !context.Columns.IsColumnVisible(col);

                    if (isHidden)
                    {
                        bool isPrevHidden = false;
                        if (col > 0)
                        {
                            isPrevHidden = !context.Columns.IsColumnVisible(col - 1);
                        }
                        
                        if (col == 0 || !isPrevHidden)
                        {
                            double colLocation = context.ViewPort.GetColumnLocation(col);
                            var rawX = (colLocation - context.ViewPort.LeftColumnLocation) * context.ZoomFactor;
                            double x = (Math.Round((rawX + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                            DrawHiddenColumnIndicator(context, x, y1, snappedRowHeight, dpi, halfPenDip, halfPenPx, invDpi);
                        }
                    }
                }
            }
        }

        private void DrawHiddenColumnIndicator(RenderContext context, double x, double rowLocation, double rowHeight, double dpi, double halfPenDip, double halfPenPx, double invDpi)
        {
            var defaultStyle = context.Worksheet.WorkBook.GetNamedStyle(StyleKeys.DefaultColumnHeaderStyleKey);

            if (x <= 0)
            {
                double lineX = (Math.Round((x + 3.0 + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                context.DrawLine(context.GridLinePen, new Point(lineX, rowLocation), new Point(lineX, rowLocation + rowHeight));
                return;
            }

            double rawLine1X = x - 1.5;
            double rawLine2X = x + 1.5;
            double line1X = (Math.Round((rawLine1X + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
            double line2X = (Math.Round((rawLine2X + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;

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



