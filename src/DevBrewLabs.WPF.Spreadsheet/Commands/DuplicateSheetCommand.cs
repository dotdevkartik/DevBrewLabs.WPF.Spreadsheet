using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.Commands
{
    public class DuplicateSheetCommand : SpreadCommand
    {
        public DuplicateSheetCommand(Spread spread) : base(spread) { }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is int sheetIndex)) return false;
            return sheetIndex >= 0 && sheetIndex < Spread.WorkBook.WorkSheets.Count;
        }

        public override void Execute(object parameter)
        {
            if (!(parameter is int sheetIndex)) return;
            var sourceSheet = (Worksheet)Spread.WorkBook.WorkSheets[sheetIndex];
            string baseName = $"{sourceSheet.Name} (Copy)";
            string newName = baseName;
            int counter = 2;
            while (true)
            {
                try
                {
                    ((Worksheets)Spread.WorkBook.WorkSheets).VerifySheetName(newName);
                    break;
                }
                catch
                {
                    newName = $"{baseName} {counter++}";
                }
            }
            var newSheet = (Worksheet)Spread.WorkBook.WorkSheets.AddSheet(newName);
            newSheet.RowCount = sourceSheet.RowCount;
            newSheet.ColumnCount = sourceSheet.ColumnCount;
            newSheet.DefaultRowHeight = sourceSheet.DefaultRowHeight;
            newSheet.DefaultColumnWidth = sourceSheet.DefaultColumnWidth;
            var data = sourceSheet.GetData(0, 0, sourceSheet.RowCount, sourceSheet.ColumnCount);
            newSheet.Load(data, 0, 0);
            for (int r = 0; r < sourceSheet.RowCount; r++)
            {
                for (int c = 0; c < sourceSheet.ColumnCount; c++)
                {
                    var formula = sourceSheet.GetFormula(r, c);
                    if (!string.IsNullOrEmpty(formula)) newSheet.SetFormula(r, c, formula);
                }
            }
            Spread.WorkBook.WorkSheets.ActiveSheet = newSheet;
        }
    }
}