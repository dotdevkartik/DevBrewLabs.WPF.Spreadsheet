using DevBrewLabs.Spreadsheet;
using System;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class InsertRowsCommand : SpreadCommand
    {
        public InsertRowsCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            return sheetView != null && sheetView.Selection.IsValid && !sheetView.WorkSheet.IsBound;
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView == null || !sheetView.Selection.IsValid || sheetView.WorkSheet.IsBound) return;

            int topRow = sheetView.Selection.TopRow;
            int count = sheetView.Selection.RowCount;

            var action = new RowInsertDeleteAction
            {
                SheetView = sheetView,
                Index = topRow,
                Count = count,
                IsInsert = true,
                PreviousSelection = sheetView.Selection.Clone()
            };

            sheetView.WorkSheet.Rows.Insert(topRow, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectRows(topRow, count);
            Spread.Refresh();
        }
    }
}
