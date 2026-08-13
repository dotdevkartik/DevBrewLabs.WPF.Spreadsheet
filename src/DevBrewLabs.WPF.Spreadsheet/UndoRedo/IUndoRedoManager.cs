namespace DevBrewLabs.WPF.Spreadsheet
{
    public interface IUndoRedoManager
    {
        int MaxCapacity { get; set; }

        void AddAction(SheetAction action);
        void BeginGroup();
        void EndGroup();
        void Redo();
        void Undo();
    }
}
