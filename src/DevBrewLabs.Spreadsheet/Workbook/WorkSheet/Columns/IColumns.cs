namespace DevBrewLabs.Spreadsheet
{
    public interface IColumns
    {
        /// <summary>
        /// Gets the column present at the provided index.
        /// </summary>
        /// <param name="index">
        /// Column index. 
        /// </param>
        /// <returns></returns>
        IColumn this[int index] { get; }
        /// <summary>
        /// Gets the column present at the specified index without creating it if it doesn't exist.
        /// </summary>
        /// <param name="index">Column index.</param>
        /// <returns>The column, or null if not created.</returns>
        IColumn GetItem(int index);
        /// <summary>
        /// Gets the column with specific column name.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        IColumn this[string address] { get; }
        /// <summary>
        /// Gets the column width.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        int GetColumnWidth(int column);
        /// <summary>
        /// Gets whether the column is visible.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        bool IsColumnVisible(int column);
        /// <summary>
        /// Gets the column index of the provided column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        int GetColumnIndex(IColumn column);
    }
}
