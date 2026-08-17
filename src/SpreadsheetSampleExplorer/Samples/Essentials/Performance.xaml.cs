using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for Performance.xaml
    /// </summary>
    public partial class Performance : UserControl
    {
        private bool _isInitialized;

        public Performance()
        {
            InitializeComponent();
            Loaded += Performance_Loaded;
        }

        private async void Performance_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                await RunBenchmark();
            }
        }

        private async Task RunBenchmark()
        {
            int rowCount = 1000000;
            if (_cmbRowCount?.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int parsedCount))
            {
                rowCount = parsedCount;
            }

            int colCount = 10;

            _txtTotalTime.Text = "Preparing data...";

            var swTotal = Stopwatch.StartNew();

            // 1. Data Preparation
            var swPrep = Stopwatch.StartNew();
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            worksheet.RowCount = rowCount;
            worksheet.ColumnCount = colCount;

            var sampleData = await Task.Run(() =>
            {
                string[] departments = { "Engineering", "Sales", "Marketing", "Finance", "Human Resources", "Operations", "Legal", "Product" };
                string[] regions = { "North America", "Europe", "Asia Pacific", "Latin America", "Middle East" };
                string[] statuses = { "Active", "Pending", "Completed", "On Hold", "Archived" };

                var rnd = new Random(42);
                var data = new object[rowCount, colCount];

                string[] headers = { "ID", "Employee Ref", "Department", "Region", "Salary ($)", "Score", "Projects", "Status", "Year Joined", "Security Code" };
                for (int col = 0; col < colCount; col++)
                {
                    data[0, col] = headers[col];
                }

                for (int row = 1; row < rowCount; row++)
                {
                    data[row, 0] = row;
                    data[row, 1] = $"EMP-{100000 + row}";
                    data[row, 2] = departments[rnd.Next(departments.Length)];
                    data[row, 3] = regions[rnd.Next(regions.Length)];
                    data[row, 4] = rnd.Next(45000, 185000);
                    data[row, 5] = Math.Round(3.0 + rnd.NextDouble() * 2.0, 1);
                    data[row, 6] = rnd.Next(1, 15);
                    data[row, 7] = statuses[rnd.Next(statuses.Length)];
                    data[row, 8] = rnd.Next(2010, 2026);
                    data[row, 9] = $"SEC-{rnd.Next(1000, 9999)}";
                }
                return data;
            });

            swPrep.Stop();

            // 2. Engine Loading
            var swEngine = Stopwatch.StartNew();


            worksheet.Load(sampleData);

            string headerStyleName = "ScrollHeaderStyle";
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

            worksheet.Columns[0].Width = 70;
            worksheet.Columns[1].Width = 110;
            worksheet.Columns[2].Width = 140;
            worksheet.Columns[3].Width = 130;
            worksheet.Columns[4].Width = 110;
            worksheet.Columns[5].Width = 100;
            worksheet.Columns[7].Width = 100;
            swEngine.Stop();

            // 3. UI First Render Measure
            var swRender = Stopwatch.StartNew();

            swRender.Stop();
            swTotal.Stop();

            double prepMs = swPrep.Elapsed.TotalMilliseconds;
            double loadMs = swEngine.Elapsed.TotalMilliseconds;
            double renderMs = swRender.Elapsed.TotalMilliseconds;
            double totalMs = swTotal.Elapsed.TotalMilliseconds;

            double spreadLoadMs = totalMs - prepMs;

            _txtTotalTime.Text = $"{spreadLoadMs:N0} ms";
            _txtCellCount.Text = $"{rowCount:N0} rows × {colCount} cols ({rowCount * colCount:N0} cells)";
        }

        private async void OnRunBenchmarkClick(object sender, RoutedEventArgs e)
        {
            await RunBenchmark();
        }
    }
}



