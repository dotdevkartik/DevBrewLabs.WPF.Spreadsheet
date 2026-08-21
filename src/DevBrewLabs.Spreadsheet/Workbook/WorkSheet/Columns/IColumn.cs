using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    public interface IColumn : ISheetDimension, IStyledObject
    {
        /// <summary>
        /// Gets the parent column collection.
        /// </summary>
        IColumns Parent { get; }
        /// <summary>
        /// Gets or sets the width of this column.
        /// </summary>
        int Width { get; set; }
        /// <summary>
        /// Gets or sets the data map for this column.
        /// </summary>
        DataMap DataMap { get; set; }
        /// <summary>
        /// Gets or sets the cell type for this column.
        /// </summary>
        ICellType CellType { get; set; }
        /// <summary>
        /// Gets or sets whether filtering is allowed on this column.
        /// Effective filter button visibility requires:
        ///   Spread.AllowFiltering AND AutoFilter range contains column AND Column.AllowFiltering
        /// Default: true.
        /// </summary>
        bool AllowFiltering { get; set; }
    }
}
