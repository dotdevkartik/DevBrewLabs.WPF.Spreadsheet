using System;

namespace DevBrewLabs.Spreadsheet
{
    public class SheetChangedEventArgs : EventArgs
    {
        protected SheetChangedEventArgs(SheetRegion region, IWorkSheet workSheet, object oldValue, object newValue)
        {
            Region = region;
            WorkSheet = workSheet;
            OldValue = oldValue;
            NewValue = newValue;
        }

        protected SheetChangedEventArgs(SheetRegion region, IWorkSheet workSheet)
        {
            Region = region;
            WorkSheet = workSheet;
        }

        public SheetChangedEventArgs(IWorkSheet workSheet)
        {
            WorkSheet = workSheet;
        }

        public IWorkSheet WorkSheet { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        internal SheetRegion Region { get; set; }
    }

    public sealed class CellChangedEventArgs : SheetChangedEventArgs
    {
        public CellChangedEventArgs(
            SheetRegion region,
            IWorkSheet workSheet,
            int row,
            int column,
            object oldValue,
            object newValue,
            CellChangeType changeType)
            : base(region, workSheet, oldValue, newValue)
        {
            Row = row;
            Column = column;
            ChangeType = changeType;
        }

        public int Row { get; }
        public int Column { get; }
        public CellChangeType ChangeType { get; }
    }

    public sealed class RangeChangedEventArgs : SheetChangedEventArgs
    {
        public RangeChangedEventArgs(
            SheetRegion region,
            IWorkSheet workSheet,
            CellRange range,
            RangeChangeType changeType)
            : base(region, workSheet)
        {
            Range = range;
            ChangeType = changeType;
        }

        public CellRange Range { get; }
        public RangeChangeType ChangeType { get; }
    }

    public sealed class RowChangedEventArgs : SheetChangedEventArgs
    {
        public RowChangedEventArgs(
            SheetRegion region,
            IWorkSheet workSheet,
            int index,
            object oldValue,
            object newValue,
            RowChangeType changeType)
            : base(region, workSheet, oldValue, newValue)
        {
            Index = index;
            ChangeType = changeType;
        }

        public int Index { get; }
        public RowChangeType ChangeType { get; }
    }

    public sealed class ColumnChangedEventArgs : SheetChangedEventArgs
    {
        public ColumnChangedEventArgs(
            SheetRegion region,
            IWorkSheet workSheet,
            int index,
            object oldValue,
            object newValue,
            ColumnChangeType changeType)
            : base(region, workSheet, oldValue, newValue)
        {
            Index = index;
            ChangeType = changeType;
        }

        public int Index { get; }
        public ColumnChangeType ChangeType { get; }
    }
}
