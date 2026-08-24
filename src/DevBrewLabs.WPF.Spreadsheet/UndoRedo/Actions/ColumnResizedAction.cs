using System.Collections.Generic;
using DevBrewLabs.WPF.Spreadsheet.UI;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class ColumnResizedAction : SheetAction
    {
        public Dictionary<int, int> OldWidths { get; private set; }
        public Dictionary<int, int> NewWidths { get; private set; }
        public SheetView SheetView { get; set; }

        public ColumnResizedAction()
        {
            OldWidths = new Dictionary<int, int>();
            NewWidths = new Dictionary<int, int>();
        }

        public override void Undo()
        {
            foreach (var kvp in OldWidths)
            {
                SheetView.WorkSheet.Columns[kvp.Key].Width = kvp.Value;
            }

            SheetView.Spread.Refresh();
        }

        public override void Redo()
        {
            foreach (var kvp in NewWidths)
            {
                SheetView.WorkSheet.Columns[kvp.Key].Width = kvp.Value;
            }

            SheetView.Spread.Refresh();
        }
    }
}
