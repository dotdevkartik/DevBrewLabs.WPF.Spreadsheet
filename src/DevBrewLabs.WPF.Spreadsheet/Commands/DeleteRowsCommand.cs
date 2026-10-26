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

            var deletedValues = new object[count, ws.ColumnCount];
            var deletedFormulas = new string[count, ws.ColumnCount];
            var deletedHeights = new Dictionary<int, int>();

            for (int r = 0; r < count; r++)
            {
                int actualRow = topRow + r;
                var rowObj = ws.Rows.GetItem(actualRow);
                if (rowObj != null)
                {
                    deletedHeights[actualRow] = rowObj.Height;
                }

                for (int c = 0; c < ws.ColumnCount; c++)
                {
                    deletedValues[r, c] = ws.GetValue(actualRow, c);
                    deletedFormulas[r, c] = ws.GetFormula(actualRow, c);
                }
            }

            var action = new RowInsertDeleteAction
            {
                SheetView = sheetView,
                Index = topRow,
                Count = count,
                IsInsert = false,
                PreviousSelection = sheetView.Selection.Clone(),
                DeletedValues = deletedValues,
                DeletedFormulas = deletedFormulas,
                DeletedRowHeights = deletedHeights
            };

            ws.Rows.Remove(topRow, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectCell(Math.Min(topRow, ws.RowCount - 1), sheetView.Selection.LeftColumn);
            Spread.Refresh();
        }
    }
}
