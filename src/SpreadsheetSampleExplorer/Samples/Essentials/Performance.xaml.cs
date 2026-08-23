using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SpreadsheetSampleExplorer.Data;

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
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;
            worksheet.RowCount = rowCount;
            worksheet.ColumnCount = colCount;

            var sampleData = await Task.Run(() =>
            {
                return DataSource.GetEmployeesData(rowCount, colCount);
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
                    BackColor = DrawingColor.FromArgb(255, 16, 124, 65), // #107C41 Excel Green
                    ForeColor = DrawingColor.White,
                    FontWeight = DrawingFontWeight.Bold,
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



