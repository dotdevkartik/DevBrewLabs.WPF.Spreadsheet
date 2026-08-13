namespace DevBrewLabs.Spreadsheet
{
    public interface IColumnHeaders : ICellContainer
    {
        /// <summary>
        /// Gets or sets the header row count.
        /// </summary>
        int RowCount { get; set; }
        /// <summary>
        /// Gets or sets the default row height.
        /// </summary>
        int DefaultRowHeight { get; set; }
        /// <summary>
        /// Gets the column headers height.
        /// </summary>
        double Height { get; }
        /// <summary>
        /// Gets the column header cells.
        /// </summary>
        IRange Cells { get; }
        /// <summary>
        /// Gets the column header rows.
        /// </summary>
        IRows Rows { get; }
    }
}
