using System;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal class ColumnHeaderRows : SheetDimensionCollection<IRow>, IRows, IDisposable
    {
        private WorkSheet _workSheet;

        public ColumnHeaders ColumnHeaders { get; }
        internal ColumnHeaderRows(ColumnHeaders parent) : base()
        {
            ColumnHeaders = parent;
            _workSheet = parent.WorkSheet;
        }

        protected override IRow CreateItem(int index)
        {
            var row = new ColumnHeaderRow(this);
            row.Index = index;
            return row;
        }

        public int GetRowHeight(int row)
        {
            var sheetRow = GetItem(row, false);

            if (sheetRow == null)
                return ColumnHeaders.DefaultRowHeight;

            return sheetRow.Height;
        }

        public int GetRowIndex(IRow row)
        {
            return GetIndex(row);
        }

        public override void Insert(int index, int count)
        {

        }

        public override void Remove(int index, int count)
        {

        }
    }
}
