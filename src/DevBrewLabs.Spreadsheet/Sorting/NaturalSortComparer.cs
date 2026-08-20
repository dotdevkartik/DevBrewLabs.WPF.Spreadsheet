using System;

namespace DevBrewLabs.Spreadsheet.Sorting
{
    /// <summary>
    /// Natural sort comparer adhering to Excel sort rules:
    /// 1. Blank/null cells always sort to the bottom.
    /// 2. Data type hierarchy: Numbers &lt; Text &lt; Booleans.
    /// 3. Natural alphanumeric string comparison in pure C#.
    /// </summary>
    internal class NaturalSortComparer : ISortComparer
    {
        private readonly bool _matchCase;

        public NaturalSortComparer(bool matchCase = false)
        {
            _matchCase = matchCase;
        }

        public int Compare(object x, object y)
        {
            bool isNullX = IsBlank(x);
            bool isNullY = IsBlank(y);

            if (isNullX && isNullY)
                return 0;

            // Excel rule: Blank cells ALWAYS sort to the bottom in both Ascending & Descending
            if (isNullX)
                return 1;
            if (isNullY)
                return -1;

            int typeOrderX = GetDataTypeOrder(x);
            int typeOrderY = GetDataTypeOrder(y);

            if (typeOrderX != typeOrderY)
            {
                return typeOrderX.CompareTo(typeOrderY);
            }

            return CompareSameType(x, y);
        }

        public static bool IsBlank(object val)
        {
            if (val == null || val == DBNull.Value)
                return true;

            if (val is string str && string.IsNullOrWhiteSpace(str))
                return true;

            return false;
        }

        private static int GetDataTypeOrder(object val)
        {
            if (IsNumeric(val))
                return 1;
            if (val is bool)
                return 3;

            return 2; // Text / other objects
        }

        private static bool IsNumeric(object val)
        {
            return val is sbyte || val is byte || val is short || val is ushort ||
                   val is int || val is uint || val is long || val is ulong ||
                   val is float || val is double || val is decimal;
        }

        private int CompareSameType(object x, object y)
        {
            if (IsNumeric(x) && IsNumeric(y))
            {
                double d1 = Convert.ToDouble(x);
                double d2 = Convert.ToDouble(y);
                return d1.CompareTo(d2);
            }

            if (x is bool b1 && y is bool b2)
            {
                return b1.CompareTo(b2);
            }

            string s1 = x.ToString();
            string s2 = y.ToString();

            return NaturalCompare(s1, s2, _matchCase);
        }

        private static int NaturalCompare(string s1, string s2, bool matchCase)
        {
            if (s1 == null && s2 == null) return 0;
            if (s1 == null) return -1;
            if (s2 == null) return 1;

            int i1 = 0, i2 = 0;
            while (i1 < s1.Length && i2 < s2.Length)
            {
                char c1 = s1[i1];
                char c2 = s2[i2];

                bool isDigit1 = char.IsDigit(c1);
                bool isDigit2 = char.IsDigit(c2);

                if (isDigit1 && isDigit2)
                {
                    int start1 = i1;
                    while (i1 < s1.Length && char.IsDigit(s1[i1])) i1++;
                    int len1 = i1 - start1;

                    int start2 = i2;
                    while (i2 < s2.Length && char.IsDigit(s2[i2])) i2++;
                    int len2 = i2 - start2;

                    // Skip leading zeros for value comparison
                    int zero1 = 0;
                    while (zero1 < len1 - 1 && s1[start1 + zero1] == '0') zero1++;
                    
                    int zero2 = 0;
                    while (zero2 < len2 - 1 && s2[start2 + zero2] == '0') zero2++;

                    int actualLen1 = len1 - zero1;
                    int actualLen2 = len2 - zero2;

                    if (actualLen1 != actualLen2)
                        return actualLen1.CompareTo(actualLen2);

                    for (int j = 0; j < actualLen1; j++)
                    {
                        char d1 = s1[start1 + zero1 + j];
                        char d2 = s2[start2 + zero2 + j];
                        if (d1 != d2)
                            return d1.CompareTo(d2);
                    }
                    
                    // If values are equal, compare lengths including zeros to stabilize sort
                    if (len1 != len2)
                        return len1.CompareTo(len2);
                }
                else
                {
                    int cmp = matchCase ? c1.CompareTo(c2) : char.ToLowerInvariant(c1).CompareTo(char.ToLowerInvariant(c2));

                    if (cmp != 0)
                        return cmp;

                    i1++;
                    i2++;
                }
            }

            return s1.Length.CompareTo(s2.Length);
        }
    }
}
