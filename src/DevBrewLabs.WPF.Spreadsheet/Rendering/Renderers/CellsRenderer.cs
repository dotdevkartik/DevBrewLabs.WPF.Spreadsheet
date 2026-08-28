using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System.Windows;

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

            var cellRect = context.GetCellRect(row, col);

            var style = context.Worksheet.GetCellStyle(row, col, sheetRow, sheetColumn);

            bool allowFiltering = context.View.Spread.AllowFiltering;

            bool isFilterHeader = allowFiltering && context.AutoFilter != null && context.AutoFilter.IsFilterHeaderCell(row, col);

            double filterButtonWidth = 16 * context.ZoomFactor;
            var textRect = cellRect;
            if (isFilterHeader)
            {
                textRect = new Rect(cellRect.X, cellRect.Y, System.Math.Max(0, cellRect.Width - filterButtonWidth), cellRect.Height);
            }

            var formatter = context.Worksheet.GetCellFormatter(row, col, sheetRow, sheetColumn);
            cellType.DrawCell(context, value, style, formatter, textRect);
        }
    }
}
