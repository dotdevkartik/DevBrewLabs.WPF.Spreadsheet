using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using System.Windows.Controls;
using SpreadsheetSampleExplorer.Data;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for Formulas.xaml
    /// Demonstrates basic, advanced, dependent, and multi-sheet interlinked formulas.
    /// </summary>
    public partial class Formulas : UserControl
    {
        public Formulas()
        {
            InitializeComponent();
            SetupWorkbook();
        }

        private void SetupWorkbook()
        {
            spread.SuspendUpdates = true;

            var workBook = spread.WorkBook;

            // Rename default sheet to Summary Dashboard
            var summarySheet = workBook.WorkSheets[0];
            summarySheet.Name = "SummaryDashboard";

            // Add additional sheets
            var salesSheet = workBook.WorkSheets.AddSheet("RegionalSales");
            var costSheet = workBook.WorkSheets.AddSheet("CostBreakdown");

            // Build Sheet 2: Regional Sales (Base Data & Dependent Formulas)
            SetupSalesSheet(salesSheet);

            // Build Sheet 3: Cost Breakdown (Expense Data & Dependent Percentages)
            SetupCostSheet(costSheet);

            // Build Sheet 1: Summary Dashboard (Cross-Sheet Interlinked Formulas)
            SetupSummarySheet(summarySheet);

            // Set Active Sheet to Summary Dashboard
            workBook.WorkSheets.ActiveSheet = summarySheet;

            spread.SuspendUpdates = false;
        }

        private void SetupSalesSheet(IWorksheet sheet)
        {
            // Section Header Style
            var titleStyle = new CellStyle();
            titleStyle.FontSize = 14;
            titleStyle.FontWeight = CellFontWeight.Bold;
            titleStyle.ForeColor = CellColor.FromArgb(255, 16, 124, 65);

            sheet.Cells[0, 0].Value = "REGIONAL SALES PERFORMANCE (DEPENDENT FORMULAS)";
            sheet.Cells[0, 0].Style = titleStyle;

            // Table Header Style
            var headerStyle = new CellStyle();
            headerStyle.FontWeight = CellFontWeight.Bold;
            headerStyle.BackColor = CellColor.FromArgb(255, 16, 124, 65); // Green Header
            headerStyle.ForeColor = CellColor.White;

            string[] headers = { "Region", "Units Sold", "Unit Price ($)", "Subtotal ($)", "Tax (8%)", "Net Total ($)", "Avg per Unit ($)" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = sheet.Cells[2, col]; // Row 3 in Excel
                cell.Value = headers[col];
                cell.Style = headerStyle;
            }

            object[][] sampleData = DataSource.GetRegionalSalesData();

            for (int i = 0; i < sampleData.Length; i++)
            {
                int row = 4 + i; // 0-based row (4 = Row 5 in Excel)
                int excelRow = row + 1;

                sheet.Cells[row, 0].Value = sampleData[i][0]; // Col A: Region
                sheet.Cells[row, 1].Value = sampleData[i][1]; // Col B: Units Sold
                sheet.Cells[row, 2].Value = sampleData[i][2]; // Col C: Unit Price

                // Dependent Formula Chain:
                // Subtotal (Col D) = Units (Col B) * Unit Price (Col C)
                sheet.Cells[row, 3].Formula = $"=B{excelRow}*C{excelRow}";

                // Tax (Col E) = Subtotal (Col D) * 0.08
                sheet.Cells[row, 4].Formula = $"=D{excelRow}*0.08";

                // Net Total (Col F) = Subtotal (Col D) + Tax (Col E)
                sheet.Cells[row, 5].Formula = $"=D{excelRow}+E{excelRow}";

                // Avg per Unit (Col G) = Net Total (Col F) / Units (Col B)
                sheet.Cells[row, 6].Formula = $"=F{excelRow}/B{excelRow}";
            }

            // Summary Row at Row 9 (0-indexed: 8)
            int summaryRow = 8;
            var summaryStyle = new CellStyle();
            summaryStyle.FontWeight = CellFontWeight.Bold;

            sheet.Cells[summaryRow, 0].Value = "Total / Overall";
            sheet.Cells[summaryRow, 0].Style = summaryStyle;

            sheet.Cells[summaryRow, 1].Formula = "=SUM(B5:B8)"; // Total Units (Col B)
            sheet.Cells[summaryRow, 1].Style = summaryStyle;

            sheet.Cells[summaryRow, 3].Formula = "=SUM(D5:D8)"; // Total Subtotal (Col D)
            sheet.Cells[summaryRow, 3].Style = summaryStyle;

            sheet.Cells[summaryRow, 4].Formula = "=SUM(E5:E8)"; // Total Tax (Col E)
            sheet.Cells[summaryRow, 4].Style = summaryStyle;

            var grandTotalStyle = new CellStyle();
            grandTotalStyle.FontWeight = CellFontWeight.Bold;
            grandTotalStyle.BackColor = CellColor.FromArgb(255, 230, 245, 235);

            sheet.Cells[summaryRow, 5].Formula = "=SUM(F5:F8)"; // Grand Net Total (Col F)
            sheet.Cells[summaryRow, 5].Style = grandTotalStyle;

            sheet.Cells[summaryRow, 6].Formula = "=AVERAGE(F5:F8)"; // Average Net Total (Col G)
            sheet.Cells[summaryRow, 6].Style = summaryStyle;

            // Column Widths
            for (int c = 0; c < 7; c++)
            {
                sheet.Columns[c].Width = 140;
            }
        }

        private void SetupCostSheet(IWorksheet sheet)
        {
            // Section Header Style
            var titleStyle = new CellStyle();
            titleStyle.FontSize = 14;
            titleStyle.FontWeight = CellFontWeight.Bold;
            titleStyle.ForeColor = CellColor.FromArgb(255, 31, 78, 121);

            sheet.Cells[0, 0].Value = "OPERATING EXPENSES (COST BREAKDOWN)";
            sheet.Cells[0, 0].Style = titleStyle;

            // Table Header Style
            var headerStyle = new CellStyle();
            headerStyle.FontWeight = CellFontWeight.Bold;
            headerStyle.BackColor = CellColor.FromArgb(255, 31, 78, 121); // Blue Header
            headerStyle.ForeColor = CellColor.White;

            string[] headers = { "Expense Category", "Q1 ($)", "Q2 ($)", "Q3 ($)", "Q4 ($)", "Annual Total ($)", "% of OPEX" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = sheet.Cells[2, col]; // Row 3 in Excel
                cell.Value = headers[col];
                cell.Style = headerStyle;
            }

            object[][] expenseData = DataSource.GetExpenseData();

            for (int i = 0; i < expenseData.Length; i++)
            {
                int row = 3 + i; // 0-based row (3 = Row 4 in Excel)
                int excelRow = row + 1;

                sheet.Cells[row, 0].Value = expenseData[i][0]; // Col A: Category
                sheet.Cells[row, 1].Value = expenseData[i][1]; // Col B: Q1
                sheet.Cells[row, 2].Value = expenseData[i][2]; // Col C: Q2
                sheet.Cells[row, 3].Value = expenseData[i][3]; // Col D: Q3
                sheet.Cells[row, 4].Value = expenseData[i][4]; // Col E: Q4

                // Annual Total Formula (Col F) = SUM(B4:E4)
                sheet.Cells[row, 5].Formula = $"=SUM(B{excelRow}:E{excelRow})";

                // % of OPEX (Col G) = Annual Total (Col F) / Total OPEX (F9)
                sheet.Cells[row, 6].Formula = $"=F{excelRow}/F9";
            }

            // Total OPEX Row at Row 9 in Excel (0-indexed: 8)
            int totalRow = 8;
            var summaryStyle = new CellStyle();
            summaryStyle.FontWeight = CellFontWeight.Bold;

            sheet.Cells[totalRow, 0].Value = "Total OPEX";
            sheet.Cells[totalRow, 0].Style = summaryStyle;

            sheet.Cells[totalRow, 1].Formula = "=SUM(B4:B8)"; // Q1 Total (Col B)
            sheet.Cells[totalRow, 2].Formula = "=SUM(C4:C8)"; // Q2 Total (Col C)
            sheet.Cells[totalRow, 3].Formula = "=SUM(D4:D8)"; // Q3 Total (Col D)
            sheet.Cells[totalRow, 4].Formula = "=SUM(E4:E8)"; // Q4 Total (Col E)

            // Grand Total OPEX (Col F)
            var grandTotalStyle = new CellStyle();
            grandTotalStyle.FontWeight = CellFontWeight.Bold;
            grandTotalStyle.BackColor = CellColor.FromArgb(255, 220, 230, 245);

            sheet.Cells[totalRow, 5].Formula = "=SUM(F4:F8)";
            sheet.Cells[totalRow, 5].Style = grandTotalStyle;

            sheet.Cells[totalRow, 6].Formula = "=SUM(G4:G8)"; // Total % (Col G) -> 100%
            sheet.Cells[totalRow, 6].Style = summaryStyle;

            // Column Widths
            sheet.Columns[0].Width = 180;
            for (int c = 1; c <= 6; c++)
            {
                sheet.Columns[c].Width = 130;
            }
        }

        private void SetupSummarySheet(IWorksheet sheet)
        {
            // Section Header Style
            var titleStyle = new CellStyle();
            titleStyle.FontSize = 14;
            titleStyle.FontWeight = CellFontWeight.Bold;

            sheet.Cells[0, 0].Value = "EXECUTIVE SUMMARY (INTERLINKED MULTI-SHEET FORMULAS)";
            sheet.Cells[0, 0].Style = titleStyle;

            // Table Header Style
            var headerStyle = new CellStyle();
            headerStyle.FontWeight = CellFontWeight.Bold;
            headerStyle.BackColor = CellColor.FromArgb(255, 70, 70, 70); // Dark Gray Header
            headerStyle.ForeColor = CellColor.White;

            string[] headers = { "Key Metric", "Calculated Value", "Formula", "Source & Dependency Description" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = sheet.Cells[2, col]; // Row 3 in Excel
                cell.Value = headers[col];
                cell.Style = headerStyle;
            }

            // Cross-Sheet Interlinked Formulas Data Definition
            // Excel Rows 4 to 11 (0-indexed: 3 to 10)
            object[][] summaryRows = new object[][]
            {
                new object[] { "Total Gross Revenue",       "=RegionalSales!F9",           "='RegionalSales'!F9",           "Interlinked: Total Net Revenue from Regional Sales Sheet" },
                new object[] { "Total Operating Expenses",  "=CostBreakdown!F9",           "='Cost Breakdown'!F9",           "Interlinked: Total OPEX from Cost Breakdown Sheet" },
                new object[] { "Net Operating Profit",      "=B4-B5",                      "=B4-B5",                         "Dependent: Gross Revenue (B4) minus OPEX (B5)" },
                new object[] { "Operating Profit Margin",   "=B6/B4",                      "=B6/B4",                         "Dependent Ratio: Net Profit (B6) / Gross Revenue (B4)" },
                new object[] { "Average Regional Revenue",  "=RegionalSales!G9",           "='RegionalSales'!G9",           "Interlinked: Average sales from Regional Sales Sheet" },
                new object[] { "Peak Regional Sales",       "=MAX(RegionalSales!F5:F8)",   "=MAX('RegionalSales'!F5:F8)",   "Cross-Sheet Aggregation: MAX from Regional Sales range" },
                new object[] { "Lowest Regional Sales",     "=MIN(RegionalSales!F5:F8)",   "=MIN('RegionalSales'!F5:F8)",   "Cross-Sheet Aggregation: MIN from Regional Sales range" },
                new object[] { "Active Region Count",       "=COUNT(RegionalSales!F5:F8)", "=COUNT('RegionalSales'!F5:F8)", "Cross-Sheet Aggregation: COUNT of regions" }
            };

            var boldStyle = new CellStyle();
            boldStyle.FontWeight = CellFontWeight.Bold;
            boldStyle.BackColor = CellColor.FromArgb(255, 220, 245, 230);

            var highlightStyle = new CellStyle();
            highlightStyle.FontWeight = CellFontWeight.Bold;
            highlightStyle.BackColor = CellColor.FromArgb(255, 220, 245, 230);

            for (int i = 0; i < summaryRows.Length; i++)
            {
                int row = 3 + i; // 0-based row (3 = Row 4 in Excel)
                sheet.Cells[row, 0].Value = summaryRows[i][0]; // Col A: Metric Name
                sheet.Cells[row, 1].Formula = (string)summaryRows[i][1]; // Col B: Formula
                sheet.Cells[row, 2].Value = summaryRows[i][2]; // Col C: Formula Display Text
                sheet.Cells[row, 3].Value = summaryRows[i][3]; // Col D: Description

                // Apply styles
                if (i == 2) // Net Operating Profit
                {
                    sheet.Cells[row, 0].Style = highlightStyle;
                    sheet.Cells[row, 1].Style = highlightStyle;
                }
                else
                {
                    sheet.Cells[row, 0].Style = boldStyle;
                    sheet.Cells[row, 1].Style = boldStyle;
                }
            }

            // Column Widths
            sheet.Columns[0].Width = 210;
            sheet.Columns[1].Width = 150;
            sheet.Columns[2].Width = 240;
            sheet.Columns[3].Width = 350;
        }
    }
}


