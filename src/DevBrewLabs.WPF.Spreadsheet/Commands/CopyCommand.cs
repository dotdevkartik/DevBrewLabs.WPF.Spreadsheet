using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class CopyCommand : SpreadCommand
    {
        public CopyCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            var sheetView = parameter as SheetView;
            return sheetView != null && Spread.ClipboardManager.CanCopy(sheetView);
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView != null) Spread.ClipboardManager.Copy(sheetView);
        }
    }
}