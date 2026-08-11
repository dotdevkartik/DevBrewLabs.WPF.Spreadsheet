namespace DevBrewLabs.WPF.Spreadsheet
{
    public abstract class SheetAction
    {
        public abstract void Redo();
        public abstract void Undo();
    }
}
