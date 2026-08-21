using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.Filtering
{
    /// <summary>
    /// Logic operator for multiple conditions.
    /// </summary>
    public enum FilterLogic
    {
        And,
        Or
    }

    /// <summary>
    /// Represents the active filter state for one column.
    /// Internal use only; developers interact with AutoFilter.
    /// </summary>
    public sealed class ColumnFilter
    {
        public int ColumnIndex { get; }
        private readonly List<IFilterCondition> _conditions = new List<IFilterCondition>();

        public IList<IFilterCondition> Conditions => _conditions;
        public FilterLogic Logic { get; set; } = FilterLogic.Or;

        public bool IsFiltered => _conditions.Count > 0;

        public ColumnFilter(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        internal bool MatchRow(FilterContext context)
        {
            if (!IsFiltered) return true;

            if (Logic == FilterLogic.Or)
            {
                foreach (var condition in _conditions)
                {
                    if (condition.Match(context))
                        return true;
                }
                return false;
            }
            else
            {
                foreach (var condition in _conditions)
                {
                    if (!condition.Match(context))
                        return false;
                }
                return true;
            }
        }
    }
}
