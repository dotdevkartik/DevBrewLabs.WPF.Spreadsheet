using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RowHeadersRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            AdjustHeaderWidth(context, topRow, leftColumn, bottomRow, rightColumn);

            var renderedSkippedAnchors = new HashSet<(int Row, int Col)>();

            for (int row = topRow; row <= bottomRow; row++)
            {
                if (!context.Rows.IsRowVisible(row)) continue;
                
                int rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var columnWidth = context.RowHeaderColumns.GetColumnWidth(col);
                    if (columnWidth == 0)
                        continue;

                    var anchor = context.RowHeaders.GetSpanCellRange(row, col);
                    if (anchor != default)
                    {
                        if (!renderedSkippedAnchors.Add((anchor.TopRow, anchor.LeftColumn)))
                            continue;

                        RenderCell(context, anchor.TopRow, anchor.LeftColumn, anchor.RowCount, anchor.ColumnCount);
                        continue;
                    }

                    RenderCell(context, row, col, 1, 1);
                }
            }
        }

        private void RenderCell(RenderContext context, int row, int col, int rowSpan, int colSpan)
        {
            var sheetRow = context.Rows.GetItem(row);
            var sheetColumn = context.RowHeaderColumns.GetItem(col);
            double rowLocation = context.ViewPort.GetRowLocation(row);
            var y = (rowLocation - context.ViewPort.TopRowLocation) * context.ZoomFactor;

            double totalHeight = 0;
            for (int r = 0; r < rowSpan; r++)
            {
                if (context.Rows.IsRowVisible(row + r))
                    totalHeight += context.Rows.GetRowHeight(row + r);
            }
            var scaledRowHeight = totalHeight * context.ZoomFactor;

            var colLocation = context.ViewPort.GetHeaderColumnLocation(col);
            var x = colLocation * context.ZoomFactor;

            double totalWidth = 0;
            for (int c = 0; c < colSpan; c++)
            {
                totalWidth += context.RowHeaderColumns.GetColumnWidth(col + c);
            }
            var scaledColumnWidth = totalWidth * context.ZoomFactor;

            var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);
            var style = context.Worksheet.GetRowHeaderCellStyle(row, col, sheetRow, sheetColumn);
            var cellValue = context.RowHeaders.GetValue(row, col);

            DrawRowHeaderCell(context, row, cellValue, style, cellRect);
        }

        private void AdjustHeaderWidth(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            for (int col = leftColumn; col <= rightColumn; col++)
            {
                int headerWidth = context.RowHeaderColumns.GetColumnWidth(col);
                int defaultColumnWidth = context.RowHeaders.DefaultColumnWidth;

                for (int row = topRow; row <= bottomRow; row++)
                {
                    if (!context.Rows.IsRowVisible(row)) continue;

                    var sheetColumn = context.RowHeaderColumns.GetItem(col);
                    var sheetRow = context.Rows.GetItem(row);
                    var style = context.Worksheet.GetRowHeaderCellStyle(row, col, sheetRow, sheetColumn);
                    var cellValue = context.RowHeaders.GetValue(row, col);
                    var textWidth = TextMeasurer
                        .MeasureWidth(cellValue != null ? cellValue.ToString() : (row + 1).ToString(), style.FontSize, Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics);
                    textWidth += 10;

                    if (textWidth > headerWidth || (textWidth < headerWidth && textWidth > defaultColumnWidth))
                        headerWidth = (int)System.Math.Ceiling(textWidth);
                }

                if (headerWidth != context.RowHeaderColumns.GetColumnWidth(col))
                {
                    context.RowHeaderColumns[col].Width = headerWidth;
                    context.View.Spread.UpdateHeadersSize();
                }
            }
        }

        private void DrawRowHeaderCell(RenderContext context, int row, object cellValue, IStyle style, Rect cellRect)
        {
            Brush backGroundBrush = WpfResourceCache.GetBrush(style.BackColor);
            DrawingFontWeight fontWeight = style.FontWeight;
            DrawingColor foreColor = style.ForeColor;

            if (context.Selection.ContainsRow(row))
            {
                var selection = context.Selection;
                bool isFullRow = selection.ColumnCount == context.Worksheet.ColumnCount;

                var headerBrush = isFullRow
                    ? context.SelectedHeaderBackground
                    : context.RangeSelectedHeaderBackground;

                if (headerBrush != null)
                {
                    backGroundBrush = headerBrush;
                }

                if (isFullRow)
                {
                    fontWeight = DrawingFontWeight.Bold;

                    var selectedForeground = context.SelectedHeaderForeground;
                    if (selectedForeground is SolidColorBrush scb)
                    {
                        foreColor = DrawingColor.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
                    }
                }
            }

            if (context.HeaderHoverManager.HoveredRow == row)
            {
                var hoverBrush = context.View.Spread?.HeaderHoverManager?.HoveredRow == row
                    ? context.View.Spread?.MouseHoverHeaderBackground
                    : null;

                if (hoverBrush != null)
                {
                    backGroundBrush = hoverBrush;
                }
            }

            context.DrawRectangle(backGroundBrush, null, cellRect);

            string text = cellValue != null ? cellValue.ToString() : (row + 1).ToString();
            context.DrawText(
                text,
                cellRect,
                style.FontFamily,
                style.FontSize,
                fontWeight,
                style.FontStyle,
                foreColor,
                style.HorizontalAlignment,
                style.VerticalAlignment,
                style.TextTrimming,
                style.AllowMultiLineText);
        }
    }
}
