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

        }

        public override void Remove(int index, int count)
        {
            
        }
    }
}
