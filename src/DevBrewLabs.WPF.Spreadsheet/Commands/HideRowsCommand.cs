using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class HideRowsCommand : SpreadCommand
    {
        public HideRowsCommand(Spread spread) : base(spread) { }

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
            var action = new RowResizedAction { SheetView = sheetView };

            Spread.SuspendUpdates = true;
            try
            {
                for (int r = sheetView.Selection.TopRow; r <= sheetView.Selection.BottomRow; r++)
                {
                    var row = ws.Rows[r];
                    if (!row.IsHidden)
                    {
                        action.OldHidden[r] = false;
                        action.NewHidden[r] = true;
                        row.IsHidden = true;
                    }
                }
            }
            finally
            {
                Spread.SuspendUpdates = false;
            }

            if (action.NewHidden.Count > 0)
            {
                Spread.UndoRedoManager?.AddAction(action);
            }

            Spread.Refresh();
        }
    }
}