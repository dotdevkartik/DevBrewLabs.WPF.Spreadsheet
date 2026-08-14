namespace DevBrewLabs.Spreadsheet
{
    // Represents a cell or range of cells in the spreadsheet.
    public interface IRange : ICell, ICellContainer, IStyledObject
    {
        /// <summary>
        /// Gets the range of cells by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IRange this[string name] { get; }
        /// <summary>
        /// Gets the cell at the specified row and column.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        ICell this[int row, int column] { get; }
        /// <summary>
        /// Gets the range of cells starting at the specified row and column, with the specified row count and column count.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        IRange this[int row, int column, int rowCount, int columnCount] { get; }

        /// <summary>
        /// Gets the row count of the range.
        /// </summary>
        int RowCount { get; }
        /// <summary>
        /// Gets the column count of the range.
        /// </summary>
        int ColumnCount { get; }
    }
}
