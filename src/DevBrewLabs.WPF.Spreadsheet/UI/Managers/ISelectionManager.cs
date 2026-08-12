using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    public interface ISelectionManager
    {
        /// <summary>
        /// Selects the cell.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        void SelectCell(int row, int col);
        /// <summary>
        /// Selects the row.
        /// </summary>
        /// <param name="row"></param>
        void SelectRow(int row);
        /// <summary>
        /// Selects row range.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="count"></param>
        void SelectRows(int row, int count);
        /// <summary>
        /// Selects column.
        /// </summary>
        /// <param name="column"></param>
        void SelectColumn(int column);
        /// <summary>
        /// Selects column range.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="count"></param>
        void SelectColumns(int column, int count);
        /// <summary>
        /// Selects cell range.
        /// </summary>
        /// <param name="range"></param>
        void SelectRange(CellRange range);
        /// <summary>
        /// Selects cell range.
        /// </summary>
        /// <param name="range"></param>
        void SelectRange(int row, int column, int rowCount, int columnCount);
        
        /// <summary>
        /// Merges the currently selected cells.
        /// </summary>
        void MergeSelection();

        /// <summary>
        /// Unmerges the currently selected cells.
        /// </summary>
        void UnmergeSelection();
    }
}
