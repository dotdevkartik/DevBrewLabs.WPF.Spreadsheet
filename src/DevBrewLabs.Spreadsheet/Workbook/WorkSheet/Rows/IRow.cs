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
    }
}
