using System.Collections.Generic;
using DevBrewLabs.WPF.Spreadsheet.UI;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class RowResizedAction : SheetAction
    {
        public Dictionary<int, int> OldHeights { get; private set; }
        public Dictionary<int, int> NewHeights { get; private set; }
        public Dictionary<int, bool> OldHidden { get; private set; }
        public Dictionary<int, bool> NewHidden { get; private set; }
        public SheetView SheetView { get; set; }

        public RowResizedAction()
        {
            OldHeights = new Dictionary<int, int>();
            NewHeights = new Dictionary<int, int>();
            OldHidden = new Dictionary<int, bool>();
            NewHidden = new Dictionary<int, bool>();
        }

        public override void Undo()
        {
            ApplyState(OldHeights, OldHidden);
        }

        public override void Redo()
        {
            ApplyState(NewHeights, NewHidden);
        }

        private void ApplyState(Dictionary<int, int> heights, Dictionary<int, bool> hidden)
        {
            if (SheetView?.WorkSheet == null) return;

            var spread = SheetView.Spread;
            if (spread != null) spread.SuspendUpdates = true;
            try
            {
                foreach (var kvp in heights)
                {
                    SheetView.WorkSheet.Rows[kvp.Key].Height = kvp.Value;
                }

                foreach (var kvp in hidden)
                {
                    SheetView.WorkSheet.Rows[kvp.Key].IsHidden = kvp.Value;
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
