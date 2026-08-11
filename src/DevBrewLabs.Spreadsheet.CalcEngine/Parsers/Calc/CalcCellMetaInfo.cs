using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Parsers
{
    internal class CalcCellMetaInfo
    {

        public bool IsEvaluating { get; set; }
        public object CalculatedValue { get; set; }
        public ISet<CellRef> Dependents { get; set; }
        public IList<object> Dependencies { get; set; }

        public CalcCellMetaInfo()
        {
            Dependents = new HashSet<CellRef>();
            Dependencies = new List<object>();
        }
    }
}
