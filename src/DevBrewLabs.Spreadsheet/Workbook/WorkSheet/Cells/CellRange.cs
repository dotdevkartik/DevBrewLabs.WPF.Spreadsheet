namespace DevBrewLabs.Spreadsheet
{
    /// <summary>
    /// Represents a cell range in worbook
    /// </summary>
    public struct CellRange
    {
        /// <summary>
        /// Gets whether this range contains only single cell.
        /// </summary>
        public bool IsSingleCell
        {
            get
            {
                return RowCount == 1 && ColumnCount == 1;
            }
        }

        /// <summary>
        /// Gets the top row in this range.
        /// </summary>
        public int TopRow
        {
            get
            {
                return _topRow;
            }
        }

        /// <summary>
        /// Gets the bottom row in this range.
        /// </summary>
        public int BottomRow
        {
            get
            {
                return RowCount == 0 ? -1 : TopRow + RowCount - 1;
            }
        }

        /// <summary>
        /// Gets the left column in this range.
        /// </summary>
        public int LeftColumn
        {
            get
            {
                return _leftColumn;
            }
        }

        /// <summary>
        /// Gets the right column in this range.
        /// </summary>
        public int RightColumn
        {
            get
            {
                return ColumnCount == 0 ? -1 : LeftColumn + ColumnCount - 1;
            }
        }

        /// <summary>
        /// Gets the row count of this range.
        /// </summary>
        public int RowCount
        {
            get
            {
                return _rowCount;
            }
        }

        /// <summary>
        /// Gets the column count of this range.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                return _columnCount;
            }
        }

        /// <summary>
        /// Gets whether this range is valid or not.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return TopRow > -1 && BottomRow > -1 && RowCount > 0 && ColumnCount > 0
                    && TopRow <= BottomRow && LeftColumn <= RightColumn;
            }
        }

        private int _topRow;
        private int _leftColumn;
        private int _rowCount;
        private int _columnCount;

        public CellRange(int row, int col)
        {
            _topRow = row;
            _leftColumn = col;
            _rowCount = row < 0 ? 0 : 1;
            _columnCount = col < 0 ? 0 : 1;
        }

        public CellRange(int row, int col, int rowCount, int columnCount)
        {
            _topRow = row;
            _leftColumn = col;
            _rowCount = row < 0 ? 0 : rowCount;
            _columnCount = col < 0 ? 0 : columnCount;
        }

        /// <summary>
        /// Sets the top row of this range.
        /// </summary>
        /// <param name="row"></param>
        public void SetTopRow(int row)
        {
            _topRow = row;
        }

        /// <summary>
        /// Sets the left column of this range.
        /// </summary>
        /// <param name="column"></param>
        public void SetLeftColumn(int column)
        {
            _leftColumn = column;
        }

        /// <summary>
        /// Sets the row count of this range.
        /// </summary>
        /// <param name="rowCount"></param>
        public void SetRowCount(int rowCount)
        {
            _rowCount = rowCount;
        }

        /// <summary>
        /// Sets the column count of this range.
        /// </summary>
        /// <param name="columnCount"></param>
        public void SetColumnCount(int columnCount)
        {
            _columnCount = columnCount;
        }

        /// <summary>
        /// Sets all values of this range.
        /// </summary>
        /// <param name="topRow"></param>
        /// <param name="leftColumn"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public void SetValues(int topRow, int leftColumn, int rowCount, int columnCount)
        {
            _topRow = topRow;
            _leftColumn = leftColumn;
            _rowCount = rowCount;
            _columnCount = columnCount;
        }

        /// <summary>
        /// Gets whether this range contains this column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool ContainsColumn(int column)
        {
            return column >= LeftColumn && column <= RightColumn;
        }

        /// <summary>
        /// Gets whether this range contains this row.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool ContainsRow(int row)
        {
            return row >= TopRow && row <= BottomRow;
        }

        /// <summary>
        /// Gets whether this range contains this cell.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool ContainsCell(int row, int column)
        {
            return ContainsColumn(column) && ContainsRow(row);
        }

        /// <summary>
        /// Gets whether this range contains the provided range.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool ContainsRange(CellRange range)
        {
            return TopRow <= range.TopRow && BottomRow >= range.BottomRow
                && LeftColumn <= range.LeftColumn && RightColumn >= range.RightColumn;
        }

        public bool Intersects(CellRange range)
        {
            return TopRow <= range.TopRow || BottomRow >= range.BottomRow
                || LeftColumn <= range.LeftColumn || RightColumn >= range.RightColumn;
        }

        public bool IntersectsWith(CellRange range)
        {
            return TopRow <= range.BottomRow && BottomRow >= range.TopRow &&
                   LeftColumn <= range.RightColumn && RightColumn >= range.LeftColumn;
        }

        public static CellRange Union(CellRange r1, CellRange r2)
        {
            if (r1 == default(CellRange)) return r2;
            if (r2 == default(CellRange)) return r1;

            int topRow = System.Math.Min(r1.TopRow, r2.TopRow);
            int leftColumn = System.Math.Min(r1.LeftColumn, r2.LeftColumn);
            int bottomRow = System.Math.Max(r1.BottomRow, r2.BottomRow);
            int rightColumn = System.Math.Max(r1.RightColumn, r2.RightColumn);

            return new CellRange(
                topRow,
                leftColumn,
                bottomRow - topRow + 1,
                rightColumn - leftColumn + 1);
        }

        public override string ToString()
        {
            return $"TopRow:{TopRow}, BottomRow:{BottomRow}, LeftColumn:{LeftColumn}, RightColumn:{RightColumn}";
        }

        public CellRange Clone()
        {
            return new CellRange(TopRow, LeftColumn, RowCount, ColumnCount);
        }

        public override bool Equals(object obj)
        {
            return obj is CellRange range && this == range;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + TopRow;
                hash = hash * 31 + LeftColumn;
                hash = hash * 31 + RowCount;
                hash = hash * 31 + ColumnCount;
                return hash;
            }
        }

        public static bool operator ==(CellRange left, CellRange right)
        {
            return left.TopRow == right.TopRow &&
                   left.LeftColumn == right.LeftColumn &&
                   left.RowCount == right.RowCount &&
                   left.ColumnCount == right.ColumnCount;
        }

        public static bool operator !=(CellRange left, CellRange right)
        {
            return !(left == right);
        }
    }
}
