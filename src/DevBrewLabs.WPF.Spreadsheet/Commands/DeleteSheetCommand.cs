using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class DeleteSheetCommand : SpreadCommand
    {
        public DeleteSheetCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            return Spread.WorkBook.WorkSheets.Count > 1;
        }

        public override void Execute(object parameter)
        {
            if (!(parameter is int sheetIndex)) return;
            if (sheetIndex < 0 || sheetIndex >= Spread.WorkBook.WorkSheets.Count)
                sheetIndex = Spread.WorkBook.WorkSheets.ActiveSheetIndex;
            if (sheetIndex >= 0 && sheetIndex < Spread.WorkBook.WorkSheets.Count)
            {
                Spread.WorkBook.WorkSheets.RemoveSheet(sheetIndex);
                int newIndex = Math.Min(sheetIndex, Spread.WorkBook.WorkSheets.Count - 1);
                Spread.WorkBook.WorkSheets.ActiveSheet = Spread.WorkBook.WorkSheets[newIndex];
            }
        }
    }
}