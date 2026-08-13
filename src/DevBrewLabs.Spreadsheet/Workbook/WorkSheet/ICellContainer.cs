using DevBrewLabs.Spreadsheet.Formatters;

namespace DevBrewLabs.Spreadsheet
{
    /// <summary>
    /// Provides access to cells and their associated properties within a cell container.
    /// </summary>
    public interface ICellContainer
    {
        /// <summary>
        /// Gets the data map for the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The data map associated with the cell.</returns>
        IDataMap GetDataMap(int row, int column);

        /// <summary>
        /// Sets the data map for the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="dataMap">The data map to associate with the cell.</param>
        void SetDataMap(int row, int column, IDataMap dataMap);

        /// <summary>
        /// Gets the style of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The style of the cell.</returns>
        IStyle GetStyle(int row, int column);

        /// <summary>
        /// Sets the style of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="style">The style to apply to the cell.</param>
        void SetStyle(int row, int column, IStyle style);

        /// <summary>
        /// Gets the name of the style applied to the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The style name of the cell.</returns>
        string GetStyleName(int row, int column);

        /// <summary>
        /// Sets the style name for the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="styleName">The name of the style to apply to the cell.</param>
        void SetStyleName(int row, int column, string styleName);

        /// <summary>
        /// Gets the value of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The value of the cell.</returns>
        object GetValue(int row, int column);

        /// <summary>
        /// Sets the value of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="value">The value to set.</param>
        void SetValue(int row, int column, object value);

        /// <summary>
        /// Gets whether the specified cell contains a formula.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns><c>true</c> if the cell contains a formula; otherwise, <c>false</c>.</returns>
        bool HasFormula(int row, int column);

        /// <summary>
        /// Gets the formula of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The formula of the cell.</returns>
        string GetFormula(int row, int column);

        /// <summary>
        /// Sets the formula for the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="formula">The formula to set.</param>
        void SetFormula(int row, int column, string formula);

        /// <summary>
        /// Gets the formatter of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The formatter of the cell.</returns>
        IFormatter GetFormatter(int row, int column);

        /// <summary>
        /// Sets the formatter for the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="formatter">The formatter to apply to the cell.</param>
        void SetFormatter(int row, int column, IFormatter formatter);

        /// <summary>
        /// Gets whether the specified cell is locked.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns><c>true</c> if the cell is locked; otherwise, <c>false</c>.</returns>
        bool GetLocked(int row, int column);

        /// <summary>
        /// Sets whether the specified cell is locked.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="locked"><c>true</c> to lock the cell; otherwise, <c>false</c>.</param>
        void SetLocked(int row, int column, bool locked);

        /// <summary>
        /// Gets the cell type of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The cell type.</returns>
        ICellType GetCellType(int row, int column);

        /// <summary>
        /// Sets the cell type of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="cellType">The cell type to set.</param>
        void SetCellType(int row, int column, ICellType cellType);

        /// <summary>
        /// Gets the row span of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The number of rows spanned by the cell.</returns>
        int GetRowSpan(int row, int column);

        /// <summary>
        /// Sets the row span of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="rowSpan">The number of rows to span.</param>
        void SetRowSpan(int row, int column, int rowSpan);

        /// <summary>
        /// Gets the column span of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <returns>The number of columns spanned by the cell.</returns>
        int GetColumnSpan(int row, int column);

        /// <summary>
        /// Sets the column span of the specified cell.
        /// </summary>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="columnSpan">The number of columns to span.</param>
        void SetColumnSpan(int row, int column, int columnSpan);

        /// <summary>
        /// Adds a merged span.
        /// </summary>
        void AddSpan(int row, int column, int rowCount, int columnCount);

        /// <summary>
        /// Removes a merged span.
        /// </summary>
        void RemoveSpan(int row, int column);

        /// <summary>
        /// Sets a raw string value to a cell, automatically inferring data types or formulas.
        /// </summary>
        void SetRawValue(int row, int column, string value);

        /// <summary>
        /// Gets the span range for the specified cell, or null if the cell is not part of a span.
        /// </summary>
        CellRange GetSpanCellRange(int row, int column);

        /// <summary>
        /// Expands the given cell range to fully include any intersecting spans.
        /// </summary>
        CellRange ExpandSpanRange(CellRange range);

        /// <summary>
        /// Gets a value indicating whether there are any merged spans in this sheet.
        /// </summary>
        bool HasSpans { get; }

        /// <summary>
        /// Gets a value indicating whether the specified cell is covered by a span (and is not the anchor).
        /// </summary>
        bool IsCovered(int row, int column);

    }
}