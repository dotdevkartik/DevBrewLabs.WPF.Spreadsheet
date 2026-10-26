using System.Collections.Generic;
using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class ColumnInsertDeleteAction : SheetAction
    {
        public SheetView SheetView { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public bool IsInsert { get; set; }
        public CellRange PreviousSelection { get; set; }
        public object[,] DeletedValues { get; set; }
        public string[,] DeletedFormulas { get; set; }
        public Dictionary<int, int> DeletedColWidths { get; set; }

        public override void Undo()
        {
            if (SheetView?.WorkSheet == null) return;
            var ws = SheetView.WorkSheet;

            if (IsInsert)
            {
                ws.Columns.Remove(Index, Count);
            }
            else
            {
                ws.Columns.Insert(Index, Count);
                if (DeletedValues != null)
                {
                    for (int r = 0; r < DeletedValues.GetLength(0); r++)
                    {
                        for (int c = 0; c < DeletedValues.GetLength(1); c++)
                        {
                            if (DeletedFormulas != null && !string.IsNullOrEmpty(DeletedFormulas[r, c]))
                                ws.SetFormula(r, Index + c, DeletedFormulas[r, c]);
                            else if (DeletedValues[r, c] != null)
                                ws.SetValue(r, Index + c, DeletedValues[r, c]);
                        }
                    }
                }
                if (DeletedColWidths != null)
                {
                    foreach (var kvp in DeletedColWidths)
                        ws.Columns[kvp.Key].Width = kvp.Value;
                }
            }

            if (PreviousSelection.IsValid)
                SheetView.SelectRange(PreviousSelection);

            SheetView.Spread?.Refresh();
        }

        public override void Redo()
        {
            if (SheetView?.WorkSheet == null) return;
            var ws = SheetView.WorkSheet;

            if (IsInsert)
            {
                ws.Columns.Insert(Index, Count);
            }
            else
            {
                ws.Columns.Remove(Index, Count);
            }

            SheetView.Spread?.Refresh();
        }
    }
}
