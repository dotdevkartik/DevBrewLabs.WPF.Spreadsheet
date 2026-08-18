using System;

namespace DevBrewLabs.Spreadsheet
{
    public class WorksheetChangeEventArgsBase : WorksheetEventArgs
    {
        protected WorksheetChangeEventArgsBase(SheetRegion region, IWorksheet workSheet, object oldValue, object newValue) : base(workSheet)
        {
            Region = region;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object OldValue { get; }
        public object NewValue { get; }
        internal SheetRegion Region { get; }
    }
}
