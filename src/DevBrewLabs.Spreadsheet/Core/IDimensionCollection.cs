namespace DevBrewLabs.Spreadsheet
{
    public interface IDimensionCollection<T> where T : class
    {
        bool HasItems { get; }
        int GetIndex(T item);
        T GetItem(int index);
    }
}
