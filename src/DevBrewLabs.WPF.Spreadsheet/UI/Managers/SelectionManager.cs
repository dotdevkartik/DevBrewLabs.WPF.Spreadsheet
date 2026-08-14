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
            var workSheet = (WorkSheet)sheetView.WorkSheet;
            
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
            var workSheet = sheetView.WorkSheet;
            ((SheetView)sheetView).ActiveRow = 0;
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
            var workSheet = sheetView.WorkSheet;
            ((SheetView)sheetView).ActiveRow = row;
            ((SheetView)sheetView).ActiveColumn = 0;
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
            var workSheet = (WorkSheet)sheetView.WorkSheet;

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
            targetRange = workSheet.ExpandSpanRange(targetRange);

            ((SheetView)sheetView).SetSelection(targetRange);

            sheetView.Spread.RaiseCellsSelectionChanged(new CellsSelectionEventArgs()
            {
                SheetView = sheetView,
                Selection = sheetView.Selection
            });

            Spread.SheetViewPane.RefreshInteractionLayers();
        }
    }
}
