namespace DevBrewLabs.Spreadsheet
{
    public interface IRowHeaders : ICellContainer
    {
        /// <summary>
        /// Gets or sets the header column count.
        /// </summary>
        int ColumnCount { get; set; }
        /// <summary>
        /// Gets or sets the default column width.
        /// </summary>
        int DefaultColumnWidth { get; set; }
        /// <summary>
        /// Gets the row headers width.
        /// </summary>
        double Width { get; }
        /// <summary>
        /// Gets the row header cells
        /// </summary>
        IRange Cells { get; }
        /// <summary>
        /// Gets the row header columns.
        /// </summary>
        IColumns Columns { get; }
    }
}
