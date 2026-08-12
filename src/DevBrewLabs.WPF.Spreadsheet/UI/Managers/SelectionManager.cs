using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class SelectionManager : UIManager, ISelectionManager
    {
        public SelectionManager(Spread spread) : base(spread)
        {
           
        }

        public void SelectCell(int row, int col)
        {
            var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
            var workSheet = (WorkSheet)sheetView.WorkSheet;
            
            var anchor = workSheet.GetSpanCellRange(row, col);
            if (anchor != default)
            {
                row = anchor.TopRow;
                col = anchor.LeftColumn;
            }

            sheetView.ActiveRow = row;
            sheetView.ActiveColumn = col;
            SelectRange(row, col, 1, 1);
        }

        public void SelectColumn(int column)
        {
            var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
            var workSheet = sheetView.WorkSheet;
            sheetView.ActiveRow = 0;
            sheetView.ActiveColumn = column;
            SelectRange(0, column, workSheet.RowCount, 1);
        }

        public void SelectColumns(int column, int count)
        {
            var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
            var workSheet = sheetView.WorkSheet;
            SelectRange(0, column, workSheet.RowCount, count);
        }

        public void SelectRow(int row)
        {
            var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
            var workSheet = sheetView.WorkSheet;
            sheetView.ActiveRow = row;
            sheetView.ActiveColumn = 0;
            SelectRange(row, 0, 1, workSheet.ColumnCount);
        }

        public void SelectRows(int row, int count)
        {
            var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
            var workSheet = sheetView.WorkSheet;
            SelectRange(row, 0, count, workSheet.ColumnCount);
        }

        public void SelectRange(CellRange range)
        {
            SelectRange(range.TopRow, range.LeftColumn, range.RowCount, range.ColumnCount);
        }

        public void SelectRange(int row, int column, int rowCount, int columnCount)
        {
            var sheetView = Spread.SheetViews.ActiveSheetView.As<SheetView>();
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

            sheetView.SetSelection(targetRange);

            sheetView.Spread.RaiseCellsSelectionChanged(new CellsSelectionEventArgs()
            {
                SheetView = sheetView,
                Selection = sheetView.Selection
            });
            RefreshInteractionLayers();
        }

        private void RefreshInteractionLayers()
        {
            var cellsInteractionLayer = Spread.SheetViewPane.CellsRegion.GetInteractionLayer();

            if (cellsInteractionLayer != null && cellsInteractionLayer.IsLoaded)
                cellsInteractionLayer.InvalidateVisual();

            var rowHeadersInteractionLayer = Spread.SheetViewPane.RowHeadersRegion.GetInteractionLayer();

            if (rowHeadersInteractionLayer != null && rowHeadersInteractionLayer.IsLoaded)
                rowHeadersInteractionLayer.InvalidateVisual();

            var columnHeadersInteractionLayer = Spread.SheetViewPane.ColumnHeadersRegion.GetInteractionLayer();

            if (columnHeadersInteractionLayer != null && columnHeadersInteractionLayer.IsLoaded)
                columnHeadersInteractionLayer.InvalidateVisual();
        }
    }
}
