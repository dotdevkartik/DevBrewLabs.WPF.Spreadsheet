using System;

namespace DevBrewLabs.Spreadsheet
{
    public class WorksheetAddedEventArgs : EventArgs
    {
        public IWorksheet AddedSheet { get; }

        public WorksheetAddedEventArgs(IWorksheet workSheet)
        {
            AddedSheet = workSheet;
        }
    }
}
