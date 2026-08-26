using DevBrewLabs.Spreadsheet;
using System;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Provides data for the <see cref="Spread.CellEditEnded"/> event.
    /// </summary>
    public class CellEditEndedEventArgs : EventArgs
    {
        public ISheetView SheetView { get; }
        public int Row { get; }
        public int Column { get; }
        public bool WasCommitted { get; }

        public CellEditEndedEventArgs(ISheetView sheetView, int row, int column, bool wasCommitted)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            WasCommitted = wasCommitted;
        }
    }
}
