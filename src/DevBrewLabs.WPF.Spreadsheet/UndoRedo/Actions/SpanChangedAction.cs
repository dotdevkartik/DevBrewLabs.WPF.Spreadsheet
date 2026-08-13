namespace DevBrewLabs.WPF.Spreadsheet
{
    using DevBrewLabs.Spreadsheet;

    internal class SpanChangedAction : SheetAction
    {
        public int Row { get; set; }
        public int Column { get; set; }
        
        public int OldRowSpan { get; set; }
        public int OldColumnSpan { get; set; }
        
        public int NewRowSpan { get; set; }
        public int NewColumnSpan { get; set; }

        public object[,] OldValues { get; set; }

        public SheetView SheetView { get; set; }

        public override void Undo()
        {
            var workSheet = SheetView.WorkSheet;
            
            if (OldRowSpan <= 1 && OldColumnSpan <= 1)
            {
                workSheet.RemoveSpan(Row, Column);
                
                if (OldValues != null)
                {
                    int rowCount = OldValues.GetLength(0);
                    int colCount = OldValues.GetLength(1);
                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            workSheet.SetValue(Row + r, Column + c, OldValues[r, c]);
                        }
                    }
                }
            }
            else
            {
                workSheet.AddSpan(Row, Column, OldRowSpan, OldColumnSpan);
            }
            
            SheetView.ActiveRow = Row;
            SheetView.ActiveColumn = Column;
            SheetView.Spread.SelectionManager.SelectRange(Row, Column, OldRowSpan <= 1 ? 1 : OldRowSpan, OldColumnSpan <= 1 ? 1 : OldColumnSpan);
            SheetView.Spread.Invalidate();
        }

        public override void Redo()
        {
            var workSheet = SheetView.WorkSheet;
            
            if (NewRowSpan <= 1 && NewColumnSpan <= 1)
            {
                workSheet.RemoveSpan(Row, Column);
            }
            else
            {
                workSheet.AddSpan(Row, Column, NewRowSpan, NewColumnSpan);
            }
            
            SheetView.ActiveRow = Row;
            SheetView.ActiveColumn = Column;
            SheetView.Spread.SelectionManager.SelectRange(Row, Column, NewRowSpan <= 1 ? 1 : NewRowSpan, NewColumnSpan <= 1 ? 1 : NewColumnSpan);
            SheetView.Spread.Invalidate();
        }
    }
}
