using System;

namespace DevBrewLabs.Spreadsheet.Filtering
{
    /// <summary>
    /// Event arguments for when the filter state changes.
    /// </summary>
    public class FilterChangedEventArgs : EventArgs
    {
        public IWorksheet Worksheet { get; }
        
        /// <summary>
        /// The column index that changed, or null if it was a general change (like ClearAll).
        /// </summary>
        public int? ColumnIndex { get; }
        
        public int VisibleRowCount { get; }
        public int TotalRowCount { get; }

        public FilterChangedEventArgs(IWorksheet worksheet, int? columnIndex, int visibleRowCount, int totalRowCount)
        {
            Worksheet = worksheet;
            ColumnIndex = columnIndex;
            VisibleRowCount = visibleRowCount;
            TotalRowCount = totalRowCount;
        }
    }
}
