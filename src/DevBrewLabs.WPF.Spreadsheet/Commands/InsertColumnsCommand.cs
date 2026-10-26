using DevBrewLabs.Spreadsheet;
using System;

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

            int leftCol = sheetView.Selection.LeftColumn;
            int count = sheetView.Selection.ColumnCount;

            var action = new ColumnInsertDeleteAction
            {
                SheetView = sheetView,
                Index = leftCol,
                Count = count,
                IsInsert = true,
                PreviousSelection = sheetView.Selection.Clone()
            };

            sheetView.WorkSheet.Columns.Insert(leftCol, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectColumns(leftCol, count);
            Spread.Refresh();
        }
    }
}
