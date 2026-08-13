using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class ColumnHeaderCell : ICell, IDisposable
    {
        private ColumnHeaderCells _parentRange;
        private WorkSheet _workSheet;

        public int Row { get; set; }
        public int Column { get; set; }
        public IRange ParentRange => _parentRange;
        public object Value { get; set; }
        public string Formula { get; set; }
        public ICellType CellType { get; set; }
        public IDataMap DataMap { get; set; }
        public bool Locked { get; set; }
        public IFormatter Formatter { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public IStyle Style { get; set; }
        public string StyleName { get; set; }

        public WorkSheet WorkSheet => _workSheet;

        internal ColumnHeaderCell(ColumnHeaderCells parent)
        {
            _parentRange = parent;
            _workSheet = parent.WorkSheet;
        }

        public void Dispose()
        {
            Value = null;
            Formula = null;
            Formatter = null;
            DataMap = null;
            _parentRange = null;
            CellType = null;
            StyleName = null;
            Style = null;
        }
    }
}
