using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using System;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Provides data for the <see cref="Spread.CellEditStarting"/> event.
    /// </summary>
    public class CellEditStartingEventArgs : EventArgs
    {
        public ISheetView SheetView { get; }
        public int Row { get; }
        public int Column { get; }
        public EditTrigger Trigger { get; }
        public bool Cancel { get; set; }

        public CellEditStartingEventArgs(ISheetView sheetView, int row, int column, EditTrigger trigger)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            Trigger = trigger;
            Cancel = false;
        }
    }
}
