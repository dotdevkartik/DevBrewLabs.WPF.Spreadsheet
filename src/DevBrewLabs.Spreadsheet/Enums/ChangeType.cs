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
        StyleName,
        Visibility
    }

    public enum ColumnChangeType
    {
        Insert,
        Delete,
        Width,
        Style,
        StyleName,
        Visibility
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

    public enum WorksheetChangeType
    {
        RowCount,
        ColumnCount,
        DefaultRowHeight,
        DefaultColumnWidth
    }
}
