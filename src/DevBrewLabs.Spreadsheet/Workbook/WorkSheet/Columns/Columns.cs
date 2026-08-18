using System;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal class Columns : SheetDimensionCollection<IColumn>, IColumns, IDisposable
    {
        public IColumn this[string address]
        {
            get
            {               
                return this[Extensions.GetColumnIndex(address)];
            }
        }

        public Worksheet WorkSheet { get; }

        internal Columns(Worksheet parent) : base()
        {
            WorkSheet = parent;
        }

        protected override IColumn CreateItem(int index)
        {
            var column =  new Column(this);
            column.Index = index;
            return column;
        }

        public int GetColumnWidth(int column)
        {
            var col = GetItem(column, false);

            if (col == null)
                return WorkSheet.DefaultColumnWidth;
            
            return col.Width;
        }

        public int GetColumnIndex(IColumn column)
        {
            return GetIndex(column);
        }

        public override void Insert(int index, int count)
        {
            
        }

        public override void Remove(int index, int count)
        {
            
        }
    }
}
