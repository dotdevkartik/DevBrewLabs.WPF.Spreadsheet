using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class AutoFitRowHeightCommand : SpreadCommand
    {
        public AutoFitRowHeightCommand(Spread spread) : base(spread) { }

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
                    int oldHeight = ws.Rows.GetRowHeight(r);
                    sheetView.AutoSizeRow(r);
                    int newHeight = ws.Rows.GetRowHeight(r);
                    if (newHeight != oldHeight)
                    {
                        action.OldHeights[r] = oldHeight;
                        action.NewHeights[r] = newHeight;
                    }
                }
            }
            finally
            {
                Spread.SuspendUpdates = false;
            }

            if (action.NewHeights.Count > 0)
            {
                Spread.UndoRedoManager?.AddAction(action);
            }

            Spread.Refresh();
        }
    }
}