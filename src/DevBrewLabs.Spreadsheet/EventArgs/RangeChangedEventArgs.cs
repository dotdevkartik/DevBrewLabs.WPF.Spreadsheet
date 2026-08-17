namespace DevBrewLabs.Spreadsheet
{
    public sealed class RangeChangedEventArgs : WorksheetChangeEventArgsBase
    {
        public RangeChangedEventArgs(
            SheetRegion region,
            IWorksheet workSheet,
            CellRange range,
            object oldValue,
            object newValue,
            RangeChangeType changeType) : base(region, workSheet, oldValue, newValue)
        {
            Range = range;
            ChangeType = changeType;
        }

        public CellRange Range { get; }
        public RangeChangeType ChangeType { get; }
    }
}
