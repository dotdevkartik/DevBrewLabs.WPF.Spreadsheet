namespace DevBrewLabs.WPF.Spreadsheet.Enums
{
    /// <summary>
    /// Specifies how an in-place cell editing session was triggered.
    /// </summary>
    public enum EditTrigger
    {
        /// <summary>
        /// The user typed a character while the cell was selected (clears existing value, inserts typed character).
        /// </summary>
        DirectTyping,

        /// <summary>
        /// The user pressed F2 (retains existing value, places caret at the end of text).
        /// </summary>
        F2Key,

        /// <summary>
        /// The user double-clicked the cell (retains existing value, places caret at the end).
        /// </summary>
        DoubleClick,

        /// <summary>
        /// Editing was triggered programmatically via Spread.BeginEdit().
        /// </summary>
        Programmatic,

        /// <summary>
        /// The user clicked an interactive dropdown button element on the cell.
        /// </summary>
        DropdownClick
    }
}
