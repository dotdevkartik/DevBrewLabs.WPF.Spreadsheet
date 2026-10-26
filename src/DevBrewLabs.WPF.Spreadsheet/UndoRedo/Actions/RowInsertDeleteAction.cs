using System.Collections.Generic;
using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class RowInsertDeleteAction : SheetAction
    {
        public SheetView SheetView { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public bool IsInsert { get; set; }
        public CellRange PreviousSelection { get; set; }
        public object[,] DeletedValues { get; set; }
        public string[,] DeletedFormulas { get; set; }
        public Dictionary<int, int> DeletedRowHeights { get; set; }

        public override void Undo()
        {
            if (SheetView?.WorkSheet == null) return;
            var ws = SheetView.WorkSheet;

            if (IsInsert)
            {
                ws.Rows.Remove(Index, Count);
            }
            else
            {
                ws.Rows.Insert(Index, Count);
                if (DeletedValues != null)
                {
                    for (int r = 0; r < DeletedValues.GetLength(0); r++)
                    {
                        for (int c = 0; c < DeletedValues.GetLength(1); c++)
                        {
                            if (DeletedFormulas != null && !string.IsNullOrEmpty(DeletedFormulas[r, c]))
                                ws.SetFormula(Index + r, c, DeletedFormulas[r, c]);
                            else if (DeletedValues[r, c] != null)
                                ws.SetValue(Index + r, c, DeletedValues[r, c]);
                        }
                    }
                }
                if (DeletedRowHeights != null)
                {
                    foreach (var kvp in DeletedRowHeights)
                        ws.Rows[kvp.Key].Height = kvp.Value;
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
                ws.Rows.Insert(Index, Count);
            }
            else
            {
                ws.Rows.Remove(Index, Count);
            }

            SheetView.Spread?.Refresh();
        }
    }
}
