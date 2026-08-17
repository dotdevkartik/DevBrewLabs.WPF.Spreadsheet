using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Renderers
{
    internal class RowHeaderGridLinesRenderer : Renderer
    {
        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = (Worksheet)SheetView.WorkSheet;
            var rows = (Rows)workSheet.Rows;
            var columns = (RowHeaderColumns)workSheet.RowHeaders.Columns;
            var viewport = SheetView.ViewPort;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            double halfPenWidth = SheetView.Spread.GridLinePen.Thickness * SheetView.Spread.PixelPerDip / 2;
            var pen = SheetView.Spread.GridLinePen;

            GuidelineSet guidelines = new GuidelineSet();
            context.PushGuidelineSet(guidelines);

            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                var rowLocation = viewport.GetRowLocation(row);
                var y = (rowLocation - viewport.TopRowLocation) * zoom;
                var scaledRowHeight = rowHeight * zoom;

                guidelines.GuidelinesY.Add(y + halfPenWidth);
                guidelines.GuidelinesY.Add(y + scaledRowHeight + halfPenWidth);

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = columns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;

                    var colLocation = viewport.GetHeaderColumnLocation(col);
                    var x = colLocation * zoom;
                    var scaledColumnWidth = columnWidth * zoom;

                    if (row == topRow)
                    {
                        guidelines.GuidelinesX.Add(x + halfPenWidth);
                        guidelines.GuidelinesX.Add(x + scaledColumnWidth + halfPenWidth);
                    }

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
                    context.DrawRectangle(null, pen, cellRect);
                }
            }

            // Render double horizontal lines for hidden rows
            int minRow = Math.Max(0, topRow);
            int maxRow = Math.Min(workSheet.RowCount - 1, bottomRow + 1);

            for (int row = minRow; row <= maxRow; row++)
            {
                if (rows.GetRowHeight(row) == 0)
                {
                    if (row == 0 || rows.GetRowHeight(row - 1) > 0)
                    {
                        var rowLocation = viewport.GetRowLocation(row);
                        var y = (rowLocation - viewport.TopRowLocation) * zoom;
                        DrawHiddenRowIndicator(context, y, leftColumn, rightColumn, columns, workSheet, zoom);
                    }
                }
            }

            context.Pop();
        }

        private void DrawHiddenRowIndicator(DrawingContext context, double y, int leftColumn, int rightColumn, RowHeaderColumns columns, Worksheet workSheet, double zoom)
        {
            var pen = SheetView.Spread.GridLinePen;
            var viewPort = SheetView.ViewPort;
            var defaultStyle = workSheet.WorkBook.GetNamedStyle(StyleKeys.DefaultRowHeaderStyleKey);

            double line1Y, line2Y;
            if (y <= 0)
            {
                line1Y = y + 1.5;
                line2Y = y + 4.5;
            }
            else
            {
                line1Y = y - 1.5;
                line2Y = y + 1.5;
            }

            var rectTop = Math.Min(line1Y, line2Y) - 0.5;
            var rectHeight = Math.Abs(line2Y - line1Y) + 1.0;

            for (int col = leftColumn; col <= rightColumn; col++)
            {
                var columnWidth = columns.GetColumnWidth(col);
                if (columnWidth == 0)
                    continue;
                var colLocation = viewPort.GetHeaderColumnLocation(col) * zoom;
                var scaledColumnWidth = columnWidth * zoom;
                var gapRect = new Rect(colLocation, rectTop, scaledColumnWidth, rectHeight);

                if (defaultStyle != null && defaultStyle.BackColor != CellColor.Transparent)
                {
                    context.DrawRectangle(Styling.WpfResourceCache.GetBrush(defaultStyle.BackColor), null, gapRect);
                }

                context.DrawLine(pen, new Point(colLocation, line1Y), new Point(colLocation + scaledColumnWidth, line1Y));
                context.DrawLine(pen, new Point(colLocation, line2Y), new Point(colLocation + scaledColumnWidth, line2Y));
            }
        }
    }
}



