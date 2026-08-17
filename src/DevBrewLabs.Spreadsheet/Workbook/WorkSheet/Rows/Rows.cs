using System;

namespace DevBrewLabs.Spreadsheet
{
    internal class Rows : SheetDimensionCollection<IRow>, IRows
    {
        public WorkSheet WorkSheet { get; }

        internal Rows(WorkSheet parent) : base()
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

        

        public override void Insert(int index, int count)
        {

        }

        public override void Remove(int index, int count)
        {
            
        }
    }
}
