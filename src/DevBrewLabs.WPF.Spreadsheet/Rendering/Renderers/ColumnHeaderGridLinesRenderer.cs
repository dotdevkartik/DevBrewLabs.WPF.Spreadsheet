using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Renderers
{
    internal class ColumnHeaderGridLinesRenderer : Renderer
    {
        protected override void OnRender(DrawingContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var workSheet = (Worksheet)SheetView.WorkSheet;
            var rows = (ColumnHeaderRows)workSheet.ColumnHeaders.Rows;
            var columns = (Columns)workSheet.Columns;

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
                
                var rowLocation = SheetView.ViewPort.GetHeaderRowLocation(row);
                var y = rowLocation * zoom;
                var scaledRowHeight = rowHeight * zoom;

                guidelines.GuidelinesY.Add(y + halfPenWidth);
                guidelines.GuidelinesY.Add(y + scaledRowHeight + halfPenWidth);

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = columns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;

                    var colLocation = SheetView.ViewPort.GetColumnLocation(col);
                    var x = (colLocation - SheetView.ViewPort.LeftColumnLocation) * zoom;
                    var scaledColumnWidth = columnWidth * zoom;

                    if (row == topRow)
                    {
                        guidelines.GuidelinesX.Add(x + halfPenWidth);
                        guidelines.GuidelinesX.Add(x + scaledColumnWidth + halfPenWidth);
                    }

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
                    context.DrawRectangle(null, pen, cellRect);
                }

                // Render double vertical lines for hidden columns
                int minCol = Math.Max(0, leftColumn);
                int maxCol = Math.Min(workSheet.ColumnCount - 1, rightColumn + 1);

                for (int col = minCol; col <= maxCol; col++)
                {
                    if (columns.GetColumnWidth(col) == 0)
                    {
                        // Draw double line indicator only for the first hidden column in a contiguous block
                        if (col == 0 || columns.GetColumnWidth(col - 1) > 0)
                        {
                            var colLocation = SheetView.ViewPort.GetColumnLocation(col);
                            var x = (colLocation - SheetView.ViewPort.LeftColumnLocation) * zoom;
                            DrawHiddenColumnIndicator(context, x, y, scaledRowHeight, workSheet);
                        }
                    }
                }
            }

            context.Pop();
        }

        private void DrawHiddenColumnIndicator(DrawingContext context, double x, double rowLocation, double rowHeight, Worksheet workSheet)
        {
            var pen = SheetView.Spread.GridLinePen;
            var defaultStyle = workSheet.WorkBook.GetNamedStyle(StyleKeys.DefaultColumnHeaderStyleKey);

            double line1X, line2X;
            if (x <= 0)
            {
                line1X = x + 1.5;
                line2X = x + 4.5;
            }
            else
            {
                line1X = x - 1.5;
                line2X = x + 1.5;
            }

            var rectLeft = Math.Min(line1X, line2X) - 0.5;
            var rectWidth = Math.Abs(line2X - line1X) + 1.0;
            var gapRect = new Rect(rectLeft, rowLocation, rectWidth, rowHeight);

            if (defaultStyle != null && defaultStyle.BackColor != CellColor.Transparent)
            {
                context.DrawRectangle(Styling.WpfResourceCache.GetBrush(defaultStyle.BackColor), null, gapRect);
            }

            context.DrawLine(pen, new Point(line1X, rowLocation), new Point(line1X, rowLocation + rowHeight));
            context.DrawLine(pen, new Point(line2X, rowLocation), new Point(line2X, rowLocation + rowHeight));
        }
    }
}



