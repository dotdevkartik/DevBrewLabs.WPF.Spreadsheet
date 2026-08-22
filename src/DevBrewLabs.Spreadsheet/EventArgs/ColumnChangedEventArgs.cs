using System;

namespace DevBrewLabs.Spreadsheet
{
    public sealed class ColumnChangedEventArgs : WorksheetChangeEventArgsBase
    {
        public ColumnChangedEventArgs(
            SheetRegion region,
            IWorksheet workSheet,
            int index,
            object oldValue,
            object newValue,
            ColumnChangeType changeType) : base(region, workSheet, oldValue, newValue)
        {
            Index = index;
            ChangeType = changeType;
        }

        public int Index { get; }
        public ColumnChangeType ChangeType { get; }
    }

    public class CellValueSetFailedEventArgs : EventArgs
    {
        public int Row { get; }
        public int Column { get; }
        public object Value { get; }
        public Exception Exception { get; }

        public CellValueSetFailedEventArgs(int row, int col, object value, Exception ex)
        {
            Row = row;
            Column = col;
            Value = value;
            Exception = ex;
        }
    }
}
