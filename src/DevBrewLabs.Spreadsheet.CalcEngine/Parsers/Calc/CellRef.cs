using System;
using System.Text;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Parsers
{
    internal class CellRef : IEquatable<CellRef>
    {
        public int Column { get; }
        public int Row { get; }
        public string SheetName { get; }
        public bool IsRowAbsolute { get; }
        public bool IsColumnAbsolute { get; }

        public string Name
        {
            get
            {
                string colStr = (IsColumnAbsolute ? "$" : "") + GetColumnLetterFromNumber(Column + 1);
                string rowStr = (IsRowAbsolute ? "$" : "") + (Row + 1).ToString();
                string refStr = colStr + rowStr;

                if (string.IsNullOrEmpty(SheetName))
                    return refStr;

                if (SheetName.Contains(" ") || SheetName.Contains("!"))
                    return $"'{SheetName}'!{refStr}";

                return $"{SheetName}!{refStr}";
            }
        }

        public CellRef(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            string cellRef = name.Trim();
            string sheetName = string.Empty;

            int exclamationIdx = cellRef.LastIndexOf('!');
            if (exclamationIdx >= 0)
            {
                sheetName = cellRef.Substring(0, exclamationIdx).Trim('\'');
                cellRef = cellRef.Substring(exclamationIdx + 1).Trim();
            }

            int digitIdx = -1;
            for (int i = 0; i < cellRef.Length; i++)
            {
                if (char.IsDigit(cellRef[i]))
                {
                    digitIdx = i;
                    break;
                }
            }

            if (digitIdx <= 0 || !int.TryParse(cellRef.Substring(digitIdx), out int rowNumber))
            {
                throw new ArgumentException($"Invalid cell reference format: '{name}'", nameof(name));
            }

            bool isRowAbs = false;
            if (digitIdx > 0 && cellRef[digitIdx - 1] == '$')
            {
                isRowAbs = true;
            }

            int colEndIdx = isRowAbs ? digitIdx - 1 : digitIdx;
            bool isColAbs = false;
            int colStartIdx = 0;
            if (cellRef.Length > 0 && cellRef[0] == '$')
            {
                isColAbs = true;
                colStartIdx = 1;
            }

            string colLetter = cellRef.Substring(colStartIdx, colEndIdx - colStartIdx);
            Column = GetColumnNumberFromLetter(colLetter) - 1;
            Row = rowNumber - 1;
            SheetName = sheetName;
            IsRowAbsolute = isRowAbs;
            IsColumnAbsolute = isColAbs;
        }

        public CellRef(int row, int col, string sheetName = "", bool isRowAbsolute = false, bool isColumnAbsolute = false)
        {
            Row = row;
            Column = col;
            SheetName = sheetName ?? string.Empty;
            IsRowAbsolute = isRowAbsolute;
            IsColumnAbsolute = isColumnAbsolute;
        }

        public static int GetColumnNumberFromLetter(string letter)
        {
            if (string.IsNullOrEmpty(letter))
                return 0;

            int col = 0;
            foreach (char c in letter)
            {
                if (c >= 'A' && c <= 'Z')
                    col = col * 26 + (c - 'A' + 1);
                else if (c >= 'a' && c <= 'z')
                    col = col * 26 + (c - 'a' + 1);
            }
            return col;
        }

        public static string GetColumnLetterFromNumber(int num)
        {
            if (num <= 0)
                return string.Empty;

            var result = new StringBuilder();
            while (num > 0)
            {
                int remainder = (num - 1) % 26;
                result.Insert(0, (char)('A' + remainder));
                num = (num - 1) / 26;
            }
            return result.ToString();
        }

        #region Equals Comparison

        public override bool Equals(object obj) => Equals(obj as CellRef);

        public bool Equals(CellRef other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Row == other.Row &&
                   Column == other.Column &&
                   string.Equals(SheetName, other.SheetName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Row;
                hash = hash * 31 + Column;
                hash = hash * 31 + (SheetName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(SheetName) : 0);
                return hash;
            }
        }

        public static bool operator ==(CellRef lhs, CellRef rhs)
        {
            if (lhs is null) return rhs is null;
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CellRef lhs, CellRef rhs) => !(lhs == rhs);

        #endregion
    }
}
