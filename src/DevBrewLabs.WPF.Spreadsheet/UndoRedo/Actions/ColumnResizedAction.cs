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
            ApplyWidths(OldWidths);
        }

        public override void Redo()
        {
            ApplyWidths(NewWidths);
        }

        private void ApplyWidths(Dictionary<int, int> widths)
        {
            if (SheetView?.WorkSheet == null) return;

            var spread = SheetView.Spread;
            if (spread != null) spread.SuspendUpdates = true;
            try
            {
                foreach (var kvp in widths)
                {
                    SheetView.WorkSheet.Columns[kvp.Key].Width = kvp.Value;
                }
            }
            finally
            {
                if (spread != null) spread.SuspendUpdates = false;
            }

            spread?.Refresh();
        }
    }
}
