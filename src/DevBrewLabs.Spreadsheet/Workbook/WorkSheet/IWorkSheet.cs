using DevBrewLabs.Spreadsheet.Sorting;
using System;

namespace DevBrewLabs.Spreadsheet
{
    public interface IWorkSheet : ICellContainer, IDisposable
    {
        /// <summary>
        /// Fires when cell is modified.
        /// </summary>
        event EventHandler<CellChangedEventArgs> CellChanged;
        /// <summary>
        /// Fires when a range is changed.
        /// </summary>
        event EventHandler<RangeChangedEventArgs> RangeChanged;
        /// <summary>
        /// Fires when row/rows changes.
        /// </summary>
        event EventHandler<RowChangedEventArgs> RowChanged;
        /// <summary>
        /// Fires when column/columns changes.
        /// </summary>
        event EventHandler<ColumnChangedEventArgs> ColumnChanged;

        /// <summary>
        /// Gets the parent workbook.
        /// </summary>
        IWorkBook WorkBook { get; }
        
        /// <summary>
        /// Gets or sets name for this sheet.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the row count for this sheet.
        /// </summary>
        int RowCount { get; set; }
        /// <summary>
        /// Gets or sets the column count for this sheet.
        /// </summary>
        int ColumnCount { get; set; }
        /// <summary>
        /// Gets or sets the default row height for this sheet.
        /// </summary>
        int DefaultRowHeight { get; set; }
        /// <summary>
        /// Gets or sets the default column width for this sheet.
        /// </summary>
        int DefaultColumnWidth { get; set; }

        /// <summary>
        /// Gets row collection of this sheet.
        /// </summary>
        IRows Rows { get; }
        /// <summary>
        /// Gets column collection of this sheet.
        /// </summary>
        IColumns Columns { get; }
        /// <summary>
        /// Gets the cells of this sheet.
        /// </summary>
        IRange Cells { get; }
        /// <summary>
        /// Gets the sheet top left region.
        /// </summary>
        ITopLeft TopLeft { get; }
        /// <summary>
        /// Gets the sheet row headers.
        /// </summary>
        IRowHeaders RowHeaders { get; }
        /// <summary>
        /// Gets the sheet column headers.
        /// </summary>
        IColumnHeaders ColumnHeaders { get; }
        /// <summary>
        /// Gets or sets the sheet data source.
        /// </summary>
        object DataSource { get; set; }
        /// <summary>
        /// Gets the range data.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        object[,] GetData(CellRange range);
        /// <summary>
        /// Gets the range data.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        object[,] GetData(int row, int column, int rowCount, int columnCount);
        /// <summary>
        /// Loads the provided 2D object array into the worksheet starting from the specified row and column.
        /// </summary>
        /// <param name="data">The 2D object array to load.</param>
        /// <param name="startRow">The starting row index (default 0).</param>
        /// <param name="startCol">The starting column index (default 0).</param>
        void Load(object[,] data, int startRow = 0, int startCol = 0);
        /// <summary>
        /// Sorts the provided cell range.
        /// </summary>
        /// <param name="range">The cell range to sort.</param>
        /// <param name="options">Options for the sort operation.</param>
        void SortRange(CellRange range, SortOptions options);
        /// <summary>
        /// Clears worksheet.
        /// </summary>
        void Clear(WorkSheetClearMode mode);
        /// <summary>
        /// Checks if the worksheet contains this range
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        bool ContainsRange(int row, int column, int rowCount, int columnCount);
        /// <summary>
        /// Sorts the complete worksheet.
        /// </summary>
        /// <param name="options">Options for the sort operation.</param>
        void Sort(SortOptions options);
    }
}