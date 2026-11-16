using DevBrewLabs.Spreadsheet;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class DeleteColumnsCommand : SpreadCommand
    {
        public DeleteColumnsCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            return sheetView != null && sheetView.Selection.IsValid && sheetView.WorkSheet.ColumnCount > sheetView.Selection.ColumnCount;
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView == null || !sheetView.Selection.IsValid) return;

            int leftCol = sheetView.Selection.LeftColumn;
            int count = sheetView.Selection.ColumnCount;
            var ws = sheetView.WorkSheet;

            var deletedCells = new Dictionary<(int Row, int Col), CellSnapshot>();
            foreach (var pos in ws.GetPopulatedCellsInColumnRange(leftCol, count))
            {
                deletedCells[pos] = GridSnapshotHelper.CaptureCellSnapshot(ws, pos.Row, pos.Column);
            }

            var deletedColSnapshots = new Dictionary<int, ColumnDimensionSnapshot>();
            for (int c = 0; c < count; c++)
            {
                int actualCol = leftCol + c;
                var colObj = ws.Columns.GetItem(actualCol);
                if (colObj != null)
                {
                    deletedColSnapshots[actualCol] = new ColumnDimensionSnapshot
                    {
                        Width = colObj.Width,
                        Style = colObj.Style,
                        StyleName = colObj.StyleName,
                        Formatter = colObj.Formatter,
                        CellType = colObj.CellType,
                        DataMap = colObj.DataMap,
                        Locked = colObj.Locked,
                        AllowFiltering = colObj.AllowFiltering
                    };
                }
            }

            var action = new ColumnInsertDeleteAction
            {
                SheetView = sheetView,
                Index = leftCol,
                Count = count,
                IsInsert = false,
                PreviousSelection = sheetView.Selection.Clone(),
                DeletedCells = deletedCells,
                DeletedColumnSnapshots = deletedColSnapshots,
                PreOperationSpans = new List<CellRange>(ws.GetAllSpans()),
                PreOperationWorkbookFormulas = GridSnapshotHelper.CaptureWorkbookFormulas(ws.WorkBook)
            };

            ws.Columns.Remove(leftCol, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectCell(sheetView.Selection.TopRow, Math.Min(leftCol, ws.ColumnCount - 1));
            Spread.Refresh();
        }
    }
}
