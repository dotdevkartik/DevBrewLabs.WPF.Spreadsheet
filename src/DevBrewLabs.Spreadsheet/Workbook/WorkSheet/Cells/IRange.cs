using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    // Represents a cell or range of cells in the spreadsheet.
    public interface IRange : IStyledObject
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
        IRange this[int row, int column] { get; }
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
        /// <summary>
        /// Gets the parent cell collection.
        /// </summary>
        IRange ParentRange { get; }
        /// <summary>
        /// Gets or sets the cell value.
        /// </summary>
        object Value { get; set; }
        /// <summary>
        /// Gets or sets the cell formula.
        /// </summary>
        string Formula { get; set; }
        /// <summary>
        /// Gets whether the cell has formula.
        /// </summary>
        bool HasFormula { get; }
        /// <summary>
        /// Gets or sets the cell type.
        /// </summary>
        ICellType CellType { get; set; }
        /// <summary>
        /// Gets or sets the data map.
        /// </summary>
        IDataMap DataMap { get; set; }
        /// <summary>
        /// Gets or sets whether the cell is locked.
        /// </summary>
        bool Locked { get; set; }
        /// <summary>
        /// Gets or sets the cell formatter.
        /// </summary>
        IFormatter Formatter { get; set; }
        /// <summary>
        /// Gets or sets the row span for this cell.
        /// </summary>
        int RowSpan { get; set; }
        /// <summary>
        /// Gets or sets the column span for this cell.
        /// </summary>
        int ColumnSpan { get; set; }
        /// <summary>
        /// Gets or sets the cell style.
        /// </summary>
        IStyle Style { get; set; }
    }
}
