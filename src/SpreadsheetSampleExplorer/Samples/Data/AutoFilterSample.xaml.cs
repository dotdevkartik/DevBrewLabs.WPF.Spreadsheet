using System;
using System.Windows;
using System.Windows.Controls;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;

namespace SpreadsheetSampleExplorer.Samples
{
    public partial class AutoFilterSample : UserControl
    {
        public AutoFilterSample()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var sheet = spreadGrid.WorkBook.WorkSheets.GetSheet(0);
            
            // Set Headers
            sheet.SetValue(0, 0, "Employee ID");
            sheet.SetValue(0, 1, "Name");
            sheet.SetValue(0, 2, "Department");
            sheet.SetValue(0, 3, "Hire Date");
            sheet.SetValue(0, 4, "Salary");
            
            for (int c = 0; c < 5; c++)
            {
                sheet.Columns[c].Width = 120;
            }
            sheet.Columns[1].Width = 160;

            // Generate Sample Data
            var depts = new[] { "Engineering", "Sales", "HR", "Marketing", "Engineering", "Sales", "HR", "Sales" };
            var names = new[] { "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Heidi" };
            var dates = new[] { new DateTime(2020, 1, 15), new DateTime(2019, 3, 20), new DateTime(2021, 6, 10), new DateTime(2022, 11, 5), new DateTime(2018, 7, 22), new DateTime(2023, 2, 1), new DateTime(2020, 9, 30), new DateTime(2019, 12, 12) };
            var salaries = new[] { 95000, 72000, 68000, 75000, 110000, 65000, 71000, 80000 };

            for (int i = 0; i < names.Length; i++)
            {
                sheet.SetValue(i + 1, 0, i + 1001);
                sheet.SetValue(i + 1, 1, names[i]);
                sheet.SetValue(i + 1, 2, depts[i]);
                sheet.SetValue(i + 1, 3, dates[i].ToString("yyyy-MM-dd"));
                sheet.SetValue(i + 1, 4, salaries[i]);
            }

            // Enable AutoFilter over the range (0,0) to (names.Length, 4)
            sheet.AutoFilter.SetRange(new CellRange(0, 0, names.Length + 1, 5));
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            var sheet = spreadGrid.WorkBook.WorkSheets.GetSheet(0);
            if (sheet.AutoFilter != null)
            {
                sheet.AutoFilter.ClearAll();
            }
        }
    }
}
