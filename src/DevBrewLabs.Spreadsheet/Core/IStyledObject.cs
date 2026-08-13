namespace DevBrewLabs.Spreadsheet
{
    public interface IStyledObject
    {
        /// <summary>
        /// Gets or sets the style for this object.
        /// </summary>
        IStyle Style { get; set; }
        /// <summary>
        /// Gets or sets the style name.
        /// </summary>
        string StyleName { get; set; }
    }
}
