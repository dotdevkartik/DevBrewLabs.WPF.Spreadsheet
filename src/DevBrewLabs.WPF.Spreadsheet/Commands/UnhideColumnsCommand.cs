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
            int start = Math.Max(0, sheetView.Selection.LeftColumn - 1);
            int end = Math.Min(sheetView.WorkSheet.ColumnCount - 1, sheetView.Selection.RightColumn + 1);
            for (int c = start; c <= end; c++)
            {
                var col = sheetView.WorkSheet.Columns[c];
                if (col.Width == 0) col.Width = sheetView.WorkSheet.DefaultColumnWidth;
            }
            sheetView.ViewPort.CalculateVisibleRange();
            Spread.SheetTabControl?.UpdateScrollbars();
            Spread.Invalidate();
        }
    }
}