using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class InsertSheetCommand : SpreadCommand
    {
        public InsertSheetCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var newSheet = Spread.WorkBook.WorkSheets.AddSheet($"Sheet{Spread.WorkBook.WorkSheets.Count + 1}");
            Spread.WorkBook.WorkSheets.ActiveSheet = newSheet;
        }
    }
}