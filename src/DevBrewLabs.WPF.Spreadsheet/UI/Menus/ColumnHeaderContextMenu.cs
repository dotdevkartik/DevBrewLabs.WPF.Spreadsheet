using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Commands;
using System;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Menus
{
    internal class ColumnHeaderContextMenu : SpreadContextMenu
    {
        public ColumnHeaderContextMenu(Spread spread, SheetView sheetView) : base(spread)
        {
            Items.Add(CreateMenuItem("Cut", "Ctrl+X", "CutIconGeometry", CutBrush, new CutCommand(Spread), sheetView));
            Items.Add(CreateMenuItem("Copy", "Ctrl+C", "CopyIconGeometry", CopyBrush, new CopyCommand(Spread), sheetView));
            Items.Add(CreateMenuItem("Paste", "Ctrl+V", "PasteIconGeometry", PasteBrush, new PasteCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("Clear Contents", "Del", "ClearIconGeometry", ClearBrush, new ClearContentsCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("Hide Columns", null, "HideGeometry", VisibilityBrush, new HideColumnsCommand(Spread), sheetView));
            Items.Add(CreateMenuItem("Unhide Columns", null, "UnhideGeometry", VisibilityBrush, new UnhideColumnsCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("AutoFit Column Width", null, "AutoFitGeometry", AutoFitBrush, new AutoFitColumnWidthCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("Sort A to Z", null, "SortAscGeometry", SortBrush, new SortCommand(Spread), new Tuple<SheetView, bool>(sheetView, true)));
            Items.Add(CreateMenuItem("Sort Z to A", null, "SortDescGeometry", SortBrush, new SortCommand(Spread), new Tuple<SheetView, bool>(sheetView, false)));        }
    }
}
