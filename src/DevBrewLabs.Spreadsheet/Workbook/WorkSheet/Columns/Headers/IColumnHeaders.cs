using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    public interface IColumnHeaders
    {
        /// <summary>
        /// Gets or sets the header row count.
        /// </summary>
        int RowCount { get; set; }
        /// <summary>
        /// Gets or sets the default row height.
        /// </summary>
        int DefaultRowHeight { get; set; }
        /// <summary>
        /// Gets the column headers height.
        /// </summary>
        double Height { get; }
        /// <summary>
        /// Gets the column header cells.
        /// </summary>
        IRange Cells { get; }

        #region Facade Methods
        IDataMap GetDataMap(int row, int column);
        void SetDataMap(int row, int column, IDataMap dataMap);
        IStyle GetStyle(int row, int column);
        void SetStyle(int row, int column, IStyle style);
        string GetStyleName(int row, int column);
        void SetStyleName(int row, int column, string styleName);
        object GetValue(int row, int column);
        void SetValue(int row, int column, object value);
        bool HasFormula(int row, int column);
        string GetFormula(int row, int column);
        void SetFormula(int row, int column, string formula);
        IFormatter GetFormatter(int row, int column);
        void SetFormatter(int row, int column, IFormatter formatter);
        bool GetLocked(int row, int column);
        void SetLocked(int row, int column, bool locked);
        ICellType GetCellType(int row, int column);
        void SetCellType(int row, int column, ICellType cellType);
        int GetRowSpan(int row, int column);
        void SetRowSpan(int row, int column, int rowSpan);
        int GetColumnSpan(int row, int column);
        void SetColumnSpan(int row, int column, int columnSpan);
        #endregion

        /// <summary>
        /// Gets the column header rows.
        /// </summary>
        IRows Rows { get; }
    }
}
