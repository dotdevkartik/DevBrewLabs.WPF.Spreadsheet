using System;

namespace DevBrewLabs.Spreadsheet
{
    public class WorksheetEventArgs : EventArgs
    {
        public IWorksheet Worksheet { get; }

        public WorksheetEventArgs(IWorksheet workSheet)
        {
            Worksheet = workSheet;
        }
    }
}
