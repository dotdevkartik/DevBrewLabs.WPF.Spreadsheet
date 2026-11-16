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
        public Dictionary<(int Row, int Col), CellSnapshot> DeletedCells { get; set; }
        public Dictionary<int, ColumnDimensionSnapshot> DeletedColumnSnapshots { get; set; }
        public List<CellRange> PreOperationSpans { get; set; }
        public List<(string SheetName, int Row, int Col, string Formula)> PreOperationWorkbookFormulas { get; set; }

        public override void Undo()
        {
            if (SheetView?.WorkSheet == null) return;
            var ws = SheetView.WorkSheet;
            var spread = SheetView.Spread;

            if (spread != null) spread.SuspendUpdates = true;
            try
            {
                if (IsInsert)
                {
                    ws.Columns.Remove(Index, Count);
                    GridSnapshotHelper.RestoreSpans(ws, PreOperationSpans);
                    GridSnapshotHelper.RestoreWorkbookFormulas(ws.WorkBook, PreOperationWorkbookFormulas);
                }
                else
                {
                    ws.Columns.Insert(Index, Count);
                    GridSnapshotHelper.RestoreSpans(ws, PreOperationSpans);

                    if (DeletedColumnSnapshots != null)
                    {
                        foreach (var kvp in DeletedColumnSnapshots)
                        {
                            var col = ws.Columns[kvp.Key];
                            var snap = kvp.Value;
                            if (snap.Width != ws.DefaultColumnWidth)
                                col.Width = snap.Width;
                            if (snap.Style != null)
                                col.Style = snap.Style;
                            if (!string.IsNullOrEmpty(snap.StyleName))
                                col.StyleName = snap.StyleName;
                            if (snap.Formatter != null)
                                col.Formatter = snap.Formatter;
                            if (snap.CellType != null)
                                col.CellType = snap.CellType;
                            if (snap.DataMap != null)
                                col.DataMap = snap.DataMap;
                            if (snap.Locked)
                                col.Locked = true;
                            if (!snap.AllowFiltering)
                                col.AllowFiltering = false;
                        }
                    }

                    if (DeletedCells != null)
                    {
                        foreach (var kvp in DeletedCells)
                        {
                            GridSnapshotHelper.RestoreCellSnapshot(ws, kvp.Key.Row, kvp.Key.Col, kvp.Value);
                        }
                    }

                    GridSnapshotHelper.RestoreWorkbookFormulas(ws.WorkBook, PreOperationWorkbookFormulas);
                }

                if (PreviousSelection.IsValid)
                    SheetView.SelectRange(PreviousSelection);
            }
            finally
            {
                if (spread != null) spread.SuspendUpdates = false;
            }

            spread?.Refresh();
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
