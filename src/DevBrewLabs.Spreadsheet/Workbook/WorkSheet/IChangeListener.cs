namespace DevBrewLabs.Spreadsheet
{
    internal interface IChangeListener
    {
        void OnWorksheetChanged(WorksheetChangedEventArgs args);
        void CellChanged(CellChangedEventArgs args);
        void RangeChanged(RangeChangedEventArgs args);
        void RowChanged(RowChangedEventArgs args);
        void ColumnChanged(ColumnChangedEventArgs args);
    }
}
