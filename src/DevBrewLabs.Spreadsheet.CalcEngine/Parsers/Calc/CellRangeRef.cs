using System;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Parsers
{
    internal class CellRangeRef : IEquatable<CellRangeRef>
    {
        public CellRef Start { get; }
        public CellRef End { get; }

        public string Name
        {
            get
            {
                if (string.Equals(Start.SheetName, End.SheetName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(End.SheetName))
                {
                    string endRefStr = (End.IsColumnAbsolute ? "$" : "") + CellRef.GetColumnLetterFromNumber(End.Column + 1) + (End.IsRowAbsolute ? "$" : "") + (End.Row + 1);
                    return Start.Name + ":" + endRefStr;
                }
                return Start.Name + ":" + End.Name;
            }
        }

        public string SheetName => Start.SheetName;

        public int TopRow { get; private set; }
        public int BottomRow { get; private set; }
        public int LeftColumn { get; private set; }
        public int RightColumn { get; private set; }
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public CellRangeRef(string cellRangeRef)
        {
            if (string.IsNullOrEmpty(cellRangeRef))
                throw new ArgumentNullException(nameof(cellRangeRef));

            var res = cellRangeRef.Split(':');
            if (res.Length != 2)
                throw new ArgumentException($"Invalid cell range reference format: '{cellRangeRef}'", nameof(cellRangeRef));

            Start = new CellRef(res[0]);
            if (!string.IsNullOrEmpty(Start.SheetName) && !res[1].Contains("!"))
            {
                string endPart = res[1].Trim();
                if (Start.SheetName.Contains(" ") || Start.SheetName.Contains("!"))
                    End = new CellRef($"'{Start.SheetName}'!{endPart}");
                else
                    End = new CellRef($"{Start.SheetName}!{endPart}");
            }
            else
            {
                End = new CellRef(res[1]);
            }
            FillRowColInfo();
        }

        public CellRangeRef(int startRow, int startColumn, int endRow, int endColumn, string sheetName = "",
            bool startRowAbs = false, bool startColAbs = false, bool endRowAbs = false, bool endColAbs = false)
        {
            Start = new CellRef(startRow, startColumn, sheetName, startRowAbs, startColAbs);
            End = new CellRef(endRow, endColumn, sheetName, endRowAbs, endColAbs);
            FillRowColInfo();
        }

        public CellRangeRef(CellRef start, CellRef end)
        {
            Start = start ?? throw new ArgumentNullException(nameof(start));
            End = end ?? throw new ArgumentNullException(nameof(end));
            FillRowColInfo();
        }

        private void FillRowColInfo()
        {
            TopRow = Math.Min(Start.Row, End.Row);
            BottomRow = Math.Max(Start.Row, End.Row);
            LeftColumn = Math.Min(Start.Column, End.Column);
            RightColumn = Math.Max(Start.Column, End.Column);
            RowCount = BottomRow - TopRow + 1;
            ColumnCount = RightColumn - LeftColumn + 1;
        }

        #region Equals Comparison

        public override bool Equals(object obj) => Equals(obj as CellRangeRef);

        public bool Equals(CellRangeRef other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Start, other.Start) && Equals(End, other.End);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (Start != null ? Start.GetHashCode() : 0);
                hash = hash * 31 + (End != null ? End.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(CellRangeRef lhs, CellRangeRef rhs)
        {
            if (lhs is null) return rhs is null;
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CellRangeRef lhs, CellRangeRef rhs) => !(lhs == rhs);

        #endregion
    }
}
