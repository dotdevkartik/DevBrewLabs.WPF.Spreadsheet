namespace DevBrewLabs.WPF.Spreadsheet.Enums
{
    /// <summary>
    /// Specifies the visual chart representation used by a <see cref="DevBrewLabs.WPF.Spreadsheet.CellTypes.SparklineCellType"/>.
    /// </summary>
    public enum SparklineType
    {
        /// <summary>
        /// A continuous polyline connecting sequential data points.
        /// </summary>
        Line,

        /// <summary>
        /// Vertical mini-bars plotted relative to a baseline.
        /// </summary>
        Column,

        /// <summary>
        /// Equal-height binary or ternary blocks indicating win (+), loss (-), or tie (0).
        /// </summary>
        WinLoss,

        /// <summary>
        /// A continuous polyline with a shaded translucent area beneath the line to the baseline.
        /// </summary>
        Area
    }
}
