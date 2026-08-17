namespace DevBrewLabs.Spreadsheet
{
    public sealed class CellChangedEventArgs : WorksheetChangeEventArgsBase
    {
        public CellChangedEventArgs(
            SheetRegion region,
            IWorksheet workSheet,
            int row,
            int column,
            object oldValue,
            object newValue,
            CellChangeType changeType) : base(region, workSheet, oldValue, newValue)
        {
            Row = row;
            Column = column;
            ChangeType = changeType;
        }

        public int Row { get; }
        public int Column { get; }
        public CellChangeType ChangeType { get; }
    }
}
