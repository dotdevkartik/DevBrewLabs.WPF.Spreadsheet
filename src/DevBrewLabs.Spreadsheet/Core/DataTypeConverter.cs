using System;

namespace DevBrewLabs.Spreadsheet.Core
{
    internal class DataTypeConverter
    {
        public static object ConvertType(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (double.TryParse(value, out double doubleResult))
                return doubleResult;

            if (DateTime.TryParse(value, out DateTime date))
                return date;

            return value;
        }
    }
}
