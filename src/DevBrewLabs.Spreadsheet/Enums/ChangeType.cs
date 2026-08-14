namespace DevBrewLabs.Spreadsheet
{
    public enum CellChangeType
    {
        Value,
        Formula,
        Style,
        Merge,
        Unmerge
    }

    public enum RowChangeType
    {
        Insert,
        Delete,
        Height,
        Style
    }

    public enum ColumnChangeType
    {
        Insert,
        Delete,
        Width,
        Style
    }

    public enum RangeChangeType
    {
        Sort,
        Clear,
        Move,
        Merge,
        Value,
        Formula,
        Style
    }
}
