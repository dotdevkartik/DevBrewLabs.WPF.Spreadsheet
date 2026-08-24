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
            int start = Math.Max(0, sheetView.Selection.TopRow - 1);
            int end = Math.Min(sheetView.WorkSheet.RowCount - 1, sheetView.Selection.BottomRow + 1);
            for (int r = start; r <= end; r++)
            {
                var row = sheetView.WorkSheet.Rows[r];
                row.IsHidden = false;
                if (row.Height == 0) row.Height = sheetView.WorkSheet.DefaultRowHeight;
            }
            Spread.Refresh();
        }
    }
}