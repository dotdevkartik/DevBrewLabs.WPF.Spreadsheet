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
}
