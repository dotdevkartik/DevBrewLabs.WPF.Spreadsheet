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

            var deletedValues = new object[ws.RowCount, count];
            var deletedFormulas = new string[ws.RowCount, count];
            var deletedWidths = new Dictionary<int, int>();

            for (int c = 0; c < count; c++)
            {
                int actualCol = leftCol + c;
                var colObj = ws.Columns.GetItem(actualCol);
                if (colObj != null)
                {
                    deletedWidths[actualCol] = colObj.Width;
                }

                for (int r = 0; r < ws.RowCount; r++)
                {
                    deletedValues[r, c] = ws.GetValue(r, actualCol);
                    deletedFormulas[r, c] = ws.GetFormula(r, actualCol);
                }
            }

            var action = new ColumnInsertDeleteAction
            {
                SheetView = sheetView,
                Index = leftCol,
                Count = count,
                IsInsert = false,
                PreviousSelection = sheetView.Selection.Clone(),
                DeletedValues = deletedValues,
                DeletedFormulas = deletedFormulas,
                DeletedColWidths = deletedWidths
            };

            ws.Columns.Remove(leftCol, count);
            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectCell(sheetView.Selection.TopRow, Math.Min(leftCol, ws.ColumnCount - 1));
            Spread.Refresh();
        }
    }
}
