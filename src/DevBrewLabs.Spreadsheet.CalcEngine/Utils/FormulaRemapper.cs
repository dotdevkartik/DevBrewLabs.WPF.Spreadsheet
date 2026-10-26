using DevBrewLabs.Spreadsheet.CalcEngine.Parsers;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Utils
{
    /// <summary>
    /// Utility for shifting and remapping formula coordinates during row/column insertions,
    /// deletions, and clipboard copy-paste / drag-fill operations.
    /// </summary>
    public static class FormulaRemapper
    {
        private static readonly Regex RefRegex = new Regex(
            @"(?<range>(('([^']+)'|([A-Za-z0-9_ ]+))!)?\$?[A-Za-z]+\$?[0-9]+:\$?[A-Za-z]+\$?[0-9]+)|(?<cell>(('([^']+)'|([A-Za-z0-9_ ]+))!)?\$?[A-Za-z]+\$?[0-9]+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Shifts relative cell and range references in a formula by the specified row and column offsets.
        /// Preserves absolute references ($). If an offset causes a coordinate to become negative, returns #REF!.
        /// </summary>
        public static string ShiftFormula(string formula, int rowDelta, int colDelta)
        {
            if (string.IsNullOrEmpty(formula) || (rowDelta == 0 && colDelta == 0))
                return formula;

            return RefRegex.Replace(formula, match =>
            {
                if (match.Groups["range"].Success)
                {
                    try
                    {
                        var rangeRef = new CellRangeRef(match.Value);
                        int startRow = rangeRef.Start.IsRowAbsolute ? rangeRef.Start.Row : rangeRef.Start.Row + rowDelta;
                        int startCol = rangeRef.Start.IsColumnAbsolute ? rangeRef.Start.Column : rangeRef.Start.Column + colDelta;
                        int endRow = rangeRef.End.IsRowAbsolute ? rangeRef.End.Row : rangeRef.End.Row + rowDelta;
                        int endCol = rangeRef.End.IsColumnAbsolute ? rangeRef.End.Column : rangeRef.End.Column + colDelta;

                        if (startRow < 0 || startCol < 0 || endRow < 0 || endCol < 0)
                            return "#REF!";

                        var newStart = new CellRef(startRow, startCol, rangeRef.Start.SheetName, rangeRef.Start.IsRowAbsolute, rangeRef.Start.IsColumnAbsolute);
                        var newEnd = new CellRef(endRow, endCol, rangeRef.End.SheetName, rangeRef.End.IsRowAbsolute, rangeRef.End.IsColumnAbsolute);
                        var newRange = new CellRangeRef(newStart, newEnd);
                        return newRange.Name;
                    }
                    catch
                    {
                        return match.Value;
                    }
                }
                else if (match.Groups["cell"].Success)
                {
                    try
                    {
                        var cellRef = new CellRef(match.Value);
                        int row = cellRef.IsRowAbsolute ? cellRef.Row : cellRef.Row + rowDelta;
                        int col = cellRef.IsColumnAbsolute ? cellRef.Column : cellRef.Column + colDelta;

                        if (row < 0 || col < 0)
                            return "#REF!";

                        var newCell = new CellRef(row, col, cellRef.SheetName, cellRef.IsRowAbsolute, cellRef.IsColumnAbsolute);
                        return newCell.Name;
                    }
                    catch
                    {
                        return match.Value;
                    }
                }

                return match.Value;
            });
        }

        /// <summary>
        /// Remaps formula coordinates when rows are inserted or removed in the specified sheet.
        /// </summary>
        public static string RemapOnRowChange(string formula, int index, int count, bool isInsert)
        {
            return RemapOnRowChange(formula, string.Empty, string.Empty, index, count, isInsert);
        }

        public static string RemapOnRowChange(string formula, string modifiedSheet, string currentSheet, int index, int count, bool isInsert)
        {
            if (string.IsNullOrEmpty(formula) || count <= 0)
                return formula;

            return RefRegex.Replace(formula, match =>
            {
                if (match.Groups["range"].Success)
                {
                    try
                    {
                        var rangeRef = new CellRangeRef(match.Value);
                        string sheet = string.IsNullOrEmpty(rangeRef.SheetName) ? currentSheet : rangeRef.SheetName;
                        if (!string.Equals(sheet, modifiedSheet, StringComparison.OrdinalIgnoreCase))
                            return match.Value;

                        int topRow = rangeRef.TopRow;
                        int bottomRow = rangeRef.BottomRow;

                        if (isInsert)
                        {
                            if (topRow >= index) topRow += count;
                            if (bottomRow >= index) bottomRow += count;
                        }
                        else
                        {
                            int removeEnd = index + count - 1;
                            if (topRow >= index && bottomRow <= removeEnd)
                                return "#REF!";

                            if (topRow > removeEnd) topRow -= count;
                            else if (topRow >= index) topRow = index;

                            if (bottomRow > removeEnd) bottomRow -= count;
                            else if (bottomRow >= index) bottomRow = Math.Max(topRow, index - 1);
                        }

                        var newStart = new CellRef(topRow, rangeRef.Start.Column, rangeRef.Start.SheetName, rangeRef.Start.IsRowAbsolute, rangeRef.Start.IsColumnAbsolute);
                        var newEnd = new CellRef(bottomRow, rangeRef.End.Column, rangeRef.End.SheetName, rangeRef.End.IsRowAbsolute, rangeRef.End.IsColumnAbsolute);
                        return new CellRangeRef(newStart, newEnd).Name;
                    }
                    catch
                    {
                        return match.Value;
                    }
                }
                else if (match.Groups["cell"].Success)
                {
                    try
                    {
                        var cellRef = new CellRef(match.Value);
                        string sheet = string.IsNullOrEmpty(cellRef.SheetName) ? currentSheet : cellRef.SheetName;
                        if (!string.Equals(sheet, modifiedSheet, StringComparison.OrdinalIgnoreCase))
                            return match.Value;

                        int row = cellRef.Row;
                        if (isInsert)
                        {
                            if (row >= index) row += count;
                        }
                        else
                        {
                            int removeEnd = index + count - 1;
                            if (row >= index && row <= removeEnd)
                                return "#REF!";

                            if (row > removeEnd) row -= count;
                        }

                        var newCell = new CellRef(row, cellRef.Column, cellRef.SheetName, cellRef.IsRowAbsolute, cellRef.IsColumnAbsolute);
                        return newCell.Name;
                    }
                    catch
                    {
                        return match.Value;
                    }
                }

                return match.Value;
            });
        }

        /// <summary>
        /// Remaps formula coordinates when columns are inserted or removed in the specified sheet.
        /// </summary>
        public static string RemapOnColumnChange(string formula, int index, int count, bool isInsert)
        {
            return RemapOnColumnChange(formula, string.Empty, string.Empty, index, count, isInsert);
        }

        public static string RemapOnColumnChange(string formula, string modifiedSheet, string currentSheet, int index, int count, bool isInsert)
        {
            if (string.IsNullOrEmpty(formula) || count <= 0)
                return formula;

            return RefRegex.Replace(formula, match =>
            {
                if (match.Groups["range"].Success)
                {
                    try
                    {
                        var rangeRef = new CellRangeRef(match.Value);
                        string sheet = string.IsNullOrEmpty(rangeRef.SheetName) ? currentSheet : rangeRef.SheetName;
                        if (!string.Equals(sheet, modifiedSheet, StringComparison.OrdinalIgnoreCase))
                            return match.Value;

                        int leftCol = rangeRef.LeftColumn;
                        int rightCol = rangeRef.RightColumn;

                        if (isInsert)
                        {
                            if (leftCol >= index) leftCol += count;
                            if (rightCol >= index) rightCol += count;
                        }
                        else
                        {
                            int removeEnd = index + count - 1;
                            if (leftCol >= index && rightCol <= removeEnd)
                                return "#REF!";

                            if (leftCol > removeEnd) leftCol -= count;
                            else if (leftCol >= index) leftCol = index;

                            if (rightCol > removeEnd) rightCol -= count;
                            else if (rightCol >= index) rightCol = Math.Max(leftCol, index - 1);
                        }

                        var newStart = new CellRef(rangeRef.Start.Row, leftCol, rangeRef.Start.SheetName, rangeRef.Start.IsRowAbsolute, rangeRef.Start.IsColumnAbsolute);
                        var newEnd = new CellRef(rangeRef.End.Row, rightCol, rangeRef.End.SheetName, rangeRef.End.IsRowAbsolute, rangeRef.End.IsColumnAbsolute);
                        return new CellRangeRef(newStart, newEnd).Name;
                    }
                    catch
                    {
                        return match.Value;
                    }
                }
                else if (match.Groups["cell"].Success)
                {
                    try
                    {
                        var cellRef = new CellRef(match.Value);
                        string sheet = string.IsNullOrEmpty(cellRef.SheetName) ? currentSheet : cellRef.SheetName;
                        if (!string.Equals(sheet, modifiedSheet, StringComparison.OrdinalIgnoreCase))
                            return match.Value;

                        int col = cellRef.Column;
                        if (isInsert)
                        {
                            if (col >= index) col += count;
                        }
                        else
                        {
                            int removeEnd = index + count - 1;
                            if (col >= index && col <= removeEnd)
                                return "#REF!";

                            if (col > removeEnd) col -= count;
                        }

                        var newCell = new CellRef(cellRef.Row, col, cellRef.SheetName, cellRef.IsRowAbsolute, cellRef.IsColumnAbsolute);
                        return newCell.Name;
                    }
                    catch
                    {
                        return match.Value;
                    }
                }

                return match.Value;
            });
        }
    }
}
