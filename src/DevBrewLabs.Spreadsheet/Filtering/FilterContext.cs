using System;

namespace DevBrewLabs.Spreadsheet.Filtering
{
    /// <summary>
    /// Context passed to filter conditions for evaluation.
    /// </summary>
    public readonly struct FilterContext
    {
        public IWorksheet Worksheet { get; }
        public int Row { get; }
        public int Column { get; }
        public object Value { get; }

        public FilterContext(IWorksheet worksheet, int row, int column, object value)
        {
            Worksheet = worksheet;
            Row = row;
            Column = column;
            Value = value;
        }
    }
}
