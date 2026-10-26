using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    public partial class DynamicGridSample : UserControl
    {
        public DynamicGridSample()
        {
            InitializeComponent();
            Loaded += OnSampleLoaded;
        }

        private void OnSampleLoaded(object sender, RoutedEventArgs e)
        {
            spreadMain.CellsSelectionChanged += OnCellsSelectionChanged;
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            var ws = sheetView.WorkSheet;

            // Reset spreadsheet structure
            ws.RowCount = 30;
            ws.ColumnCount = 10;
            ws.Clear(WorkSheetClearMode.All);

            // Configure 2-Tier Hierarchical Column Headers
            ws.ColumnHeaders.RowCount = 2;

            // Row 0 - Merged Group Headers
            ws.ColumnHeaders.AddSpan(0, 0, 1, 2);
            ws.ColumnHeaders.Cells[0, 0].Value = "Product Catalog";

            ws.ColumnHeaders.AddSpan(0, 2, 1, 3);
            ws.ColumnHeaders.Cells[0, 2].Value = "Q1 Monthly Sales ($k)";

            ws.ColumnHeaders.AddSpan(0, 5, 1, 2);
            ws.ColumnHeaders.Cells[0, 5].Value = "Metrics & Projections";

            // Row 1 - Individual Column Headers
            ws.ColumnHeaders.Cells[1, 0].Value = "SKU Code";
            ws.ColumnHeaders.Cells[1, 1].Value = "Product Name";
            ws.ColumnHeaders.Cells[1, 2].Value = "January";
            ws.ColumnHeaders.Cells[1, 3].Value = "February";
            ws.ColumnHeaders.Cells[1, 4].Value = "March";
            ws.ColumnHeaders.Cells[1, 5].Value = "Q1 Total";
            ws.ColumnHeaders.Cells[1, 6].Value = "Q2 Projected";

            // Header Styles
            var groupHeaderStyle = new CellStyle
            {
                FontWeight = DrawingFontWeight.Bold,
                FontSize = 13,
                HorizontalAlignment = CellHorizontalAlignment.Center,
                VerticalAlignment = CellVerticalAlignment.Center,
                BackColor = DrawingColor.FromArgb(255, 30, 41, 59), // Slate 800
                ForeColor = DrawingColor.FromArgb(255, 241, 245, 249) // Slate 100
            };
            ws.ColumnHeaders.SetStyle(0, 0, groupHeaderStyle);
            ws.ColumnHeaders.SetStyle(0, 2, groupHeaderStyle);
            ws.ColumnHeaders.SetStyle(0, 5, groupHeaderStyle);

            var subHeaderStyle = new CellStyle
            {
                FontWeight = DrawingFontWeight.Bold,
                FontSize = 12,
                HorizontalAlignment = CellHorizontalAlignment.Center,
                VerticalAlignment = CellVerticalAlignment.Center,
                BackColor = DrawingColor.FromArgb(255, 241, 245, 249),
                ForeColor = DrawingColor.FromArgb(255, 51, 65, 85)
            };
            for (int c = 0; c < 7; c++)
            {
                ws.ColumnHeaders.SetStyle(1, c, subHeaderStyle);
            }

            // Set Column Widths
            ws.Columns[0].Width = 110;
            ws.Columns[1].Width = 180;
            ws.Columns[2].Width = 95;
            ws.Columns[3].Width = 95;
            ws.Columns[4].Width = 95;
            ws.Columns[5].Width = 115;
            ws.Columns[6].Width = 125;

            // Initial Sample Data (Rows 0-3)
            string[] skus = { "SKU-1001", "SKU-1002", "SKU-1003", "SKU-1004" };
            string[] names = { "Hyperion Pro Laptop", "Optix 4K Monitor", "ErgoWave Keyboard", "Quantum Wireless Mouse" };
            double[,] sales = {
                { 45, 52, 60 },
                { 28, 34, 41 },
                { 85, 92, 104 },
                { 110, 118, 129 }
            };

            var numberStyle = new CellStyle { HorizontalAlignment = CellHorizontalAlignment.Right };
            var formulaStyle = new CellStyle
            {
                FontWeight = DrawingFontWeight.Bold,
                HorizontalAlignment = CellHorizontalAlignment.Right,
                BackColor = DrawingColor.FromArgb(255, 240, 253, 244),
                ForeColor = DrawingColor.FromArgb(255, 21, 128, 61)
            };

            for (int r = 0; r < 4; r++)
            {
                ws.SetValue(r, 0, skus[r]);
                ws.SetValue(r, 1, names[r]);

                for (int c = 0; c < 3; c++)
                {
                    ws.SetValue(r, 2 + c, sales[r, c]);
                    ws.SetStyle(r, 2 + c, numberStyle);
                }

                // Formulas: Q1 Total = C(r+1) + D(r+1) + E(r+1), Q2 Projected = F(r+1) * 1.15
                int excelRow = r + 1;
                ws.SetFormula(r, 5, $"=C{excelRow}+D{excelRow}+E{excelRow}");
                ws.SetFormula(r, 6, $"=F{excelRow}*1.15");

                ws.SetStyle(r, 5, formulaStyle);
                ws.SetStyle(r, 6, formulaStyle);
            }

            // Select first product cells to highlight drag-fill handle
            sheetView.SelectRange(new CellRange(0, 0, 4, 1));
            UpdateAddressDisplay(new CellRange(0, 0, 4, 1));
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void OnCellsSelectionChanged(object sender, CellsSelectionEventArgs e)
        {
            UpdateAddressDisplay(e.Selection);
        }

        private void UpdateAddressDisplay(CellRange range)
        {
            if (txtCellAddress == null) return;

            if (range.RowCount <= 1 && range.ColumnCount <= 1)
            {
                txtCellAddress.Text = GetCellName(range.TopRow, range.LeftColumn);
            }
            else
            {
                string topLeft = GetCellName(range.TopRow, range.LeftColumn);
                string bottomRight = GetCellName(range.BottomRow, range.RightColumn);
                txtCellAddress.Text = $"{topLeft}:{bottomRight}";
            }

            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView != null && range.RowCount == 1 && range.ColumnCount == 1)
            {
                string formula = sheetView.WorkSheet.GetFormula(range.TopRow, range.LeftColumn);
                if (!string.IsNullOrEmpty(formula))
                {
                    txtContextInfo.Text = $"fx {formula}";
                }
                else
                {
                    object val = sheetView.WorkSheet.GetValue(range.TopRow, range.LeftColumn);
                    txtContextInfo.Text = val != null ? $"Val: {val}" : "Ready";
                }
            }
            else if (range.RowCount > 1 || range.ColumnCount > 1)
            {
                txtContextInfo.Text = $"Selection: {range.RowCount}R × {range.ColumnCount}C";
            }
        }

        private static string GetCellName(int row, int col)
        {
            var sb = new StringBuilder();
            int c = col;
            while (c >= 0)
            {
                sb.Insert(0, (char)('A' + (c % 26)));
                c = (c / 26) - 1;
            }
            sb.Append(row + 1);
            return sb.ToString();
        }

        private void UpdateHistoryState()
        {
            btnUndo.IsEnabled = spreadMain.CanUndo;
            btnRedo.IsEnabled = spreadMain.CanRedo;
        }

        private void btnInsertRow_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            int insertAt = sheetView.Selection.TopRow;
            int count = Math.Max(1, sheetView.Selection.RowCount);

            sheetView.WorkSheet.Rows.Insert(insertAt, count);
            txtContextInfo.Text = $"Inserted {count} row(s) at {insertAt + 1}. Formulas remapped.";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            int deleteAt = sheetView.Selection.TopRow;
            int count = Math.Max(1, sheetView.Selection.RowCount);

            if (sheetView.WorkSheet.RowCount <= count)
            {
                MessageBox.Show("Cannot delete all rows.", "Delete Row", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            sheetView.WorkSheet.Rows.Remove(deleteAt, count);
            txtContextInfo.Text = $"Deleted {count} row(s). Dependent formulas evaluated.";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnInsertCol_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            int insertAt = sheetView.Selection.LeftColumn;
            int count = Math.Max(1, sheetView.Selection.ColumnCount);

            sheetView.WorkSheet.Columns.Insert(insertAt, count);
            txtContextInfo.Text = $"Inserted {count} column(s). Formulas remapped.";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnDeleteCol_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            int deleteAt = sheetView.Selection.LeftColumn;
            int count = Math.Max(1, sheetView.Selection.ColumnCount);

            if (sheetView.WorkSheet.ColumnCount <= count)
            {
                MessageBox.Show("Cannot delete all columns.", "Delete Column", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            sheetView.WorkSheet.Columns.Remove(deleteAt, count);
            txtContextInfo.Text = $"Deleted {count} column(s).";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            spreadMain.Undo();
            txtContextInfo.Text = "Undone previous action.";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            spreadMain.Redo();
            txtContextInfo.Text = "Redone previous action.";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnAutoFill_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            var sel = sheetView.Selection;
            if (sel.RowCount == 0 || sel.ColumnCount == 0) return;

            int extraRows = 3;
            switch (cmbFillCount.SelectedIndex)
            {
                case 0: extraRows = 1; break;
                case 1: extraRows = 3; break;
                case 2: extraRows = 5; break;
                case 3: extraRows = 10; break;
            }

            bool isCtrl = chkCtrlCopy.IsChecked == true;
            var targetRange = new CellRange(sel.TopRow, sel.LeftColumn, sel.RowCount + extraRows, sel.ColumnCount);

            sheetView.WorkSheet.AutoFill(sel, targetRange, isCtrlPressed: isCtrl);
            sheetView.SelectRange(targetRange);

            string modeText = isCtrl ? "Copied" : "Extended series";
            txtContextInfo.Text = $"{modeText} by {extraRows} row(s).";
            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            var sheetView = spreadMain.Sheets.ActiveSheet;
            if (sheetView == null) return;

            int idx = cmbClearMode.SelectedIndex;
            WorkSheetClearMode mode = WorkSheetClearMode.Data;
            switch (idx)
            {
                case 1: mode = WorkSheetClearMode.Formula; break;
                case 2: mode = WorkSheetClearMode.Styles; break;
                case 3: mode = WorkSheetClearMode.All; break;
            }

            var sel = sheetView.Selection;
            if (mode == WorkSheetClearMode.Data && (sel.RowCount < sheetView.WorkSheet.RowCount || sel.ColumnCount < sheetView.WorkSheet.ColumnCount))
            {
                sheetView.ClearContents();
                txtContextInfo.Text = "Cleared contents of selection.";
            }
            else
            {
                sheetView.WorkSheet.Clear(mode);
                txtContextInfo.Text = $"Cleared {mode} on worksheet.";
            }

            UpdateHistoryState();
            spreadMain.Invalidate();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            LoadSampleData();
            txtContextInfo.Text = "Worksheet reset to default sample data.";
        }
    }
}
