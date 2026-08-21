using System;

namespace DevBrewLabs.Spreadsheet.Filtering.Conditions
{
    public enum TextFilterOperator
    {
        Equals,
        NotEquals,
        BeginsWith,
        EndsWith,
        Contains,
        NotContains
    }

    public class TextFilter : IFilterCondition
    {
        public TextFilterOperator Operator { get; }
        public string Text { get; }

        public TextFilter(TextFilterOperator op, string text)
        {
            Operator = op;
            Text = text ?? "";
        }

        public bool Match(FilterContext context)
        {
            var cellValue = context.Value?.ToString() ?? "";

            switch (Operator)
            {
                case TextFilterOperator.Equals:
                    return string.Equals(cellValue, Text, StringComparison.InvariantCultureIgnoreCase);
                case TextFilterOperator.NotEquals:
                    return !string.Equals(cellValue, Text, StringComparison.InvariantCultureIgnoreCase);
                case TextFilterOperator.BeginsWith:
                    return cellValue.StartsWith(Text, StringComparison.InvariantCultureIgnoreCase);
                case TextFilterOperator.EndsWith:
                    return cellValue.EndsWith(Text, StringComparison.InvariantCultureIgnoreCase);
                case TextFilterOperator.Contains:
                    return cellValue.IndexOf(Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
                case TextFilterOperator.NotContains:
                    return cellValue.IndexOf(Text, StringComparison.InvariantCultureIgnoreCase) < 0;
                default:
                    return true;
            }
        }
    }
}
