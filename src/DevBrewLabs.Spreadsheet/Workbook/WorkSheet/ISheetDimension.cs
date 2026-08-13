using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    public interface ISheetDimension
    {
        /// <summary>
        /// Gets the index of this item in the collection.
        /// </summary>
        int Index { get; }
        /// <summary>
        /// Gets whether the row is visible.
        /// </summary>
        bool Visible { get; }
        /// <summary>
        /// Gets or sets whether the column supports editing.
        /// </summary>
        bool Locked { get; set; }
        /// <summary>
        /// Gets or sets the row formatter.
        /// </summary>
        IFormatter Formatter { get; set; }
    }
}
