using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class Rows : SheetDimensionCollection<IRow>, IRows
    {
        public Worksheet WorkSheet { get; }

        internal Rows(Worksheet parent) : base()
        {
            WorkSheet = parent;
        }

        protected override IRow CreateItem(int index)
        {
            var row =  new Row(this);
            row.Index = index;
            return row;
        }

        public int GetRowHeight(int row)
        {
            var sheetRow = GetItem(row, false);

            if (sheetRow == null)
                return WorkSheet.DefaultRowHeight;

            return sheetRow.Height;
        }

        public bool IsRowVisible(int row)
        {
            var sheetRow = GetItem(row, false);

            if (sheetRow == null)
                return WorkSheet.DefaultRowHeight > 0;

            return sheetRow.Visible;
        }

        

        public override void Insert(int index, int count)
        {
            if (WorkSheet.IsBound)
                throw new InvalidOperationException("Cannot insert rows into a data-bound worksheet.");
            base.Insert(index, count);
            WorkSheet.InsertRowsCore(index, count);
        }

        public override void Remove(int index, int count)
        {
            if (WorkSheet.IsBound)
                throw new InvalidOperationException("Cannot remove rows from a data-bound worksheet.");
            base.Remove(index, count);
            WorkSheet.RemoveRowsCore(index, count);
        }
    }
}
