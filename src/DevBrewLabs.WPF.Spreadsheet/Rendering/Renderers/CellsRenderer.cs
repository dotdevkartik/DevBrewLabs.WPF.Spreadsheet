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
                var rowHeight = context.Rows.GetRowHeight(row);
                if (rowHeight == 0) continue;

                for (int col = leftColumn; col <= rightColumn; col++)
                {
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
            var formatter = context.Worksheet.GetCellFormatter(row, col, sheetRow, sheetColumn);
            cellType.DrawCell(context, value, style, formatter, cellRect);
        }
    }
}

