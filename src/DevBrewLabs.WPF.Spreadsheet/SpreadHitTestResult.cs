using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet
{
    public class SpreadHitTestResult
    {
        public ISheetView Sheet { get; internal set; }
        public int Row { get; internal set; }
        public int Column { get; internal set; }
        /// <summary>
        /// Position of the hittest element.
        /// </summary>
        internal Point Position { get; set; }
        /// <summary>
        /// Actual hit test position
        /// </summary>
        internal Point ActualHitTestPoint { get; set; }
        public SheetElement Element { get; internal set; }
        public CellTypes.CellElement CellElement { get; internal set; }
    }
}
