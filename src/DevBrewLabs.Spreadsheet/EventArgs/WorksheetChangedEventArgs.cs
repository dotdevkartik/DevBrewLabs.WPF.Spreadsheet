namespace DevBrewLabs.Spreadsheet
{
    public class WorksheetChangedEventArgs : WorksheetChangeEventArgsBase
    {
        public WorksheetChangedEventArgs(
            SheetRegion region, 
            IWorksheet sheet, 
            object oldValue, 
            object newValue, 
            WorksheetChangeType changeType) : base(region, sheet, oldValue, newValue)
        {
            ChangeType = changeType;
        }

        public WorksheetChangeType ChangeType { get; }
    }
}
