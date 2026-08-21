namespace DevBrewLabs.Spreadsheet
{
    public interface IRows
    {
        /// <summary>
        /// Gets the row present at the provided index.
        /// </summary>
        /// <param name="index">
        /// Row index.
        /// </param>
        /// <returns></returns>
        IRow this[int index] { get; }
        /// <summary>
        /// Gets the row height.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        int GetRowHeight(int row);
        /// <summary>
        /// Gets whether the row is visible.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        bool IsRowVisible(int row);
    }
}
