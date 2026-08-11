using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Sorting;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for Sorting.xaml
    /// </summary>
    public partial class Sorting : UserControl
    {
        private const int TotalRows = 500;
        private const int TotalCols = 9;

        public Sorting()
        {
            InitializeComponent();
            PopulateSortableData();
        }

        private void PopulateSortableData()
        {
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            worksheet.RowCount = TotalRows + 1;
            worksheet.ColumnCount = TotalCols;

            string[] firstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
            string[] categories = { "Electronics", "Software", "Hardware", "Cloud Services", "Consulting", "Office Supplies" };
            string[] countries = { "United States", "Germany", "United Kingdom", "Canada", "Japan", "Australia", "France", "India" };
            string[] statuses = { "Completed", "Pending", "Processing", "Cancelled", "Shipped" };

            var rnd = new Random(123);
            var data = new object[TotalRows + 1, TotalCols];

            // Header labels in row 0
            string[] headers = { "Order ID", "Customer Name", "Category", "Country", "Units Sold", "Unit Price ($)", "Total Revenue ($)", "Rating (1-5)", "Status" };
            for (int col = 0; col < TotalCols; col++)
            {
                data[0, col] = headers[col];
            }

            for (int row = 1; row <= TotalRows; row++)
            {
                string name = $"{firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}";
                int units = rnd.Next(1, 200);
                double price = Math.Round(10.0 + rnd.NextDouble() * 490.0, 2);
                double total = Math.Round(units * price, 2);

                data[row, 0] = 1000 + row;
                data[row, 1] = name;
                data[row, 2] = categories[rnd.Next(categories.Length)];
                data[row, 3] = countries[rnd.Next(countries.Length)];
                data[row, 4] = units;
                data[row, 5] = price;
                data[row, 6] = total;
                data[row, 7] = rnd.Next(1, 6);
                data[row, 8] = statuses[rnd.Next(statuses.Length)];
            }

            worksheet.Load(data);

            // Style headers with Excel Green
            string headerStyleName = "SortHeaderStyle";
            if (worksheet.WorkBook.GetNamedStyle(headerStyleName) == null)
            {
                var style = new CellStyle
                {
                    BackColor = CellColor.FromArgb(255, 16, 124, 65), // #107C41 Excel Green
                    ForeColor = CellColor.White,
                    FontWeight = CellFontWeight.Bold,
                    HorizontalAlignment = CellHorizontalAlignment.Center
                };
                worksheet.WorkBook.AddNamedStyle(headerStyleName, style);
            }
            worksheet.Rows[0].StyleName = headerStyleName;

            // Set column widths
            worksheet.Columns[0].Width = 80;
            worksheet.Columns[1].Width = 150;
            worksheet.Columns[2].Width = 130;
            worksheet.Columns[3].Width = 130;
            worksheet.Columns[4].Width = 90;
            worksheet.Columns[5].Width = 100;
            worksheet.Columns[6].Width = 130;
            worksheet.Columns[7].Width = 90;
            worksheet.Columns[8].Width = 100;
        }

        private CellRange GetTargetSortRange()
        {
            var selection = spread.SheetViews.ActiveSheetView.Selection;
            if (selection != null && selection.RowCount > 1)
            {
                // Respect the exact selected range
                return selection;
            }

            // Default to sorting all data rows (excluding row 0 header)
            return new CellRange(1, 0, TotalRows, TotalCols);
        }

        private int GetTargetSortColumn()
        {
            var selection = spread.SheetViews.ActiveSheetView.Selection;
            if (selection != null)
            {
                // If a range is selected, sort by the first column of that range.
                // If a single cell is selected (which defaults to sorting the whole table), sort by that cell's column.
                return selection.LeftColumn;
            }
            return 0;
        }

        private void OnSortAscending(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            var range = GetTargetSortRange();
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(GetTargetSortColumn(), true));
            worksheet.SortRange(range, options);
        }

        private void OnSortDescending(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            var range = GetTargetSortRange();
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(GetTargetSortColumn(), false));
            worksheet.SortRange(range, options);
        }

        private void OnMultiLevelSort(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            var range = GetTargetSortRange();
            
            var options = new SortOptions();
            // Level 1: Category (Column 2), Ascending
            options.SortLevels.Add(new SortInfo(2, true));
            // Level 2: Rating (Column 7), Descending
            options.SortLevels.Add(new SortInfo(7, false));
            
            worksheet.SortRange(range, options);
        }

        private void OnCustomSort(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            var range = GetTargetSortRange();
            
            var options = new SortOptions();
            // Sort by string length ascending
            options.SortLevels.Add(new SortInfo(GetTargetSortColumn(), true) { CustomComparer = new StringLengthComparer() });
            
            worksheet.SortRange(range, options);
        }

        private class StringLengthComparer : ISortComparer
        {
            public int Compare(object x, object y)
            {
                string s1 = x?.ToString() ?? string.Empty;
                string s2 = y?.ToString() ?? string.Empty;
                
                int lenCompare = s1.Length.CompareTo(s2.Length);
                if (lenCompare == 0)
                {
                    return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);
                }
                return lenCompare;
            }
        }
    }
}



