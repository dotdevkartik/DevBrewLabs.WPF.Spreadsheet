using DevBrewLabs.Evalis;
using System;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Utils
{
    internal static class FormulaEngineConverter
    {
        public static CalcValue ConvertToCalcValue(IEvaluationResult result)
        {
            if (result == null)
                return new CalcValue { Kind = CalcValueKind.Number, Value = 0 };

            if (result.Error.HasValue)
            {
                return new CalcValue
                {
                    Kind = CalcValueKind.Error,
                    Value = result.Error.Value
                };
            }

            return ConvertToCalcValue(result.Value);
        }

        public static CalcValue ConvertToCalcValue(object value)
        {
            if (value is IEvaluationResult result)
                return ConvertToCalcValue(result);

            if (value == null)
                return new CalcValue { Kind = CalcValueKind.Number, Value = 0 };

            if (value is CalcValue calcValue)
                return calcValue;

            if (value is object[] array1D)
            {
                var transformedValue = new CalcValue[1, array1D.Length];
                for (var idx = 0; idx < array1D.Length; idx++)
                {
                    transformedValue[0, idx] = ConvertToCalcValue(array1D[idx]);
                }
                return new CalcValue
                {
                    Kind = CalcValueKind.Array,
                    Value = transformedValue
                };
            }

            if (value is object[,] array2D)
            {
                var transformedValue = new CalcValue[array2D.GetLength(0), array2D.GetLength(1)];
                for (var r = 0; r < array2D.GetLength(0); r++)
                {
                    for (var c = 0; c < array2D.GetLength(1); c++)
                    {
                        transformedValue[r, c] = ConvertToCalcValue(array2D[r, c]);
                    }
                }
                return new CalcValue
                {
                    Kind = CalcValueKind.Array,
                    Value = transformedValue
                };
            }

            Type type = value.GetType();
            CalcValueKind kind;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return new CalcValue { Kind = CalcValueKind.Number, Value = 0 };
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    kind = CalcValueKind.Number;
                    break;

                case TypeCode.DateTime:
                    kind = CalcValueKind.Date;
                    break;
                case TypeCode.Boolean:
                    kind = CalcValueKind.Bool;
                    break;
                case TypeCode.Char:
                case TypeCode.String:
                    kind = CalcValueKind.String;
                    break;
                default:
                    return new CalcValue { Kind = CalcValueKind.Error, Value = Error.Value("Unknown type") };
            }

            return new CalcValue { Kind = kind, Value = value };
        }
    }
}
