using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Sorting;
using DevBrewLabs.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SpreadsheetSampleExplorer.Data;

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
            Loaded += (s, e) => LoadUnboundData();
        }

        private void OnModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            switch (modeComboBox.SelectedIndex)
            {
                case 0:
                    LoadUnboundData();
                    break;
                case 1:
                    LoadBoundData();
                    break;
                case 2:
                    LoadHybridData();
                    break;
            }
        }

        private void OnResetData(object sender, RoutedEventArgs e)
        {
            switch (modeComboBox.SelectedIndex)
            {
                case 0:
                    LoadUnboundData();
                    break;
                case 1:
                    LoadBoundData();
                    break;
                case 2:
                    LoadHybridData();
                    break;
            }
        }

        private void LoadUnboundData()
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            worksheet.DataSource = null;
            worksheet.RowCount = TotalRows + 1;
            worksheet.ColumnCount = TotalCols;

            // Reset column data maps and headers
            for (int i = 0; i < TotalCols; i++)
            {
                worksheet.Columns[i].DataMap = null;
                worksheet.Columns[i].CellType = null;
            }

            var data = DataSource.GetSortableData(TotalRows, TotalCols);
            worksheet.Load(data);

            // Style headers with Excel Green
            string headerStyleName = "SortHeaderStyle";
            if (worksheet.WorkBook.GetNamedStyle(headerStyleName) == null)
            {
                var style = new CellStyle
                {
                    BackColor = DrawingColor.FromArgb(255, 16, 124, 65), // #107C41 Excel Green
                    ForeColor = DrawingColor.White,
                    FontWeight = DrawingFontWeight.Bold,
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

            UpdateStatus();
        }

        private void LoadBoundData()
        {
            var customers = DataSource.GetCustomers(500).ToList();
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            
            // Clear prior row styles and load bound data
            worksheet.Rows[0].StyleName = null;
            worksheet.DataSource = customers;

            worksheet.Columns[0].DataMap = new PropertyDataMap("Id");
            worksheet.Columns[0].Width = 70;
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Id";

            worksheet.Columns[1].DataMap = new PropertyDataMap("FirstName");
            worksheet.Columns[1].Width = 130;
            worksheet.ColumnHeaders.Cells[0, 1].Value = "First Name";

            worksheet.Columns[2].DataMap = new PropertyDataMap("LastName");
            worksheet.Columns[2].Width = 130;
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Last Name";

            worksheet.Columns[3].DataMap = new PropertyDataMap("Gender");
            worksheet.Columns[3].Width = 90;
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Gender";

            worksheet.Columns[4].DataMap = new PropertyDataMap("Age");
            worksheet.Columns[4].Width = 80;
            worksheet.ColumnHeaders.Cells[0, 4].Value = "Age";

            worksheet.Columns[5].DataMap = new PropertyDataMap("Email");
            worksheet.Columns[5].Width = 200;
            worksheet.ColumnHeaders.Cells[0, 5].Value = "Email";

            worksheet.Columns[6].DataMap = new PropertyDataMap("Phone");
            worksheet.Columns[6].Width = 130;
            worksheet.ColumnHeaders.Cells[0, 6].Value = "Phone";

            worksheet.ColumnCount = 7;
            UpdateStatus();
        }

        private void LoadHybridData()
        {
            var customers = DataSource.GetCustomers(500).ToList();
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            
            worksheet.Rows[0].StyleName = null;
            worksheet.DataSource = customers;

            // Bound Columns
            worksheet.Columns[0].DataMap = new PropertyDataMap("Id");
            worksheet.Columns[0].Width = 70;
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Id";

            worksheet.Columns[1].DataMap = new PropertyDataMap("FirstName");
            worksheet.Columns[1].Width = 130;
            worksheet.ColumnHeaders.Cells[0, 1].Value = "First Name";

            worksheet.Columns[2].DataMap = new PropertyDataMap("LastName");
            worksheet.Columns[2].Width = 130;
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Last Name";

            worksheet.Columns[3].DataMap = new PropertyDataMap("Age");
            worksheet.Columns[3].Width = 80;
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Age";

            // Unbound Column 4: Checkbox
            worksheet.Columns[4].DataMap = null;
            worksheet.Columns[4].CellType = new CheckBoxCellType();
            worksheet.Columns[4].Width = 80;
            worksheet.ColumnHeaders.Cells[0, 4].Value = "Active";

            // Unbound Column 5: Notes
            worksheet.Columns[5].DataMap = null;
            worksheet.Columns[5].CellType = null;
            worksheet.Columns[5].Width = 160;
            worksheet.ColumnHeaders.Cells[0, 5].Value = "Review Notes";

            // Populate some unbound notes to demonstrate that they stay with records when sorted
            for (int r = 0; r < Math.Min(15, customers.Count); r++)
            {
                worksheet.SetValue(r, 4, r % 2 == 0);
                worksheet.SetValue(r, 5, $"Note for {customers[r].FirstName}");
            }

            worksheet.ColumnCount = 6;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            statusTextBlock.Text = $"IsBound: {worksheet.IsBound} | Rows: {worksheet.RowCount} | Cols: {worksheet.ColumnCount}";
        }

        private CellRange GetTargetSortRange()
        {
            var selection = spread.Sheets.ActiveSheet.Selection;
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;

            if (selection != default && selection.RowCount > 1)
            {
                return selection;
            }

            if (worksheet.IsBound)
            {
                return new CellRange(0, 0, worksheet.RowCount, worksheet.ColumnCount);
            }

            return new CellRange(1, 0, TotalRows, TotalCols);
        }

        private int GetTargetSortColumn()
        {
            var selection = spread.Sheets.ActiveSheet.Selection;
            if (selection != default)
            {
                return selection.LeftColumn;
            }
            return 0;
        }

        private void OnSortAscending(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            var range = GetTargetSortRange();
            var options = new SortOptions
            {
                HasHeader = !worksheet.IsBound,
                SortColumnOnly = !worksheet.IsBound
            };
            for (int col = range.LeftColumn; col < range.LeftColumn + range.ColumnCount; col++)
            {
                options.SortLevels.Add(new SortInfo(col, true));
            }
            worksheet.SortRange(range, options);
        }

        private void OnSortDescending(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            var range = GetTargetSortRange();
            var options = new SortOptions
            {
                HasHeader = !worksheet.IsBound,
                SortColumnOnly = !worksheet.IsBound
            };
            for (int col = range.LeftColumn; col < range.LeftColumn + range.ColumnCount; col++)
            {
                options.SortLevels.Add(new SortInfo(col, false));
            }
            worksheet.SortRange(range, options);
        }

        private void OnMultiLevelSort(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            var range = GetTargetSortRange();
            
            var options = new SortOptions
            {
                HasHeader = !worksheet.IsBound,
                SortColumnOnly = !worksheet.IsBound
            };

            if (worksheet.IsBound)
            {
                // Level 1: First Name (Col 1), Ascending
                options.SortLevels.Add(new SortInfo(1, true));
                // Level 2: Last Name (Col 2), Descending
                options.SortLevels.Add(new SortInfo(2, false));
            }
            else
            {
                if (range.ColumnCount >= 2)
                {
                    options.SortLevels.Add(new SortInfo(range.LeftColumn, true));
                    options.SortLevels.Add(new SortInfo(range.LeftColumn + 1, false));
                }
                else
                {
                    // Level 1: Category (Column 2), Ascending
                    options.SortLevels.Add(new SortInfo(2, true));
                    // Level 2: Rating (Column 7), Descending
                    options.SortLevels.Add(new SortInfo(7, false));
                }
            }
            
            worksheet.SortRange(range, options);
        }

        private void OnCustomSort(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            var range = GetTargetSortRange();
            
            var options = new SortOptions
            {
                HasHeader = !worksheet.IsBound,
                SortColumnOnly = !worksheet.IsBound
            };
            for (int col = range.LeftColumn; col < range.LeftColumn + range.ColumnCount; col++)
            {
                options.SortLevels.Add(new SortInfo(col, true) { CustomComparer = new StringLengthComparer() });
            }
            
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



