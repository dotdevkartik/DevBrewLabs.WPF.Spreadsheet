using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Provides data for the <see cref="HyperlinkCellType.Click"/> and <see cref="HyperlinkCellType.RequestNavigate"/> events.
    /// </summary>
    public class CellHyperlinkClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the sheet view where the hyperlink was clicked.
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
        /// Gets the <see cref="HyperlinkCellType"/> associated with the clicked cell.
        /// </summary>
        public HyperlinkCellType CellType { get; }

        /// <summary>
        /// Gets the resolved target URL or address for the hyperlink.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the displayed text for the hyperlink.
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the event has been handled.
        /// When true, default navigation/process launching is suppressed.
        /// </summary>
        public bool Handled { get; set; }

        public CellHyperlinkClickedEventArgs(
            ISheetView sheetView,
            int row,
            int column,
            HyperlinkCellType cellType,
            string url,
            string displayText)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            CellType = cellType;
            Url = url;
            DisplayText = displayText;
        }
    }
}
