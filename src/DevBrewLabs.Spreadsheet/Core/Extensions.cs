using System;

namespace DevBrewLabs.Spreadsheet
{
    internal static class Extensions
    {
        public static bool IsNumber(this object value)
        {
            return value is int || value is double || value is float || value is decimal || value is long || value is short;
        }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetColumnHeader(int index)
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

        public static int GetColumnIndex(string address)
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
