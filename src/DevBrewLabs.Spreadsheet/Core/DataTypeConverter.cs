using System;

namespace DevBrewLabs.Spreadsheet.Core
{
    internal class DataTypeConverter
    {
        public static object ConvertType(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (int.TryParse(value, out int integer))
                return integer;

            if (double.TryParse(value, out double doubleResult))
                return doubleResult;

            if (decimal.TryParse(value, out decimal decimalResult))
                return decimalResult;

            if (DateTime.TryParse(value, out DateTime date))
                return date;

            return value;
        }
    }
}
