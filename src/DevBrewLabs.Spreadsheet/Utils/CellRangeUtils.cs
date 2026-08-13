using System;

namespace DevBrewLabs.Spreadsheet.Utils
{
    internal static class CellRangeUtils
    {
        public static void ValidateIndexes(this Cells range, int row, int column, int rowCount, int columnCount)
        {
            ValidateIndexesImpl(range.Row, range.Column, range.RowCount, range.ColumnCount, row, column, rowCount, columnCount);
        }

        public static void ValidateIndexes(this RowHeaderCells range, int row, int column, int rowCount, int columnCount)
        {
            ValidateIndexesImpl(range.Row, range.Column, range.RowCount, range.ColumnCount, row, column, rowCount, columnCount);
        }

        public static void ValidateIndexes(this ColumnHeaderCells range, int row, int column, int rowCount, int columnCount)
        {
            ValidateIndexesImpl(range.Row, range.Column, range.RowCount, range.ColumnCount, row, column, rowCount, columnCount);
        }

        private static void ValidateIndexesImpl(int rangeRow, int rangeColumn, int rangeRowCount, int rangeColumnCount, 
            int row, int column, int rowCount, int columnCount)
        {
            if (row < rangeRow || row >= rangeRow + rangeRowCount)
                throw new ArgumentOutOfRangeException(nameof(row), "Row index is out of range.");
            if (column < rangeColumn || column >= rangeColumn + rangeColumnCount)
                throw new ArgumentOutOfRangeException(nameof(column), "Column index is out of range.");
            if (rowCount <= 0 || row + rowCount > rangeRow + rangeRowCount)
                throw new ArgumentOutOfRangeException(nameof(rowCount), "Row count is out of range.");
            if (columnCount <= 0 || column + columnCount > rangeColumn + rangeColumnCount)
                throw new ArgumentOutOfRangeException(nameof(columnCount), "Column count is out of range.");
        }
    }
}
