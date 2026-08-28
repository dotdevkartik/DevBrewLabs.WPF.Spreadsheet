using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Provides data for the <see cref="ButtonCellType.Click"/> and <see cref="Spread.CellButtonClicked"/> events.
    /// </summary>
    public class CellButtonClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the sheet view where the button click occurred.
        /// </summary>
        public ISheetView SheetView { get; }

        /// <summary>
        /// Gets the zero-based row index of the clicked cell.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Gets the zero-based column index of the clicked cell.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the <see cref="ButtonCellType"/> associated with the clicked cell.
        /// </summary>
        public ButtonCellType CellType { get; }

        public CellButtonClickedEventArgs(ISheetView sheetView, int row, int column, ButtonCellType cellType)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            CellType = cellType;
        }
    }
}
