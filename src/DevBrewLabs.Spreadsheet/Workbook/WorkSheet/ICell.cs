using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    /// <summary>
    /// Represents a single cell in a worksheet.
    /// </summary>
    public interface ICell : IStyledObject
    {
        /// <summary>
        /// Gets or sets the cell formatter.
        /// </summary>
        IFormatter Formatter { get; set; }

        /// <summary>
        /// Gets or sets the cell value.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets the cell formula.
        /// </summary>
        string Formula { get; set; }

        /// <summary>
        /// Gets or sets the data map associated with the cell.
        /// </summary>
        IDataMap DataMap { get; set; }

        /// <summary>
        /// Gets or sets the cell type.
        /// </summary>
        ICellType CellType { get; set; }

        /// <summary>
        /// Gets or sets whether the cell is locked.
        /// </summary>
        bool Locked { get; set; }

        /// <summary>
        /// Gets or sets the row span for the cell.
        /// </summary>
        int RowSpan { get; set; }

        /// <summary>
        /// Gets or sets the column span for the cell.
        /// </summary>
        int ColumnSpan { get; set; }

        /// <summary>
        /// Gets the parent range containing the cell.
        /// </summary>
        IRange ParentRange { get; }
    }
}
