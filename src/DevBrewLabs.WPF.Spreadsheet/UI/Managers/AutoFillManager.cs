using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Core;
using System;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class AutoFillManager : UIManager
    {
        public AutoFillManager(Spread spread) : base(spread)
        {
        }

        public void ExecuteAutoFill(SheetView sheetView, CellRange sourceRange, CellRange targetRange, bool isCtrlPressed)
        {
            if (sheetView?.WorkSheet == null || !sourceRange.IsValid || !targetRange.IsValid || sourceRange == targetRange)
                return;

            var ws = sheetView.WorkSheet;
            if (!AutoFillHelper.TryGetFillDirection(sourceRange, targetRange, out var direction))
                return;

            var fillArea = AutoFillHelper.GetFillArea(sourceRange, targetRange, direction);
            if (!fillArea.IsValid)
                return;

            var oldValues = new object[fillArea.RowCount, fillArea.ColumnCount];
            var oldFormulas = new string[fillArea.RowCount, fillArea.ColumnCount];
            for (int r = 0; r < fillArea.RowCount; r++)
            {
                for (int c = 0; c < fillArea.ColumnCount; c++)
                {
                    int row = fillArea.TopRow + r;
                    int col = fillArea.LeftColumn + c;
                    oldValues[r, c] = ws.GetValue(row, col);
                    oldFormulas[r, c] = ws.GetFormula(row, col);
                }
            }

            var action = new ClipboardPasteAction { SheetView = sheetView };
            action.OldState.Value = oldValues;
            action.OldState.Formulas = oldFormulas;
            action.OldState.Row = fillArea.TopRow;
            action.OldState.Column = fillArea.LeftColumn;
            action.OldState.Selection = sourceRange.Clone();

            Spread.SuspendUpdates = true;
            try
            {
                ws.AutoFill(sourceRange, targetRange, isCtrlPressed);
            }
            finally
            {
                Spread.SuspendUpdates = false;
            }

            var newValues = new object[fillArea.RowCount, fillArea.ColumnCount];
            var newFormulas = new string[fillArea.RowCount, fillArea.ColumnCount];
            for (int r = 0; r < fillArea.RowCount; r++)
            {
                for (int c = 0; c < fillArea.ColumnCount; c++)
                {
                    int row = fillArea.TopRow + r;
                    int col = fillArea.LeftColumn + c;
                    newValues[r, c] = ws.GetValue(row, col);
                    newFormulas[r, c] = ws.GetFormula(row, col);
                }
            }
            action.NewState.Value = newValues;
            action.NewState.Formulas = newFormulas;
            action.NewState.Row = fillArea.TopRow;
            action.NewState.Column = fillArea.LeftColumn;
            action.NewState.Selection = targetRange.Clone();

            Spread.UndoRedoManager?.AddAction(action);
            sheetView.SelectRange(targetRange);
            Spread.Refresh();
        }
    }
}
