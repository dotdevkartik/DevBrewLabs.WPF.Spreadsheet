using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class SortCommand : SpreadCommand
    {
        public SortCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is Tuple<SheetView, bool> args)) return false;
            return args.Item1 != null && args.Item1.Selection.IsValid;
        }

        public override void Execute(object parameter)
        {
            if (!(parameter is Tuple<SheetView, bool> args)) return;
            var sheetView = args.Item1;
            var ascending = args.Item2;
            if (sheetView.Selection.RowCount > 1)
            {
                sheetView.WorkSheet.SortRange(sheetView.Selection, new SortOptions { HasHeader = false, SortLevels = new List<SortInfo> { new SortInfo(sheetView.ActiveColumn, ascending) } });
            }
            else
            {
                sheetView.WorkSheet.Sort(new SortOptions { HasHeader = false, SortLevels = new List<SortInfo> { new SortInfo(sheetView.ActiveColumn, ascending) } });
            }
            Spread.Refresh();
        }
    }
}