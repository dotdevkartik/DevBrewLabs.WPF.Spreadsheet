using System.Collections.Generic;
using DevBrewLabs.WPF.Spreadsheet.UI;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class RowResizedAction : SheetAction
    {
        public Dictionary<int, int> OldHeights { get; private set; }
        public Dictionary<int, int> NewHeights { get; private set; }
        public SheetView SheetView { get; set; }

        public RowResizedAction()
        {
            OldHeights = new Dictionary<int, int>();
            NewHeights = new Dictionary<int, int>();
        }

        public override void Undo()
        {
            foreach (var kvp in OldHeights)
            {
                SheetView.WorkSheet.Rows[kvp.Key].Height = kvp.Value;
            }

            SheetView.Spread.Refresh();
        }

        public override void Redo()
        {
            foreach (var kvp in NewHeights)
            {
                SheetView.WorkSheet.Rows[kvp.Key].Height = kvp.Value;
            }

            SheetView.Spread.Refresh();
        }
    }
}
