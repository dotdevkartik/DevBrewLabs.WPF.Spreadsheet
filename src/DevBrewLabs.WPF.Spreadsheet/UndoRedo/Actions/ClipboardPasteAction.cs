namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class ClipboardPasteAction : SheetAction
    {
        public CellEditState OldState { get; private set; }
        public CellEditState NewState { get; private set; }
        public SheetView SheetView { get; set; }

        public ClipboardPasteAction()
        {
            OldState = new CellEditState();
            NewState = new CellEditState();
        }

        public override void Redo()
        {
            Execute(NewState);
        }

        public override void Undo()
        {
            Execute(OldState);
        }

        private void Execute(CellEditState state)
        {
            var data = (object[,])state.Value;
            var formulas = state.Formulas;

            SheetView.Spread.SuspendUpdates = true;

            for (int row = 0; row < data.GetLength(0); row++)
            {
                for (int column = 0; column < data.GetLength(1); column++)
                {
                    int r = state.Row + row;
                    int c = state.Column + column;
                    string formula = formulas != null ? formulas[row, column] : null;

                    if (!string.IsNullOrEmpty(formula))
                    {
                        SheetView.WorkSheet.SetFormula(r, c, formula);
                    }
                    else
                    {
                        SheetView.WorkSheet.SetFormula(r, c, null);
                        var value = data[row, column];
                        if (value is string strVal)
                        {
                            try
                            {
                                SheetView.WorkSheet.SetRawValue(r, c, strVal);
                            }
                            catch
                            {
                                SheetView.WorkSheet.SetValue(r, c, strVal);
                            }
                        }
                        else
                        {
                            SheetView.WorkSheet.SetValue(r, c, value);
                        }
                    }
                }
            }

            var selection = state.Selection;
            SheetView.ActiveRow = state.Row;
            SheetView.ActiveColumn = state.Column;
            SheetView.SelectRange(selection.TopRow, selection.LeftColumn, selection.RowCount, selection.ColumnCount);
            SheetView.Spread.SuspendUpdates = false;
        }
    }
}
