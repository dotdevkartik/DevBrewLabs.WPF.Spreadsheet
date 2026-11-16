using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class AutoFitColumnWidthCommand : SpreadCommand
    {
        public AutoFitColumnWidthCommand(Spread spread) : base(spread) { }

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

            for (int c = sheetView.Selection.LeftColumn; c <= sheetView.Selection.RightColumn; c++)
            {
                int oldWidth = ws.Columns.GetColumnWidth(c);
                sheetView.AutoSizeColumn(c);
                int newWidth = ws.Columns.GetColumnWidth(c);
                if (newWidth != oldWidth)
                {
                    action.OldWidths[c] = oldWidth;
                    action.NewWidths[c] = newWidth;
                }
            }

            if (action.NewWidths.Count > 0)
            {
                Spread.UndoRedoManager?.AddAction(action);
            }
        }
    }
}