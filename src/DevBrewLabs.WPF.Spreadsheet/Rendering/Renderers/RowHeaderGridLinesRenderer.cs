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
            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int row = topRow; row <= bottomRow; row++)
            {
                if (!context.Rows.IsRowVisible(row)) continue;

                int rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                double rowLocation = context.ViewPort.GetRowLocation(row);

                var y = (rowLocation - context.ViewPort.TopRowLocation) * context.Zoom;
                var scaledRowHeight = rowHeight * context.Zoom;

                guidelines.GuidelinesY.Add(y + context.HalfPenWidth);
                guidelines.GuidelinesY.Add(y + scaledRowHeight + context.HalfPenWidth);

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;

                    var colLocation = context.ViewPort.GetHeaderColumnLocation(col);
                    var x = colLocation * context.Zoom;
                    var scaledColumnWidth = columnWidth * context.Zoom;

                    if (row == topRow)
                    {
                        guidelines.GuidelinesX.Add(x + context.HalfPenWidth);
                        guidelines.GuidelinesX.Add(x + scaledColumnWidth + context.HalfPenWidth);
                    }

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
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
                        var y = (rowLocation - context.ViewPort.TopRowLocation) * context.Zoom;
                        DrawHiddenRowIndicator(context, y, leftColumn, rightColumn);
                    }
                }
            }

            context.Pop();
        }

        private void DrawHiddenRowIndicator(RenderContext context, double y, int leftColumn, int rightColumn)
        {
            var defaultStyle = context.Worksheet.WorkBook.GetNamedStyle(StyleKeys.DefaultRowHeaderStyleKey);

            if (y <= 0)
            {
                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;
                    var colLocation = context.ViewPort.GetHeaderColumnLocation(col) * context.Zoom;
                    var scaledColumnWidth = columnWidth * context.Zoom;

                    context.DrawLine(context.GridLinePen, new Point(colLocation, y + 3.0), new Point(colLocation + scaledColumnWidth, y + 3.0));
                }
                return;
            }

            double line1Y = y - 1.5;
            double line2Y = y + 1.5;

            var rectTop = Math.Min(line1Y, line2Y) - 0.5;
            var rectHeight = Math.Abs(line2Y - line1Y) + 1.0;

            for (int col = leftColumn; col <= rightColumn; col++)
            {
                var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                if (columnWidth == 0)
                    continue;
                var colLocation = context.ViewPort.GetHeaderColumnLocation(col) * context.Zoom;
                var scaledColumnWidth = columnWidth * context.Zoom;
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
