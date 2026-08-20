using System;
using System.Collections.Generic;
using System.Linq;

namespace DevBrewLabs.Spreadsheet.Filtering.Conditions
{
    /// <summary>
    /// Excel-style checkbox filter that matches if the cell value is in a set of allowed values.
    /// </summary>
    public class ValueListFilter : IFilterCondition
    {
        public HashSet<object> AllowedValues { get; set; } = new HashSet<object>();
        public bool IncludeBlanks { get; set; }

        public ValueListFilter() { }
        
        public ValueListFilter(IEnumerable<object> values)
        {
            if (values != null)
                AllowedValues = new HashSet<object>(values);
        }

        public bool Match(FilterContext context)
        {
            var value = context.Value;

            if (value == null || value == DBNull.Value || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                return IncludeBlanks;
            }

            if (AllowedValues == null || AllowedValues.Count == 0)
                return false;

            // Spreadsheet-appropriate value comparison
            foreach (var allowed in AllowedValues)
            {
                if (allowed == null) continue;
                
                if (allowed.Equals(value))
                    return true;

                if (allowed.ToString().Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
