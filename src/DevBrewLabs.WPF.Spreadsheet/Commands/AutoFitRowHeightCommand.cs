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
            for (int r = sheetView.Selection.TopRow; r <= sheetView.Selection.BottomRow; r++)
            {
                sheetView.AutoSizeRow(r);
            }
            sheetView.ViewPort.CalculateVisibleRange();
            Spread.SheetTabControl?.UpdateScrollbars();
            Spread.Invalidate();
        }
    }
}