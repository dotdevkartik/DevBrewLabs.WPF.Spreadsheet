using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class ColumnHeadersRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            for (int row = topRow; row <= bottomRow; row++)
            {
                var rowHeight = context.ColumnHeaderRows.GetRowHeight(row);
                if (rowHeight == 0)
                    continue;
                var headerRow = context.ColumnHeaderRows.GetItem(row);
                var rowLocation = context.ViewPort.GetHeaderRowLocation(row);
                var y = rowLocation * context.ZoomFactor;
                var scaledRowHeight = rowHeight * context.ZoomFactor;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    if (!context.Columns.IsColumnVisible(col)) continue;

                    int columnWidth = context.Columns.GetColumnWidth(col);

                    if (columnWidth == 0)
                        continue;

                    var headerColumn = context.Columns.GetItem(col);
                    double colLocation = context.ViewPort.GetColumnLocation(col);

                    var x = (colLocation - context.ViewPort.LeftColumnLocation) * context.ZoomFactor;
                    var scaledColumnWidth = columnWidth * context.ZoomFactor;

                    var cellRect = new Rect(x, y, scaledColumnWidth, scaledRowHeight);

                    var style = context.Worksheet.GetColumnHeaderCellStyle(row, col, headerRow, headerColumn);
                    var cellValue = context.Worksheet.ColumnHeaders.GetValue(row, col);

                    DrawColumnHeaderCell(context, row, col, cellValue, style, cellRect);
                }
            }
        }

        private void DrawColumnHeaderCell(RenderContext context, int row, int column, object cellValue, IStyle style, Rect cellRect)
        {
            Brush backGroundBrush = WpfResourceCache.GetBrush(style.BackColor);
            DrawingFontWeight fontWeight = style.FontWeight;
            DrawingColor foreColor = style.ForeColor;

            if (context.Selection.ContainsColumn(column))
            {
                var selection = context.Selection;
                bool isFullColumn = selection.RowCount == context.Worksheet.RowCount;

                var headerBrush = isFullColumn
                    ? context.SelectedHeaderBackground
                    : context.RangeSelectedHeaderBackground;

                if (headerBrush != null)
                {
                    backGroundBrush = headerBrush;
                }

                if (isFullColumn)
                {
                    fontWeight = DrawingFontWeight.Bold;

                    var selectedForeground = context.SelectedHeaderForeground;
                    if (selectedForeground is SolidColorBrush scb)
                    {
                        foreColor = DrawingColor.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
                    }
                }
            }

            if (context.HeaderHoverManager.HoveredColumn == column)
            {
                var hoverBrush = context.View.Spread?.MouseHoverHeaderBackground;
                if (hoverBrush != null)
                {
                    backGroundBrush = hoverBrush;
                }
            }

            context.DrawRectangle(backGroundBrush, null, cellRect);

            string text = cellValue != null ? cellValue.ToString() : RenderingExtensions.GetColumnHeader(column);
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
