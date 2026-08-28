using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Renderers
{
    internal class RowHeaderGridLinesRenderer : RendererBase
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
                if (!context.Rows.IsRowVisible(row)) continue;

                int rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                double rowLocation = context.ViewPort.GetRowLocation(row);

                var rawY = (rowLocation - context.ViewPort.TopRowLocation) * context.ZoomFactor;
                var scaledRowHeight = rowHeight * context.ZoomFactor;

                double y1 = (Math.Round((rawY + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                double y2 = (Math.Round((rawY + scaledRowHeight + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                double snappedRowHeight = y2 - y1;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;

                    var colLocation = context.ViewPort.GetHeaderColumnLocation(col);
                    var rawX = colLocation * context.ZoomFactor;
                    var scaledColumnWidth = columnWidth * context.ZoomFactor;

                    double x1 = (Math.Round((rawX + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                    double x2 = (Math.Round((rawX + scaledColumnWidth + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                    double snappedColWidth = x2 - x1;

                    var cellRect = new Rect(x1, y1, snappedColWidth, snappedRowHeight);
                    context.DrawRectangle(null, context.GridLinePen, cellRect);
                }
            }

            // Render double horizontal lines for hidden rows
            int minRow = Math.Max(0, topRow);
            int maxRow = Math.Min(context.Worksheet.RowCount - 1, bottomRow + 1);

            for (int row = minRow; row <= maxRow; row++)
            {
                bool isHidden = !context.Rows.IsRowVisible(row);

                if (isHidden)
                {
                    bool isPrevHidden = false;
                    if (row > 0)
                    {
                        isPrevHidden = !context.Rows.IsRowVisible(row - 1);
                    }
                    
                    if (row == 0 || !isPrevHidden)
                    {
                        double rowLocation = context.ViewPort.GetRowLocation(row);
                        var rawY = (rowLocation - context.ViewPort.TopRowLocation) * context.ZoomFactor;
                        double y = (Math.Round((rawY + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                        DrawHiddenRowIndicator(context, y, leftColumn, rightColumn, dpi, halfPenDip, halfPenPx, invDpi);
                    }
                }
            }
        }

        private void DrawHiddenRowIndicator(RenderContext context, double y, int leftColumn, int rightColumn, double dpi, double halfPenDip, double halfPenPx, double invDpi)
        {
            var defaultStyle = context.Worksheet.WorkBook.GetNamedStyle(StyleKeys.DefaultRowHeaderStyleKey);

            if (y <= 0)
            {
                double lineY = (Math.Round((y + 3.0 + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;
                    var colLocation = context.ViewPort.GetHeaderColumnLocation(col) * context.ZoomFactor;
                    var scaledColumnWidth = columnWidth * context.ZoomFactor;

                    context.DrawLine(context.GridLinePen, new Point(colLocation, lineY), new Point(colLocation + scaledColumnWidth, lineY));
                }
                return;
            }

            double rawLine1Y = y - 1.5;
            double rawLine2Y = y + 1.5;
            double line1Y = (Math.Round((rawLine1Y + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;
            double line2Y = (Math.Round((rawLine2Y + halfPenDip) * dpi, MidpointRounding.AwayFromZero) - halfPenPx) * invDpi;

            var rectTop = Math.Min(line1Y, line2Y) - 0.5;
            var rectHeight = Math.Abs(line2Y - line1Y) + 1.0;

            for (int col = leftColumn; col <= rightColumn; col++)
            {
                var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                if (columnWidth == 0)
                    continue;
                var colLocation = context.ViewPort.GetHeaderColumnLocation(col) * context.ZoomFactor;
                var scaledColumnWidth = columnWidth * context.ZoomFactor;
                var gapRect = new Rect(colLocation, rectTop, scaledColumnWidth, rectHeight);

                if (defaultStyle != null && defaultStyle.BackColor != DrawingColor.Transparent)
                {
                    context.DrawRectangle(defaultStyle.BackColor, null, gapRect);
                }

                context.DrawLine(context.GridLinePen, new Point(colLocation, line1Y), new Point(colLocation + scaledColumnWidth, line1Y));
                context.DrawLine(context.GridLinePen, new Point(colLocation, line2Y), new Point(colLocation + scaledColumnWidth, line2Y));
            }
        }
    }
}
