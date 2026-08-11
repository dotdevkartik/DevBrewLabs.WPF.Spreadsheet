using DevBrewLabs.Spreadsheet;

using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet
{
    internal static class RenderingExtensions
    {
        internal static Rect ToCellCheckBoxRect(this Rect cellRect, Size checkBoxSize)
        {
            cellRect.X = cellRect.Left + cellRect.Width / 2 - checkBoxSize.Width / 2;
            cellRect.Y = cellRect.Top + cellRect.Height / 2 - checkBoxSize.Height / 2;
            cellRect.Size = checkBoxSize;
            return cellRect;
        }

        /// <summary>
        /// Gets the height of the rendered row region.
        /// </summary>
        /// <param name="sheetView"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        internal static double GetRowRenderedHeight(this SheetView sheetView, int row)
        {
            if (sheetView.ViewPort.ViewRange.ContainsRow(row))
            {
                var rowRect = sheetView.ViewPort.GetRowRect(row);

                if (rowRect.Bottom > sheetView.ViewPort.ActualBounds.Bottom)
                {
                    return sheetView.ViewPort.ActualBounds.Bottom - rowRect.Top;
                }

                if (rowRect.Top < sheetView.ViewPort.ActualBounds.Top)
                {
                    return rowRect.Bottom - sheetView.ViewPort.ActualBounds.Top;
                }

                var workSheet = sheetView.WorkSheet;
                var sheetRow = ((Rows)workSheet.Rows).GetItem(row);
                return sheetRow != null ? sheetRow.Height : workSheet.DefaultRowHeight;
            }

            return 0;
        }

        /// <summary>
        /// Gets the width of the rendered column region.
        /// </summary>
        /// <param name="sheetView"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        internal static double GetColumnRenderedWidth(this SheetView sheetView, int column)
        {
            if (sheetView.ViewPort.ViewRange.ContainsColumn(column))
            {
                var colRect = sheetView.ViewPort.GetColumnRect(column);

                if (colRect.Right > sheetView.ViewPort.ActualBounds.Right)
                {
                    return sheetView.ViewPort.ActualBounds.Right - colRect.Left;
                }

                if (colRect.Left < sheetView.ViewPort.ActualBounds.Left)
                {
                    return colRect.Right - sheetView.ViewPort.ActualBounds.Left;
                }

                var workSheet = sheetView.WorkSheet;
                var sheetColumn = ((Columns)workSheet.Columns).GetItem(column);
                return sheetColumn != null ? sheetColumn.Width : workSheet.DefaultColumnWidth;
            }

            return 0;
        }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static string GetColumnHeader(int index)
        {
            string str = "";
            char achar;
            int mod;
            while (true)
            {
                mod = (index % 26) + 65;
                index = index / 26;
                achar = (char)mod;
                str = achar + str;
                if (index > 0) index--;
                else if (index == 0) break;
            }
            return str;
        }

        internal static int GetColumnIndex(string address)
        {
            int[] digits = new int[address.Length];
            for (int i = 0; i < address.Length; ++i)
            {
                digits[i] = Convert.ToInt32(address[i]) - 64;
            }
            int mul = 1; int index = 0;
            for (int pos = digits.Length - 1; pos >= 0; --pos)
            {
                index += digits[pos] * mul;
                mul *= 26;
            }
            return index - 1;
        }
    }
}
