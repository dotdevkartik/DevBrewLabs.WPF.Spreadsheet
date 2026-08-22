using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class SelectionManager : UIManager
    {
        public SelectionManager(Spread spread) : base(spread)
        {
           
        }

        public void SelectCell(ISheetView sheetView, int row, int col)
        {
            var workSheet = (Worksheet)sheetView.WorkSheet;
            
            var anchor = workSheet.GetSpanCellRange(row, col);
            if (anchor != default)
            {
                row = anchor.TopRow;
                col = anchor.LeftColumn;
            }

            ((SheetView)sheetView).ActiveRow = row;
            ((SheetView)sheetView).ActiveColumn = col;
            SelectRange(sheetView, row, col, 1, 1);
        }

        public void SelectColumn(ISheetView sheetView, int column)
        {
            var workSheet = (Worksheet)sheetView.WorkSheet;
            int activeRow = 0;

            while (activeRow < workSheet.RowCount)
            {
                var anchor = workSheet.GetSpanCellRange(activeRow, column);
                if (anchor != default)
                {
                    activeRow = anchor.BottomRow + 1;
                }
                else
                {
                    break;
                }
            }

            if (activeRow >= workSheet.RowCount)
            {
                activeRow = 0;
            }

            ((SheetView)sheetView).ActiveRow = activeRow;
            ((SheetView)sheetView).ActiveColumn = column;
            SelectRange(sheetView, 0, column, workSheet.RowCount, 1);
        }

        public void SelectColumns(ISheetView sheetView, int column, int count)
        {
            var workSheet = sheetView.WorkSheet;
            SelectRange(sheetView, 0, column, workSheet.RowCount, count);
        }

        public void SelectRow(ISheetView sheetView, int row)
        {
            var workSheet = (Worksheet)sheetView.WorkSheet;
            int activeColumn = 0;

            while (activeColumn < workSheet.ColumnCount)
            {
                var anchor = workSheet.GetSpanCellRange(row, activeColumn);
                if (anchor != default)
                {
                    activeColumn = anchor.RightColumn + 1;
                }
                else
                {
                    break;
                }
            }

            if (activeColumn >= workSheet.ColumnCount)
            {
                activeColumn = 0;
            }

            ((SheetView)sheetView).ActiveRow = row;
            ((SheetView)sheetView).ActiveColumn = activeColumn;
            SelectRange(sheetView, row, 0, 1, workSheet.ColumnCount);
        }

        public void SelectRows(ISheetView sheetView, int row, int count)
        {
            var workSheet = sheetView.WorkSheet;
            SelectRange(sheetView, row, 0, count, workSheet.ColumnCount);
        }

        public void SelectRange(ISheetView sheetView, CellRange range)
        {
            SelectRange(sheetView, range.TopRow, range.LeftColumn, range.RowCount, range.ColumnCount);
        }

        public void SelectRange(ISheetView sheetView, int row, int column, int rowCount, int columnCount)
        {
            var selection = sheetView.Selection;
            var workSheet = (Worksheet)sheetView.WorkSheet;

            if (!workSheet.ContainsRange(row, column, rowCount, columnCount))
                return;

            int targetRow = row;
            int targetCol = column;
            int targetRowCount = rowCount;
            int targetColCount = columnCount;

            switch (sheetView.SelectionMode)
            {
                case SelectionMode.Column:
                case SelectionMode.Columns:
                    targetRow = 0;
                    targetCol = column;
                    targetRowCount = workSheet.RowCount;
                    targetColCount = columnCount;
                    break;

                case SelectionMode.Row:
                case SelectionMode.Rows:
                    targetRow = row;
                    targetCol = 0;
                    targetRowCount = rowCount;
                    targetColCount = workSheet.ColumnCount;
                    break;
            }

            var targetRange = new CellRange(targetRow, targetCol, targetRowCount, targetColCount);

            bool isFullColumn = targetRowCount == workSheet.RowCount;
            bool isFullRow = targetColCount == workSheet.ColumnCount;

            if (!isFullColumn && !isFullRow)
            {
                targetRange = workSheet.ExpandSpanRange(targetRange);
            }

            ((SheetView)sheetView).SetSelection(targetRange);

            sheetView.Spread.RaiseCellsSelectionChanged(new CellsSelectionEventArgs()
            {
                SheetView = sheetView,
                Selection = sheetView.Selection
            });

            Spread.SheetViewHost.RefreshInteractionLayers();
            Spread.SheetViewHost.Draw(rowHeaders: true, columnHeaders: true, cells: false, gridLines: false, topLeft: false);
        }
    }
}
