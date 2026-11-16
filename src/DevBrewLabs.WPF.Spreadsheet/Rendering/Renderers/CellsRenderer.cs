using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class CellsRenderer : RendererBase
    {
        public override void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            bool hasSpans = context.Worksheet.HasSpans;
            System.Collections.Generic.HashSet<(int Row, int Col)> renderedSkippedAnchors = hasSpans
                ? new System.Collections.Generic.HashSet<(int Row, int Col)>()
                : null;

            for (int row = topRow; row <= bottomRow; row++)
            {
                var sheetRow = context.Rows.GetItem(row);
                bool isRowVisible = sheetRow != null ? sheetRow.Visible : context.Worksheet.DefaultRowHeight > 0;
                if (!isRowVisible) continue;

                var rowHeight = sheetRow != null ? sheetRow.Height : context.Worksheet.DefaultRowHeight;
                if (rowHeight == 0) continue;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
                    var sheetColumn = context.Columns.GetItem(col);
                    bool isColVisible = sheetColumn != null ? sheetColumn.Visible : context.Worksheet.DefaultColumnWidth > 0;
                    if (!isColVisible) continue;

                    var columnWidth = sheetColumn != null ? sheetColumn.Width : context.Worksheet.DefaultColumnWidth;
                    if (columnWidth == 0) continue;

                    if (hasSpans)
                    {
                        var anchor = context.Worksheet.GetSpanCellRange(row, col);
                        if (anchor != default)
                        {
                            if (!renderedSkippedAnchors.Add((anchor.TopRow, anchor.LeftColumn)))
                                continue; // Already rendered

                            var anchorRow = anchor.TopRow == row ? sheetRow : context.Rows.GetItem(anchor.TopRow);
                            var anchorCol = anchor.LeftColumn == col ? sheetColumn : context.Columns.GetItem(anchor.LeftColumn);
                            RenderSingleCell(context, anchor.TopRow, anchor.LeftColumn, anchorRow, anchorCol);
                            continue;
                        }
                    }

                    RenderSingleCell(context, row, col, sheetRow, sheetColumn);
                }
            }
        }

        private void RenderSingleCell(RenderContext context, int row, int col, IRow sheetRow, IColumn sheetColumn)
        {
            var cellType = (BaseCellType)(context.Worksheet.GetCellType(row, col) ?? sheetColumn?.CellType ?? TextCellType.Default);
            object value = context.Worksheet.GetValue(row, col);
            IStyle cellStyle = context.Worksheet.GetStyle(row, col);
            string cellStyleName = cellStyle == null ? context.Worksheet.GetStyleName(row, col) : null;

            if (value == null && sheetColumn == null && sheetRow == null && cellStyle == null && string.IsNullOrEmpty(cellStyleName))
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

            var style = context.Worksheet.GetCellStyle(row, col, sheetRow, sheetColumn, cellStyle, cellStyleName);

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
