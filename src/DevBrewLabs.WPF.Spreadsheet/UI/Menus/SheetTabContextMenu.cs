using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Commands;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Menus
{
    internal class SheetTabContextMenu : SpreadContextMenu
    {
        public SheetTabContextMenu(Spread spread, int sheetIndex) : base(spread)
        {
            Items.Add(CreateMenuItem("Insert Sheet", null, "AddSheetGeometry", AddSheetBrush, new InsertSheetCommand(Spread), null));
            Items.Add(CreateMenuItem("Delete Sheet", null, "DeleteSheetGeometry", DeleteSheetBrush, new DeleteSheetCommand(Spread), sheetIndex));
            Items.Add(CreateMenuItem("Duplicate Sheet", null, "DuplicateSheetGeometry", DuplicateSheetBrush, new DuplicateSheetCommand(Spread), sheetIndex));
        }
    }
}
