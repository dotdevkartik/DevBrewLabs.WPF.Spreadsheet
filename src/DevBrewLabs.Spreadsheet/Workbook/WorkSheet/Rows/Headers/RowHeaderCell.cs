using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Formatters;
using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class RowHeaderCell : IRange, IDisposable
    {
        private RowHeaderCells _parentRange;
        private WorkSheet _workSheet;

        public int RowCount => 1;
        public int ColumnCount => 1;
        internal object MetaData { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool HasFormula => !string.IsNullOrEmpty(Formula);
        public IRange ParentRange => _parentRange;
        public bool IsVisible { get; internal set; }
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

        public IRange this[int row, int column, int rowCount, int columnCount] => this;
        public IRange this[int row, int column] => this;
        public IRange this[string name] => this;

        public WorkSheet WorkSheet => _workSheet;

        internal RowHeaderCell(RowHeaderCells parent)
        {
            _parentRange = parent;
            _workSheet = parent.WorkSheet;
            IsVisible = true;
        }

        public void Dispose()
        {
            Value = null;
            Formula = null;
            Formatter = null;
            MetaData = null;
            DataMap = null;
            _parentRange = null;
            CellType = null;
            StyleName = null;
            Style = null;
        }
    }
}
