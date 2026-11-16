using DevBrewLabs.Spreadsheet;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class DeleteRowsCommand : SpreadCommand
    {
        public DeleteRowsCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            return sheetView != null && sheetView.Selection.IsValid && !sheetView.WorkSheet.IsBound && sheetView.WorkSheet.RowCount > sheetView.Selection.RowCount;
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView == null || !sheetView.Selection.IsValid || sheetView.WorkSheet.IsBound) return;

            int topRow = sheetView.Selection.TopRow;
            int count = sheetView.Selection.RowCount;
            var ws = sheetView.WorkSheet;

            var deletedCells = new Dictionary<(int Row, int Col), CellSnapshot>();
            foreach (var pos in ws.GetPopulatedCellsInRowRange(topRow, count))
            {
                deletedCells[pos] = GridSnapshotHelper.CaptureCellSnapshot(ws, pos.Row, pos.Column);
            }

            var deletedRowSnapshots = new Dictionary<int, RowDimensionSnapshot>();
            for (int r = 0; r < count; r++)
            {
                int actualRow = topRow + r;
                var rowObj = ws.Rows.GetItem(actualRow);
                if (rowObj != null)
                {
                    deletedRowSnapshots[actualRow] = new RowDimensionSnapshot
                    {
                        Height = rowObj.Height,
                        IsHidden = rowObj.IsHidden,
                        Style = rowObj.Style,
                        StyleName = rowObj.StyleName,
                        Formatter = rowObj.Formatter,
                        Locked = rowObj.Locked
                    };
                }
            }

            var action = new RowInsertDeleteAction
            {
                SheetView = sheetView,
                Index = topRow,
                Count = count,
                IsInsert = false,
                PreviousSelection = sheetView.Selection.Clone(),
                DeletedCells = deletedCells,
                DeletedRowSnapshots = deletedRowSnapshots,
                PreOperationSpans = new List<CellRange>(ws.GetAllSpans()),
                PreOperationWorkbookFormulas = GridSnapshotHelper.CaptureWorkbookFormulas(ws.WorkBook)
            };

            ws.Rows.Remove(topRow, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectCell(Math.Min(topRow, ws.RowCount - 1), sheetView.Selection.LeftColumn);
            Spread.Refresh();
        }
    }
}
