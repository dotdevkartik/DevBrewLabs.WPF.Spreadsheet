using DevBrewLabs.Spreadsheet;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI
{
    internal interface IViewPort
    {
        /// <summary>
        /// Gets the visible cell range.
        /// </summary>
        CellRange ViewRange { get; }
        /// <summary>
        /// Gets the viewport bounds.
        /// </summary>
        Rect ActualBounds { get; }
        /// <summary>
        /// Gets the column bounds.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        Rect GetColumnRect(int col);
        /// <summary>
        /// Gets the row bounds.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        Rect GetRowRect(int row);
        /// <summary>
        /// Gets the cell bounds
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        Rect GetCellRect(int row, int col);
        /// <summary>
        /// Gets the range bounds.
        /// </summary>
        /// <param name="cellRange"></param>
        /// <returns></returns>
        Rect GetRangeRect(CellRange cellRange);
        /// <summary>
        /// Gets the Rect for visible cell range.
        /// </summary>
        /// <returns></returns>
        Rect GetViewRangeRect();
        /// <summary>
        /// Gets the range bounds.
        /// </summary>
        /// <param name="topRow"></param>
        /// <param name="leftColumn"></param>
        /// <param name="bottomRow"></param>
        /// <param name="rightColumn"></param>
        Rect GetRangeRect(int topRow, int leftColumn, int bottomRow, int rightColumn);
        /// <summary>
        /// Gets whether the row is visible.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        bool IsRowVisible(int row);
        /// <summary>
        /// Gets whether the column is visible.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        bool IsColumnVisible(int column);
    }
}
