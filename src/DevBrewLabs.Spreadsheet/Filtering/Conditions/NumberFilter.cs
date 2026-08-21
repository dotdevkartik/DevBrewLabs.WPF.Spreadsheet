using System;

namespace DevBrewLabs.Spreadsheet.Filtering.Conditions
{
    public enum NumberFilterOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEquals,
        LessThan,
        LessThanOrEquals,
        Between
    }

    public class NumberFilter : IFilterCondition
    {
        public NumberFilterOperator Operator { get; }
        public decimal Value1 { get; }
        public decimal Value2 { get; }

        public NumberFilter(NumberFilterOperator op, decimal value1, decimal value2 = 0)
        {
            Operator = op;
            Value1 = value1;
            Value2 = value2;
        }

        public bool Match(FilterContext context)
        {
            if (context.Value == null) return false;
            
            if (!decimal.TryParse(context.Value.ToString(), out decimal cellValue))
                return false;

            switch (Operator)
            {
                case NumberFilterOperator.Equals:
                    return cellValue == Value1;
                case NumberFilterOperator.NotEquals:
                    return cellValue != Value1;
                case NumberFilterOperator.GreaterThan:
                    return cellValue > Value1;
                case NumberFilterOperator.GreaterThanOrEquals:
                    return cellValue >= Value1;
                case NumberFilterOperator.LessThan:
                    return cellValue < Value1;
                case NumberFilterOperator.LessThanOrEquals:
                    return cellValue <= Value1;
                case NumberFilterOperator.Between:
                    return cellValue >= Value1 && cellValue <= Value2;
                default:
                    return true;
            }
        }
    }
}
