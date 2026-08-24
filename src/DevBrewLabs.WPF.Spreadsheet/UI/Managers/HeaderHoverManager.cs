namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class HeaderHoverManager : UIManager
    {
        public int HoveredColumn { get; private set; } = -1;
        public int HoveredRow { get; private set; } = -1;

        public HeaderHoverManager(Spread spread) : base(spread)
        {
        }

        public void SetHoveredColumn(SheetView view, int column)
        {
            if (HoveredColumn == column)
                return;

            HoveredColumn = column;
            Spread?.InvalidateSurfaces(rowHeaders: false, columnHeaders: true, cells: false, gridLines: false, topLeft: false);
        }

        public void SetHoveredRow(SheetView view, int row)
        {
            if (HoveredRow == row)
                return;

            HoveredRow = row;
            Spread?.InvalidateSurfaces(rowHeaders: true, columnHeaders: false, cells: false, gridLines: false, topLeft: false);
        }

        public void ClearHoveredColumn(SheetView view)
        {
            if (HoveredColumn != -1)
            {
                HoveredColumn = -1;
                Spread?.InvalidateSurfaces(rowHeaders: false, columnHeaders: true, cells: false, gridLines: false, topLeft: false);
            }
        }

        public void ClearHoveredRow(SheetView view)
        {
            if (HoveredRow != -1)
            {
                HoveredRow = -1;
                Spread?.InvalidateSurfaces(rowHeaders: true, columnHeaders: false, cells: false, gridLines: false, topLeft: false);
            }
        }

        public void ClearAll(SheetView view)
        {
            bool colChanged = HoveredColumn != -1;
            bool rowChanged = HoveredRow != -1;
            HoveredColumn = -1;
            HoveredRow = -1;

            if (colChanged || rowChanged)
            {
                Spread?.InvalidateSurfaces(rowHeaders: rowChanged, columnHeaders: colChanged, cells: false, gridLines: false, topLeft: false);
            }
        }
    }
}
