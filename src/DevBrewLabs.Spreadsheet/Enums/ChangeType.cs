namespace DevBrewLabs.Spreadsheet
{
    public enum CellChangeType
    {
        Value,
        Formula,
        Style,
        StyleName,
        Merge,
        Unmerge
    }

    public enum RowChangeType
    {
        Insert,
        Delete,
        Height,
        Style,
        StyleName
    }

    public enum ColumnChangeType
    {
        Insert,
        Delete,
        Width,
        Style,
        StyleName
    }

    public enum RangeChangeType
    {
        Sort,
        Clear,
        Move,
        Merge,
        Value,
        Formula,
        Style,
        StyleName
    }
}
