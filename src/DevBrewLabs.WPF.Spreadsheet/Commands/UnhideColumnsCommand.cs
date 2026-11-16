using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class UnhideColumnsCommand : SpreadCommand
    {
        public UnhideColumnsCommand(Spread spread) : base(spread) { }

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
            int start = Math.Max(0, sheetView.Selection.LeftColumn - 1);
            int end = Math.Min(ws.ColumnCount - 1, sheetView.Selection.RightColumn + 1);
            var action = new ColumnResizedAction { SheetView = sheetView };

            Spread.SuspendUpdates = true;
            try
            {
                for (int c = start; c <= end; c++)
                {
                    var col = ws.Columns.GetItem(c);
                    if (col != null && col.Width == 0)
                    {
                        action.OldWidths[c] = 0;
                        action.NewWidths[c] = ws.DefaultColumnWidth;
                        col.Width = ws.DefaultColumnWidth;
                    }
                }
            }
            finally
            {
                Spread.SuspendUpdates = false;
            }

            if (action.NewWidths.Count > 0)
            {
                Spread.UndoRedoManager?.AddAction(action);
            }

            Spread.Refresh();
        }
    }
}