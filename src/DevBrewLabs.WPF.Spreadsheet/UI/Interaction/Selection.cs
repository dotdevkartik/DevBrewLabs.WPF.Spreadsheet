using DevBrewLabs.Spreadsheet;
using System.Collections.Generic;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    public class Selection
    {
        private readonly List<CellRange> _ranges = new List<CellRange>();

        public IEnumerable<CellRange> Ranges => _ranges;

        public Selection()
        {
        }

        public void Add(CellRange range)
        {
            if (range.IsValid)
            {
                _ranges.Add(range);
            }
        }

        public void Clear()
        {
            _ranges.Clear();
        }
    }
}
