namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Specifies when an underline should be rendered for hyperlinks.
    /// </summary>
    public enum HyperlinkUnderlineMode
    {
        /// <summary>
        /// The hyperlink is always underlined.
        /// </summary>
        Always,

        /// <summary>
        /// The hyperlink is underlined only when hovered.
        /// </summary>
        Hover,

        /// <summary>
        /// The hyperlink is never underlined.
        /// </summary>
        Never
    }
}
