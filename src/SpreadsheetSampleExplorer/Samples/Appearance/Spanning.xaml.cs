using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using System.Windows;
using System.Windows.Controls;
using SpreadsheetSampleExplorer.Data;

namespace SpreadsheetSampleExplorer.Samples
{
    public partial class Spanning : UserControl
    {
        public Spanning()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var workSheet = spreadMain.WorkBook.WorkSheets.GetSheet(0);
            
            workSheet.RowCount = 50;
            workSheet.ColumnCount = 20;

            // Main Header
            workSheet.SetValue(1, 1, "Q3 2026 REGIONAL SALES REPORT");
            workSheet.AddSpan(1, 1, 2, 7); // Span 2 rows, 7 columns
            
            var titleStyle = new CellStyle { 
                FontWeight = CellFontWeight.Bold, 
                FontSize = 22, 
                HorizontalAlignment = CellHorizontalAlignment.Center,
                VerticalAlignment = CellVerticalAlignment.Center,
                BackColor = CellColor.FromArgb(255, 30, 64, 175), // Deep blue
                ForeColor = CellColor.FromArgb(255, 255, 255, 255) // White
            };
            workSheet.SetStyle(1, 1, titleStyle);

            // Region Headers
            workSheet.SetValue(4, 2, "North America");
            workSheet.AddSpan(4, 2, 1, 2);
            workSheet.SetValue(4, 4, "Europe");
            workSheet.AddSpan(4, 4, 1, 2);
            workSheet.SetValue(4, 6, "Asia Pacific");
            workSheet.AddSpan(4, 6, 1, 2);

            var regionHeaderStyle = new CellStyle {
                FontWeight = CellFontWeight.Bold,
                HorizontalAlignment = CellHorizontalAlignment.Center,
                BackColor = CellColor.FromArgb(255, 224, 231, 255), // Light blue
                ForeColor = CellColor.FromArgb(255, 30, 58, 138)
            };
            workSheet.SetStyle(4, 2, regionHeaderStyle);
            workSheet.SetStyle(4, 4, regionHeaderStyle);
            workSheet.SetStyle(4, 6, regionHeaderStyle);

            // Sub headers (Target vs Actual)
            var subHeaderStyle = new CellStyle { FontWeight = CellFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Right };
            for(int i = 0; i < 3; i++)
            {
                workSheet.SetValue(5, 2 + (i * 2), "Target");
                workSheet.SetValue(5, 3 + (i * 2), "Actual");
                workSheet.SetStyle(5, 2 + (i * 2), subHeaderStyle);
                workSheet.SetStyle(5, 3 + (i * 2), subHeaderStyle);
            }

            // Products
            var products = new[] { "Enterprise License", "Cloud Storage", "Premium Support", "Consulting Services" };
            for(int i = 0; i < products.Length; i++)
            {
                workSheet.SetValue(6 + i, 1, products[i]);
                workSheet.SetStyle(6 + i, 1, new CellStyle { FontWeight = CellFontWeight.Bold });
            }

            int[,] data = DataSource.GetSpanningSalesData();

            for(int r = 0; r < 4; r++)
            {
                for(int c = 0; c < 6; c++)
                {
                    workSheet.SetValue(6 + r, 2 + c, $"${data[r, c]:N0}");
                }
            }

            // Summary notes box
            workSheet.AddSpan(11, 1, 5, 7); 
            workSheet.SetStyle(11, 1, new CellStyle { 
                VerticalAlignment = CellVerticalAlignment.Top,
                BackColor = CellColor.FromArgb(255, 254, 249, 195), // Light yellow
                ForeColor = CellColor.FromArgb(255, 113, 63, 18),
                FontWeight = CellFontWeight.Bold,
                FontSize = 14
            });
            workSheet.SetValue(11, 1, " Executive Summary & Notes:\n\n - North America exceeded Enterprise targets by 10%.\n - Europe saw a surge in Cloud Storage adoption.\n - Asia Pacific slightly missed consulting targets; requires Q4 intervention.");
            
            // Adjust column width for product names
            workSheet.Columns[0].Width = 160;

            spreadMain.Invalidate();
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.SheetViews.ActiveSheetView;
            var selection = sheetView.Selection;
            if (selection.RowCount > 1 || selection.ColumnCount > 1)
            {
                try
                {
                    sheetView.MergeRange(selection);
                    spreadMain.Invalidate();
                }
                catch (System.InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Cannot Merge", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btnUnmerge_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.SheetViews.ActiveSheetView;
            sheetView.UnmergeRange(sheetView.Selection);
            spreadMain.Invalidate();
        }
    }
}
