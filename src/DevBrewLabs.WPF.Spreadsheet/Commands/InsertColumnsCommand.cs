using DevBrewLabs.Spreadsheet;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class InsertColumnsCommand : SpreadCommand
    {
        public InsertColumnsCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            return sheetView != null && sheetView.Selection.IsValid;
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView == null || !sheetView.Selection.IsValid) return;

            var ws = sheetView.WorkSheet;
            int leftCol = sheetView.Selection.LeftColumn;
            int count = sheetView.Selection.ColumnCount;

            var action = new ColumnInsertDeleteAction
            {
                SheetView = sheetView,
                Index = leftCol,
                Count = count,
                IsInsert = true,
                PreviousSelection = sheetView.Selection.Clone(),
                PreOperationSpans = new List<CellRange>(ws.GetAllSpans()),
                PreOperationWorkbookFormulas = GridSnapshotHelper.CaptureWorkbookFormulas(ws.WorkBook)
            };

            ws.Columns.Insert(leftCol, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectColumns(leftCol, count);
            Spread.Refresh();
        }
    }
}
