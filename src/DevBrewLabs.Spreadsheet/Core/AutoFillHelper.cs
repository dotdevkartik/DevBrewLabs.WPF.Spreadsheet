using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.CalcEngine.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DevBrewLabs.Spreadsheet.Core
{
    public enum AutoFillDirection
    {
        Down,
        Up,
        Right,
        Left
    }

    public static class AutoFillHelper
    {
        private static readonly string[] DaysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        private static readonly string[] DaysOfWeekShort = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        private static readonly string[] Months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        private static readonly string[] MonthsShort = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private static readonly Regex AlphaNumericRegex = new Regex(@"^(.*?)(\d+)$", RegexOptions.Compiled);

        public static bool TryGetFillDirection(CellRange source, CellRange target, out AutoFillDirection direction)
        {
            direction = AutoFillDirection.Down;
            if (!source.IsValid || !target.IsValid || source == target)
                return false;

            if (target.BottomRow > source.BottomRow)
            {
                direction = AutoFillDirection.Down;
                return true;
            }
            if (target.TopRow < source.TopRow)
            {
                direction = AutoFillDirection.Up;
                return true;
            }
            if (target.RightColumn > source.RightColumn)
            {
                direction = AutoFillDirection.Right;
                return true;
            }
            if (target.LeftColumn < source.LeftColumn)
            {
                direction = AutoFillDirection.Left;
                return true;
            }

            return false;
        }

        public static CellRange GetFillArea(CellRange source, CellRange target, AutoFillDirection direction)
        {
            switch (direction)
            {
                case AutoFillDirection.Down:
                    return new CellRange(source.BottomRow + 1, source.LeftColumn, target.BottomRow - source.BottomRow, source.ColumnCount);
                case AutoFillDirection.Up:
                    return new CellRange(target.TopRow, source.LeftColumn, source.TopRow - target.TopRow, source.ColumnCount);
                case AutoFillDirection.Right:
                    return new CellRange(source.TopRow, source.RightColumn + 1, source.RowCount, target.RightColumn - source.RightColumn);
                case AutoFillDirection.Left:
                    return new CellRange(source.TopRow, target.LeftColumn, source.RowCount, source.LeftColumn - target.LeftColumn);
                default:
                    return default;
            }
        }

        public static void AutoFill(IWorksheet ws, CellRange sourceRange, CellRange targetRange, bool isCtrlPressed = false)
        {
            if (ws == null || !sourceRange.IsValid || !targetRange.IsValid)
                return;

            if (!TryGetFillDirection(sourceRange, targetRange, out var direction))
                return;

            switch (direction)
            {
                case AutoFillDirection.Down:
                    FillVertical(ws, sourceRange, targetRange.BottomRow - sourceRange.BottomRow, isDown: true, isCtrlPressed);
                    break;

                case AutoFillDirection.Up:
                    FillVertical(ws, sourceRange, sourceRange.TopRow - targetRange.TopRow, isDown: false, isCtrlPressed);
                    break;

                case AutoFillDirection.Right:
                    FillHorizontal(ws, sourceRange, targetRange.RightColumn - sourceRange.RightColumn, isRight: true, isCtrlPressed);
                    break;

                case AutoFillDirection.Left:
                    FillHorizontal(ws, sourceRange, sourceRange.LeftColumn - targetRange.LeftColumn, isRight: false, isCtrlPressed);
                    break;
            }
        }

        private static void FillVertical(IWorksheet ws, CellRange source, int stepCount, bool isDown, bool isCtrlPressed)
        {
            int n = source.RowCount;
            for (int c = source.LeftColumn; c <= source.RightColumn; c++)
            {
                var formulas = new string[n];
                var values = new object[n];
                var styles = new IStyle[n];
                var styleNames = new string[n];

                for (int r = 0; r < n; r++)
                {
                    int row = source.TopRow + r;
                    formulas[r] = ws.GetFormula(row, c);
                    values[r] = ws.GetValue(row, c);
                    styles[r] = ws.GetStyle(row, c);
                    styleNames[r] = ws.GetStyleName(row, c);
                }

                for (int k = 1; k <= stepCount; k++)
                {
                    int targetRow = isDown ? (source.BottomRow + k) : (source.TopRow - k);
                    int srcIndex = isDown ? ((k - 1) % n) : ((n - (k % n)) % n);
                    int srcRow = source.TopRow + srcIndex;

                    // Copy style
                    if (styles[srcIndex] != null)
                        ws.SetStyle(targetRow, c, styles[srcIndex]);
                    if (!string.IsNullOrEmpty(styleNames[srcIndex]))
                        ws.SetStyleName(targetRow, c, styleNames[srcIndex]);

                    // Formula vs Value
                    if (formulas[srcIndex] != null)
                    {
                        string shiftedFormula = FormulaRemapper.ShiftFormula(formulas[srcIndex], targetRow - srcRow, 0);
                        ws.SetFormula(targetRow, c, shiftedFormula);
                    }
                    else
                    {
                        ws.SetFormula(targetRow, c, null);
                        object val = ComputeNextValue(values, k, isDown ? 1 : -1, isCtrlPressed);
                        ws.SetValue(targetRow, c, val);
                    }
                }
            }
        }

        private static void FillHorizontal(IWorksheet ws, CellRange source, int stepCount, bool isRight, bool isCtrlPressed)
        {
            int n = source.ColumnCount;
            for (int r = source.TopRow; r <= source.BottomRow; r++)
            {
                var formulas = new string[n];
                var values = new object[n];
                var styles = new IStyle[n];
                var styleNames = new string[n];

                for (int c = 0; c < n; c++)
                {
                    int col = source.LeftColumn + c;
                    formulas[c] = ws.GetFormula(r, col);
                    values[c] = ws.GetValue(r, col);
                    styles[c] = ws.GetStyle(r, col);
                    styleNames[c] = ws.GetStyleName(r, col);
                }

                for (int k = 1; k <= stepCount; k++)
                {
                    int targetCol = isRight ? (source.RightColumn + k) : (source.LeftColumn - k);
                    int srcIndex = isRight ? ((k - 1) % n) : ((n - (k % n)) % n);
                    int srcCol = source.LeftColumn + srcIndex;

                    // Copy style
                    if (styles[srcIndex] != null)
                        ws.SetStyle(r, targetCol, styles[srcIndex]);
                    if (!string.IsNullOrEmpty(styleNames[srcIndex]))
                        ws.SetStyleName(r, targetCol, styleNames[srcIndex]);

                    // Formula vs Value
                    if (formulas[srcIndex] != null)
                    {
                        string shiftedFormula = FormulaRemapper.ShiftFormula(formulas[srcIndex], 0, targetCol - srcCol);
                        ws.SetFormula(r, targetCol, shiftedFormula);
                    }
                    else
                    {
                        ws.SetFormula(r, targetCol, null);
                        object val = ComputeNextValue(values, k, isRight ? 1 : -1, isCtrlPressed);
                        ws.SetValue(r, targetCol, val);
                    }
                }
            }
        }

        private static object ComputeNextValue(object[] sourceValues, int k, int sign, bool isCtrlPressed)
        {
            int n = sourceValues.Length;
            if (n == 0) return null;

            if (n == 1)
            {
                object v0 = sourceValues[0];
                if (v0 == null) return null;

                // 1. Numeric
                if (TryGetDouble(v0, out double num))
                {
                    if (isCtrlPressed)
                    {
                        double nextNum = num + sign * k;
                        return IsIntegerType(v0) ? (object)(long)Math.Round(nextNum) : nextNum;
                    }
                    return v0; // Single number drag without Ctrl copies
                }

                // 2. DateTime
                if (TryGetDateTime(v0, out DateTime dt))
                {
                    if (isCtrlPressed)
                        return dt; // Ctrl copies date
                    return dt.AddDays(sign * k); // Without Ctrl increments date
                }

                // 3. Lists (Days of week, Months)
                string strVal = v0.ToString();
                if (TryMatchList(strVal, DaysOfWeek, out int dayIdx))
                {
                    if (isCtrlPressed) return strVal;
                    return DaysOfWeek[Mod(dayIdx + sign * k, DaysOfWeek.Length)];
                }
                if (TryMatchList(strVal, DaysOfWeekShort, out int daySIdx))
                {
                    if (isCtrlPressed) return strVal;
                    return DaysOfWeekShort[Mod(daySIdx + sign * k, DaysOfWeekShort.Length)];
                }
                if (TryMatchList(strVal, Months, out int mIdx))
                {
                    if (isCtrlPressed) return strVal;
                    return Months[Mod(mIdx + sign * k, Months.Length)];
                }
                if (TryMatchList(strVal, MonthsShort, out int mSIdx))
                {
                    if (isCtrlPressed) return strVal;
                    return MonthsShort[Mod(mSIdx + sign * k, MonthsShort.Length)];
                }

                // 4. AlphaNumeric (Item 1 -> Item 2)
                var match = AlphaNumericRegex.Match(strVal);
                if (match.Success)
                {
                    if (isCtrlPressed) return strVal;
                    string prefix = match.Groups[1].Value;
                    string digits = match.Groups[2].Value;
                    if (long.TryParse(digits, out long curNum))
                    {
                        long next = curNum + sign * k;
                        return prefix + next.ToString().PadLeft(digits.Length, '0');
                    }
                }

                return v0;
            }
            else
            {
                // Multi-cell sequence (n >= 2)
                // 1. All numeric
                bool allNumeric = true;
                double[] nums = new double[n];
                bool allIntegers = true;
                for (int i = 0; i < n; i++)
                {
                    if (sourceValues[i] != null && TryGetDouble(sourceValues[i], out nums[i]))
                    {
                        if (!IsIntegerType(sourceValues[i])) allIntegers = false;
                    }
                    else
                    {
                        allNumeric = false;
                        break;
                    }
                }

                if (allNumeric)
                {
                    if (isCtrlPressed)
                    {
                        int idx = sign > 0 ? ((k - 1) % n) : ((n - (k % n)) % n);
                        return sourceValues[idx];
                    }

                    double step = (nums[n - 1] - nums[0]) / (n - 1);
                    double nextVal = sign > 0 ? (nums[n - 1] + k * step) : (nums[0] - k * step);
                    if (allIntegers && (step % 1 == 0))
                    {
                        return (long)Math.Round(nextVal);
                    }
                    return nextVal;
                }

                // 2. All DateTime
                bool allDates = true;
                DateTime[] dates = new DateTime[n];
                for (int i = 0; i < n; i++)
                {
                    if (sourceValues[i] != null && TryGetDateTime(sourceValues[i], out dates[i]))
                    {
                    }
                    else
                    {
                        allDates = false;
                        break;
                    }
                }

                if (allDates)
                {
                    if (isCtrlPressed)
                    {
                        int idx = sign > 0 ? ((k - 1) % n) : ((n - (k % n)) % n);
                        return sourceValues[idx];
                    }

                    long stepTicks = (dates[n - 1].Ticks - dates[0].Ticks) / (n - 1);
                    long nextTicks = sign > 0 ? (dates[n - 1].Ticks + k * stepTicks) : (dates[0].Ticks - k * stepTicks);
                    return new DateTime(nextTicks);
                }

                // 3. Repeating Pattern / Default
                int cycleIdx = sign > 0 ? ((k - 1) % n) : ((n - (k % n)) % n);
                return sourceValues[cycleIdx];
            }
        }

        private static bool TryGetDouble(object val, out double result)
        {
            if (val is double d) { result = d; return true; }
            if (val is float f) { result = f; return true; }
            if (val is int i) { result = i; return true; }
            if (val is long l) { result = l; return true; }
            if (val is short s) { result = s; return true; }
            if (val is byte b) { result = b; return true; }
            if (val is decimal dec) { result = (double)dec; return true; }
            if (val is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                result = parsed;
                return true;
            }
            result = 0;
            return false;
        }

        private static bool IsIntegerType(object val)
        {
            return val is int || val is long || val is short || val is byte;
        }

        private static bool TryGetDateTime(object val, out DateTime result)
        {
            if (val is DateTime dt)
            {
                result = dt;
                return true;
            }
            if (val is string str && DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                result = parsed;
                return true;
            }
            result = default;
            return false;
        }

        private static bool TryMatchList(string val, string[] list, out int index)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (string.Equals(val, list[i], StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
