using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Represents the view for a worksheet
    /// </summary>
    public interface ISheetView
    {
        /// <summary>
        /// Gets or sets the grid line visibility.
        /// </summary>
        GridLineVisibility GridLineVisibility { get; set; }
        /// <summary>
        /// Gets or sets headers visibility
        /// </summary>
        HeadersVisibility HeadersVisibility { get; set; }
        /// <summary>
        /// Gets or sets selection mode.
        /// </summary>
        SelectionMode SelectionMode { get; set; }
        /// <summary>
        /// Gets or sets the zoom factor (1.0 = 100%). Clamped between 0.1 and 4.0.
        /// </summary>
        double ZoomFactor { get; set; }
        /// <summary>
        /// Fires when the ZoomFactor changes.
        /// </summary>
        event System.EventHandler<ZoomChangedEventArgs> ZoomChanged;
        /// <summary>
        /// Gets the sheet view port.
        /// </summary>
        IViewPort ViewPort { get; }
        /// <summary>
        /// Gets the current scroll position.
        /// </summary>
        Point ScrollPosition { get; }
        /// <summary>
        /// Gets or sets the mouse wheel scroll direction.
        /// </summary>
        MouseWheelScrollDirection MouseWheelScrollDirection { get; set; }
        /// <summary>
        /// Gets the owner spread.
        /// </summary>
        Spread Spread { get; }
        /// <summary>
        /// Gets the active row.
        /// </summary>
        int ActiveRow { get; }
        /// <summary>
        /// Gets the active column.
        /// </summary>
        int ActiveColumn { get; }
        /// <summary>
        /// Gets the current selection.
        /// </summary>
        CellRange Selection { get; }
        /// <summary>
        /// Gets the underlying worksheet for this view.
        /// </summary>
        IWorkSheet WorkSheet { get; }
        /// <summary>
        /// Copies current selection to clipboard.
        /// </summary>
        void CopyToClipboard();
        /// <summary>
        /// Pastes data from clipboard to sheet.
        /// </summary>
        void PasteFromClipboard();
        /// <summary>
        /// Copies the provided cell range to clipboard.
        /// </summary>
        /// <param name="range"></param>
        void CopyToClipboard(CellRange range);
        /// <summary>
        /// Horizontally scrolls the sheet.
        /// </summary>
        /// <param name="offset"></param>
        void ScrollToHorizontalOffset(double offset);
        /// <summary>
        ///  Vertically scrolls the sheet.
        /// </summary>
        /// <param name="offset"></param>
        void ScrollToVerticalOffset(double offset);
        /// <summary>
        /// Gets or sets whether to automatically resize rows on cell change.
        /// </summary>
        bool AutoSizeRows { get; set; }
        /// <summary>
        /// Gets or sets whether to automatically resize columns on cell change.
        /// </summary>
        bool AutoSizeColumns { get; set; }
        /// <summary>
        /// Auto sizes the row height based on cell text content.
        /// </summary>
        /// <param name="row">The row index.</param>
        void AutoSizeRow(int row);
        /// <summary>
        /// Autosize column.
        /// </summary>
        /// <param name="column"></param>
        void AutoSizeColumn(int column);
    }
}
