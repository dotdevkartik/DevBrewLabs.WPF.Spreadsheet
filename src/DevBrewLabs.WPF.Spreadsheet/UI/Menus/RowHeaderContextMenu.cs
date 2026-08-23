using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Commands;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Menus
{
    internal class RowHeaderContextMenu : SpreadContextMenu
    {
        public RowHeaderContextMenu(Spread spread, SheetView sheetView) : base(spread)
        {
            Items.Add(CreateMenuItem("Cut", "Ctrl+X", "CutIconGeometry", CutBrush, new CutCommand(Spread), sheetView));
            Items.Add(CreateMenuItem("Copy", "Ctrl+C", "CopyIconGeometry", CopyBrush, new CopyCommand(Spread), sheetView));
            Items.Add(CreateMenuItem("Paste", "Ctrl+V", "PasteIconGeometry", PasteBrush, new PasteCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("Clear Contents", "Del", "ClearIconGeometry", ClearBrush, new ClearContentsCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("Hide Rows", null, "HideGeometry", VisibilityBrush, new HideRowsCommand(Spread), sheetView));
            Items.Add(CreateMenuItem("Unhide Rows", null, "UnhideGeometry", VisibilityBrush, new UnhideRowsCommand(Spread), sheetView));

            Items.Add(CreateSeparator());

            Items.Add(CreateMenuItem("AutoFit Row Height", null, "AutoFitGeometry", AutoFitBrush, new AutoFitRowHeightCommand(Spread), sheetView));
        }
    }
}
