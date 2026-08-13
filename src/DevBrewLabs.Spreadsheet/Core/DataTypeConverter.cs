using System;

namespace DevBrewLabs.Spreadsheet.Core
{
    internal class DataTypeConverter
    {
        public static object ConvertType(object value)
        {
            if (value == null)
                return null;

            switch (value)
            {
                case string v:
                    return v;
                case byte v:
                    return (double)v;
                case sbyte v:
                    return (double)v;
                case short v:
                    return (double)v;
                case ushort v:
                    return (double)v;
                case int v:
                    return (double)v;
                case uint v:
                    return (double)v;
                case long v:
                    return (double)v;
                case ulong v:
                    return (double)v;
                case float v:
                    return (double)v;
                case decimal v:
                    return (double)v;
                default:
                    return value;
            }
        }

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
