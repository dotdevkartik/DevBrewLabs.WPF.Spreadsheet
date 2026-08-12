using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using System.Windows;
using System.Windows.Controls;

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

            // Header
            workSheet.SetValue(1, 1, "Financial Report Q3");
            workSheet.AddSpan(1, 1, 2, 5); // Span 2 rows, 5 columns
            
            var titleStyle = new CellStyle { 
                FontWeight = CellFontWeight.Bold, 
                FontSize = 18, 
                HorizontalAlignment = CellHorizontalAlignment.Center,
                VerticalAlignment = CellVerticalAlignment.Center,
                BackColor = CellColor.FromArgb(255, 224, 242, 254),
                ForeColor = CellColor.FromArgb(255, 3, 105, 161)
            };
            workSheet.SetStyle(1, 1, titleStyle);

            // Sub headers
            workSheet.SetValue(4, 1, "Revenue Streams");
            workSheet.AddSpan(4, 1, 1, 3); // Span 1 row, 3 columns
            workSheet.SetStyle(4, 1, new CellStyle { FontWeight = CellFontWeight.Bold, BackColor = CellColor.FromArgb(255, 241, 245, 249) });
            
            workSheet.SetValue(5, 1, "Product A");
            workSheet.SetValue(6, 1, "Product B");
            workSheet.SetValue(7, 1, "Product C");
            
            workSheet.SetValue(5, 2, 15000);
            workSheet.SetValue(6, 2, 23000);
            workSheet.SetValue(7, 2, 8500);

            // Another span example
            workSheet.SetValue(10, 1, "Notes:");
            workSheet.AddSpan(10, 1, 4, 4); // Big notes box
            workSheet.SetStyle(10, 1, new CellStyle { 
                VerticalAlignment = CellVerticalAlignment.Top,
                BackColor = CellColor.FromArgb(255, 254, 243, 199)
            });
            
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
                    spreadMain.SelectionManager.MergeSelection();
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
            spreadMain.SelectionManager.UnmergeSelection();
            spreadMain.Invalidate();
        }
    }
}
