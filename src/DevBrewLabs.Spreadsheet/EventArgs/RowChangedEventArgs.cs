namespace DevBrewLabs.Spreadsheet
{
    public sealed class RowChangedEventArgs : WorksheetChangeEventArgsBase
    {
        public RowChangedEventArgs(
            SheetRegion region,
            IWorksheet workSheet,
            int index,
            object oldValue,
            object newValue,
            RowChangeType changeType) : base(region, workSheet, oldValue, newValue)
        {
            Index = index;
            ChangeType = changeType;
        }

        public int Index { get; }
        public RowChangeType ChangeType { get; }
    }
}
