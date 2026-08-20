using System;

namespace DevBrewLabs.Spreadsheet.Filtering.Conditions
{
    public enum DateFilterOperator
    {
        Equals,
        Before,
        After,
        Between,
        Today,
        Tomorrow,
        Yesterday,
        ThisWeek,
        ThisMonth,
        ThisYear
    }

    public class DateFilter : IFilterCondition
    {
        public DateFilterOperator Operator { get; }
        public DateTime? Date1 { get; }
        public DateTime? Date2 { get; }
        
        /// <summary>
        /// Reference date captured at filter creation time for deterministic relative matching (e.g. "Today").
        /// </summary>
        public DateTime ReferenceDate { get; }

        public DateFilter(DateFilterOperator op, DateTime? date1 = null, DateTime? date2 = null, DateTime? referenceDate = null)
        {
            Operator = op;
            Date1 = date1;
            Date2 = date2;
            ReferenceDate = referenceDate ?? DateTime.Today;
        }

        public bool Match(FilterContext context)
        {
            if (context.Value == null) return false;
            
            if (!DateTime.TryParse(context.Value.ToString(), out DateTime cellValue))
                return false;

            cellValue = cellValue.Date; // Compare just dates usually

            switch (Operator)
            {
                case DateFilterOperator.Equals:
                    return Date1.HasValue && cellValue == Date1.Value.Date;
                case DateFilterOperator.Before:
                    return Date1.HasValue && cellValue < Date1.Value.Date;
                case DateFilterOperator.After:
                    return Date1.HasValue && cellValue > Date1.Value.Date;
                case DateFilterOperator.Between:
                    return Date1.HasValue && Date2.HasValue && cellValue >= Date1.Value.Date && cellValue <= Date2.Value.Date;
                case DateFilterOperator.Today:
                    return cellValue == ReferenceDate;
                case DateFilterOperator.Tomorrow:
                    return cellValue == ReferenceDate.AddDays(1);
                case DateFilterOperator.Yesterday:
                    return cellValue == ReferenceDate.AddDays(-1);
                case DateFilterOperator.ThisWeek:
                    int diff = (7 + (cellValue.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime startOfWeek = cellValue.AddDays(-1 * diff).Date;
                    
                    int refDiff = (7 + (ReferenceDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime refStartOfWeek = ReferenceDate.AddDays(-1 * refDiff).Date;
                    
                    return startOfWeek == refStartOfWeek;
                case DateFilterOperator.ThisMonth:
                    return cellValue.Month == ReferenceDate.Month && cellValue.Year == ReferenceDate.Year;
                case DateFilterOperator.ThisYear:
                    return cellValue.Year == ReferenceDate.Year;
                default:
                    return true;
            }
        }
    }
}
