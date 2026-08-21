using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class CellsRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            var renderedSkippedAnchors = new System.Collections.Generic.HashSet<(int Row, int Col)>();

            for (int row = topRow; row <= bottomRow; row++)
            {
                if (!context.Rows.IsRowVisible(row)) continue;

                var rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0) continue;
                Debug.WriteLine(rowHeight);

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    if (!context.Columns.IsColumnVisible(col)) continue;

                    var columnWidth = context.Columns.GetColumnWidth(col);
                    if (columnWidth == 0) continue;

                    var anchor = context.Worksheet.GetSpanCellRange(row, col);
                    if (anchor != default)
                    {
                        if (!renderedSkippedAnchors.Add((anchor.TopRow, anchor.LeftColumn)))
                            continue; // Already rendered

                        RenderSingleCell(context, anchor.TopRow, anchor.LeftColumn);
                        continue;
                    }

                    RenderSingleCell(context, row, col);
                }
            }
        }

        private void RenderSingleCell(RenderContext context, int row, int col)
        {
            var sheetRow = context.Rows.GetItem(row);
            var sheetColumn = context.Columns.GetItem(col);
            var cellType = (BaseCellType)(context.Worksheet.GetCellType(row, col) ?? sheetColumn?.CellType ?? TextCellType.Default);
            object value = context.Worksheet.GetValue(row, col);

            if (value == null && sheetColumn == null && sheetRow == null)
            {
                switch (cellType)
                {
                    case ButtonCellType buttonCellType:
                        value = buttonCellType.Text;
                        break;
                    case CheckBoxCellType checkBoxCellType:
                        break;
                    default:
                        return;
                }
            }

            var unzoomedRect = context.ViewPort.GetCellRect(row, col);
            var x = (unzoomedRect.X - context.ViewPort.LeftColumnLocation) * context.Zoom;
            var y = (unzoomedRect.Y - context.ViewPort.TopRowLocation) * context.Zoom;
            var width = unzoomedRect.Width * context.Zoom;
            var height = unzoomedRect.Height * context.Zoom;

            var cellRect = new Rect(x, y, width - context.GridLinePen.Thickness, height - context.GridLinePen.Thickness);

            var style = context.Worksheet.GetCellStyle(row, col, sheetRow, sheetColumn);

            bool allowFiltering = context.SheetView.Spread.AllowFiltering;

            bool isFilterHeader = allowFiltering && context.AutoFilter != null && context.AutoFilter.IsFilterHeaderCell(row, col);

            double filterButtonWidth = 16 * context.Zoom;
            var textRect = cellRect;
            if (isFilterHeader)
            {
                textRect = new Rect(cellRect.X, cellRect.Y, System.Math.Max(0, cellRect.Width - filterButtonWidth), cellRect.Height);
            }

            var formatter = context.Worksheet.GetCellFormatter(row, col, sheetRow, sheetColumn);
            cellType.DrawCell(context, value, style, formatter, textRect);

            if (isFilterHeader)
            {
                var iconRect = new Rect(cellRect.Right - filterButtonWidth, cellRect.Y, filterButtonWidth, cellRect.Height);
                bool isActive = context.AutoFilter.IsColumnFiltered(col);
                DrawFilterIcon(context, iconRect, isActive);
            }
        }

        private void DrawFilterIcon(RenderContext context, Rect rect, bool isActive)
        {
            double iconSize = 8 * context.Zoom;
            double x = rect.X + (rect.Width - iconSize) / 2;
            double y = rect.Y + (rect.Height - iconSize) / 2;
            
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                if (isActive)
                {
                    ctx.BeginFigure(new Point(x, y), true, true);
                    ctx.LineTo(new Point(x + iconSize, y), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.6, y + iconSize * 0.5), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.6, y + iconSize), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.4, y + iconSize * 0.8), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.4, y + iconSize * 0.5), true, true);
                }
                else
                {
                    ctx.BeginFigure(new Point(x, y + iconSize * 0.3), true, true);
                    ctx.LineTo(new Point(x + iconSize, y + iconSize * 0.3), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.5, y + iconSize * 0.8), true, true);
                }
            }
            geometry.Freeze();

            var color = isActive ? DrawingColor.DodgerBlue : DrawingColor.DimGray;
            context.DrawGeometry(color, null, geometry);
        }
    }
}
