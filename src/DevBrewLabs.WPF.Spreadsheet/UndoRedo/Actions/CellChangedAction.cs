namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class CellChangedAction : SheetAction
    {
        public CellEditState OldState { get; private set; }
        public CellEditState NewState { get; private set; }
        public SheetView SheetView { get; set; }

        public CellChangedAction()
        {
            OldState = new CellEditState();
            NewState = new CellEditState();
        }

        public override void Undo()
        {
            Execute(OldState);
        }

        public override void Redo()
        {
            Execute(NewState);
        }

        private void Execute(CellEditState state)
        {
            SheetView.WorkSheet.SetValue(state.Row, state.Column, state.Value);
            var selection = state.Selection;
            SheetView.ActiveRow = state.Row;
            SheetView.ActiveColumn = state.Column;
            SheetView.SelectRange(selection.TopRow, selection.LeftColumn, selection.RowCount, selection.ColumnCount);
            SheetView.Spread.Invalidate();
        }
    }
}
