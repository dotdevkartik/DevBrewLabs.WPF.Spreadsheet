using DevBrewLabs.Spreadsheet;
using System;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    internal class ApplyFilterCommand : SpreadCommand
    {
        public ApplyFilterCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            return Spread.AllowFiltering && parameter is SheetView sheetView && sheetView.Selection.IsValid;
        }

        public override void Execute(object parameter)
        {
            var sheetView = parameter as SheetView;
            if (sheetView == null || !sheetView.Selection.IsValid) return;

            var ws = sheetView.WorkSheet;
            if (ws.AutoFilter != null && ws.AutoFilter.IsEnabled)
            {
                ws.AutoFilter.SetRange(default);
            }
            else
            {
                var rowCount = sheetView.Selection.BottomRow - sheetView.Selection.TopRow + 1;
                var colCount = sheetView.Selection.RightColumn - sheetView.Selection.LeftColumn + 1;
                
                // If single cell, maybe they meant to filter a larger block, but for now we trust the selection block.
                var range = new CellRange(sheetView.Selection.TopRow, sheetView.Selection.LeftColumn, rowCount, colCount);
                ws.AutoFilter?.SetRange(range);
            }
            
            Spread.Refresh();
        }
    }
}
