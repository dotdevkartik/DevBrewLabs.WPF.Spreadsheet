using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class HideColumnsCommand : SpreadCommand
    {
        public HideColumnsCommand(Spread spread) : base(spread) { }

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
            var action = new ColumnResizedAction { SheetView = sheetView };

            Spread.SuspendUpdates = true;
            try
            {
                for (int c = sheetView.Selection.LeftColumn; c <= sheetView.Selection.RightColumn; c++)
                {
                    int oldWidth = ws.Columns.GetColumnWidth(c);
                    if (oldWidth != 0)
                    {
                        action.OldWidths[c] = oldWidth;
                        action.NewWidths[c] = 0;
                        ws.Columns[c].Width = 0;
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