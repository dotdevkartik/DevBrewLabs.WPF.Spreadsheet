using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class MergeCellsCommand : SpreadCommand
    {
        public MergeCellsCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            return sheetView != null && sheetView.Selection.IsValid && (sheetView.Selection.RowCount > 1 || sheetView.Selection.ColumnCount > 1);
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView != null) sheetView.MergeRange(sheetView.Selection);
        }
    }
}