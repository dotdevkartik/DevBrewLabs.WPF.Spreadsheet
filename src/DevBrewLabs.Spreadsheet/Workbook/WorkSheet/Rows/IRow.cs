using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    public interface IRow : ISheetDimension, IStyledObject
    {
        /// <summary>
        /// Gets the parent row collection.
        /// </summary>
        IRows Parent { get; }
        /// <summary>
        /// Gets or sets the height of this row.
        /// </summary>
        int Height { get; set; }
        /// <summary>
        /// Gets or sets whether this row is manually hidden.
        /// </summary>
        bool IsHidden { get; set; }
        /// <summary>
        /// Gets or sets whether this row is filtered out by AutoFilter.
        /// </summary>
        bool IsFilteredOut { get; set; }
    }
}
