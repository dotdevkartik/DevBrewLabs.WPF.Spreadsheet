using System.Collections.Generic;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal struct CellSnapshot
    {
        public object Value;
        public string Formula;
        public IStyle Style;
        public string StyleName;
        public IFormatter Formatter;
        public ICellType CellType;
        public IDataMap DataMap;
        public bool Locked;
    }

    internal struct RowDimensionSnapshot
    {
        public int Height;
        public bool IsHidden;
        public IStyle Style;
        public string StyleName;
        public IFormatter Formatter;
        public bool Locked;
    }

    internal struct ColumnDimensionSnapshot
    {
        public int Width;
        public IStyle Style;
        public string StyleName;
        public IFormatter Formatter;
        public ICellType CellType;
        public DataMap DataMap;
        public bool Locked;
        public bool AllowFiltering;
    }

    internal static class GridSnapshotHelper
    {
        public static CellSnapshot CaptureCellSnapshot(IWorksheet ws, int row, int col)
        {
            return new CellSnapshot
            {
                Formula = ws.GetFormula(row, col),
                Value = string.IsNullOrEmpty(ws.GetFormula(row, col)) ? ws.GetValue(row, col) : null,
                Style = ws.GetStyle(row, col),
                StyleName = ws.GetStyleName(row, col),
                Formatter = ws.GetFormatter(row, col),
                CellType = ws.GetCellType(row, col),
                DataMap = ws.GetDataMap(row, col),
                Locked = ws.GetLocked(row, col)
            };
        }

        public static void RestoreCellSnapshot(IWorksheet ws, int row, int col, CellSnapshot snap)
        {
            if (!string.IsNullOrEmpty(snap.Formula))
                ws.SetFormula(row, col, snap.Formula);
            else if (snap.Value != null)
                ws.SetValue(row, col, snap.Value);

            if (snap.Style != null)
                ws.SetStyle(row, col, snap.Style);
            if (!string.IsNullOrEmpty(snap.StyleName))
                ws.SetStyleName(row, col, snap.StyleName);
            if (snap.Formatter != null)
                ws.SetFormatter(row, col, snap.Formatter);
            if (snap.CellType != null)
                ws.SetCellType(row, col, snap.CellType);
            if (snap.DataMap != null)
                ws.SetDataMap(row, col, snap.DataMap);
            if (snap.Locked)
                ws.SetLocked(row, col, true);
        }

        public static List<(string SheetName, int Row, int Col, string Formula)> CaptureWorkbookFormulas(IWorkbook workbook)
        {
            var result = new List<(string, int, int, string)>();
            if (workbook?.WorkSheets == null) return result;

            foreach (var sheet in workbook.WorkSheets)
            {
                foreach (var cell in sheet.GetAllFormulaCells())
                {
                    result.Add((sheet.Name, cell.Row, cell.Column, cell.Formula));
                }
            }

            return result;
        }

        public static void RestoreSpans(IWorksheet ws, List<CellRange> spans)
        {
            if (ws == null || spans == null) return;

            var currentSpans = new List<CellRange>(ws.GetAllSpans());
            foreach (var s in currentSpans)
            {
                ws.RemoveSpan(s.TopRow, s.LeftColumn);
            }

            foreach (var s in spans)
            {
                ws.AddSpan(s.TopRow, s.LeftColumn, s.RowCount, s.ColumnCount);
            }
        }

        public static void RestoreWorkbookFormulas(IWorkbook workbook, List<(string SheetName, int Row, int Col, string Formula)> formulas)
        {
            if (workbook?.WorkSheets == null || formulas == null) return;

            foreach (var entry in formulas)
            {
                var targetSheet = workbook.WorkSheets.GetSheet(entry.SheetName);
                if (targetSheet != null && targetSheet.GetFormula(entry.Row, entry.Col) != entry.Formula)
                {
                    targetSheet.SetFormula(entry.Row, entry.Col, entry.Formula);
                }
            }
        }
    }

    internal class RowInsertDeleteAction : SheetAction
    {
        public SheetView SheetView { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public bool IsInsert { get; set; }
        public CellRange PreviousSelection { get; set; }
        public Dictionary<(int Row, int Col), CellSnapshot> DeletedCells { get; set; }
        public Dictionary<int, RowDimensionSnapshot> DeletedRowSnapshots { get; set; }
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
                    ws.Rows.Remove(Index, Count);
                    GridSnapshotHelper.RestoreSpans(ws, PreOperationSpans);
                    GridSnapshotHelper.RestoreWorkbookFormulas(ws.WorkBook, PreOperationWorkbookFormulas);
                }
                else
                {
                    ws.Rows.Insert(Index, Count);
                    GridSnapshotHelper.RestoreSpans(ws, PreOperationSpans);

                    if (DeletedRowSnapshots != null)
                    {
                        foreach (var kvp in DeletedRowSnapshots)
                        {
                            var row = ws.Rows[kvp.Key];
                            var snap = kvp.Value;
                            if (snap.Height != ws.DefaultRowHeight)
                                row.Height = snap.Height;
                            if (snap.IsHidden)
                                row.IsHidden = true;
                            if (snap.Style != null)
                                row.Style = snap.Style;
                            if (!string.IsNullOrEmpty(snap.StyleName))
                                row.StyleName = snap.StyleName;
                            if (snap.Formatter != null)
                                row.Formatter = snap.Formatter;
                            if (snap.Locked)
                                row.Locked = true;
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
