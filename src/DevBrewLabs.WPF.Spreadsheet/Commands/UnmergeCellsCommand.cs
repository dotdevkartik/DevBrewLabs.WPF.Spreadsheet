using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class UnmergeCellsCommand : SpreadCommand
    {
        public UnmergeCellsCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView == null || !sheetView.Selection.IsValid) return false;
            return sheetView.WorkSheet.GetSpanCellRange(sheetView.ActiveRow, sheetView.ActiveColumn) != default;
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView != null) sheetView.UnmergeRange(sheetView.Selection);
        }
    }
}