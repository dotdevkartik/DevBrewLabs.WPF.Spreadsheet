namespace DevBrewLabs.WPF.Spreadsheet
{
    public interface IUndoRedoManager
    {
        int MaxCapacity { get; set; }
        bool CanUndo { get; }
        bool CanRedo { get; }

        void AddAction(SheetAction action);
        void BeginGroup();
        void EndGroup();
        void Redo();
        void Undo();
    }
}
