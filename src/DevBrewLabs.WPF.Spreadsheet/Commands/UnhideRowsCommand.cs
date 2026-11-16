using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class UnhideRowsCommand : SpreadCommand
    {
        public UnhideRowsCommand(Spread spread) : base(spread) { }

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
            int start = Math.Max(0, sheetView.Selection.TopRow - 1);
            int end = Math.Min(ws.RowCount - 1, sheetView.Selection.BottomRow + 1);
            var action = new RowResizedAction { SheetView = sheetView };

            Spread.SuspendUpdates = true;
            try
            {
                for (int r = start; r <= end; r++)
                {
                    var row = ws.Rows.GetItem(r);
                    if (row == null || (!row.IsHidden && row.Height > 0))
                        continue;

                    if (row.IsHidden)
                    {
                        action.OldHidden[r] = true;
                        action.NewHidden[r] = false;
                        row.IsHidden = false;
                    }

                    if (row.Height == 0)
                    {
                        action.OldHeights[r] = 0;
                        action.NewHeights[r] = ws.DefaultRowHeight;
                        row.Height = ws.DefaultRowHeight;
                    }
                }
            }
            finally
            {
                Spread.SuspendUpdates = false;
            }

            if (action.NewHidden.Count > 0 || action.NewHeights.Count > 0)
            {
                Spread.UndoRedoManager?.AddAction(action);
            }

            Spread.Refresh();
        }
    }
}