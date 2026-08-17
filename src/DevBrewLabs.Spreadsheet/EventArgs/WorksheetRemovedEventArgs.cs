using System;

namespace DevBrewLabs.Spreadsheet
{
    public class WorksheetRemovedEventArgs : EventArgs
    {
        public IWorksheet RemovedSheet { get; }

        public WorksheetRemovedEventArgs(IWorksheet workSheet)
        {
            RemovedSheet = workSheet;
        }
    }
}
