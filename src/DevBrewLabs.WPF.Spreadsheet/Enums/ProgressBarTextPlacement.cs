namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Specifies where the progress text/percentage is displayed relative to the progress bar.
    /// </summary>
    public enum ProgressBarTextPlacement
    {
        /// <summary>
        /// Text is rendered centered over the progress bar track.
        /// </summary>
        Overlay,

        /// <summary>
        /// The progress bar is drawn on the left, and text is rendered to its right.
        /// </summary>
        Right,

        /// <summary>
        /// No text is rendered.
        /// </summary>
        None
    }
}
