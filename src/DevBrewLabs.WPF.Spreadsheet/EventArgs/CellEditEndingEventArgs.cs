using DevBrewLabs.Spreadsheet;
using System;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Provides data for the <see cref="Spread.CellEditEnding"/> event.
    /// </summary>
    public class CellEditEndingEventArgs : EventArgs
    {
        public ISheetView SheetView { get; }
        public int Row { get; }
        public int Column { get; }
        public object NewValue { get; }
        public bool Cancel { get; set; }

        public CellEditEndingEventArgs(ISheetView sheetView, int row, int column, object newValue)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            NewValue = newValue;
            Cancel = false;
        }
    }
}
