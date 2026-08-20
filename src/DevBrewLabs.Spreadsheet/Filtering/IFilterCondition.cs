using System;

namespace DevBrewLabs.Spreadsheet.Filtering
{
    /// <summary>
    /// Base interface for all filter conditions. Implement this to create custom filters.
    /// </summary>
    public interface IFilterCondition
    {
        /// <summary>
        /// Evaluates whether the given cell context passes this filter.
        /// </summary>
        bool Match(FilterContext context);
    }
}
