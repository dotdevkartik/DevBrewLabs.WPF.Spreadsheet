using DevBrewLabs.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal class CellEditState
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public CellRange Selection { get; set; }
        public object Value { get; set; }
    }
}
