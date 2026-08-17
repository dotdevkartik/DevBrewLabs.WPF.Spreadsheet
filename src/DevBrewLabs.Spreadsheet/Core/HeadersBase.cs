using System;

namespace DevBrewLabs.Spreadsheet
{
    internal abstract class HeadersBase : IDisposable
    {
        protected Worksheet _workSheet;

        public Worksheet WorkSheet => _workSheet;

        internal HeadersBase(Worksheet workSheet)
        {
            _workSheet = workSheet;
        }

        public virtual void Dispose()
        {
            _workSheet = null;
        }
    }
}
