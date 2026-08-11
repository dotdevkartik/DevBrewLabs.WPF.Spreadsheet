using DevBrewLabs.Evalis;
using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using System.Threading.Tasks;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Evaluator
{
    /// <summary>
    /// Represents the context for evaluating formulas in Sheets, providing access to cell and range values through an IDataProvider.
    /// </summary>
    public class CalcEngineContext : IEngineContext
    {
        private readonly IDataAdapter _dataProvider;
        private string _currentSheetName;

        public CalcEngineContext(IDataAdapter dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public string CurrentSheetName
        {
            get { return _currentSheetName; }
        }

        public void SetCurrentSheet(string sheetName)
        {
            _currentSheetName = sheetName;
        }

        public Task<object> Resolve(string key)
        {
            if (key.Contains(":"))
            {
                var rangeRef = new CellRangeRef(key);
                var sheetName = string.IsNullOrEmpty(rangeRef.SheetName) ? _currentSheetName : rangeRef.SheetName;
                var value = _dataProvider.GetRangeValue(sheetName, rangeRef.TopRow, rangeRef.LeftColumn, rangeRef.RowCount, rangeRef.ColumnCount);
                if (value is object[,] array2D)
                {
                    var flatArray = new object[array2D.Length];
                    int index = 0;
                    for (int r = 0; r < array2D.GetLength(0); r++)
                    {
                        for (int c = 0; c < array2D.GetLength(1); c++)
                        {
                            flatArray[index++] = array2D[r, c];
                        }
                    }
                    return Task.FromResult<object>(flatArray);
                }
                return Task.FromResult<object>(value);
            }
            else
            {
                var cellRef = new CellRef(key);
                var sheetName = string.IsNullOrEmpty(cellRef.SheetName) ? _currentSheetName : cellRef.SheetName;
                var value = _dataProvider.GetValue(sheetName, cellRef.Row, cellRef.Column);
                return Task.FromResult(value);
            }
        }
    }
}
