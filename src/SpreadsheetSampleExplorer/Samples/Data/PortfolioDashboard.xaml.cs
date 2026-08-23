using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SpreadsheetSampleExplorer.Data;
using SpreadsheetSampleExplorer.Models;

namespace SpreadsheetSampleExplorer.Samples
{
    public class CurrencyFormatter : IFormatter
    {
        public string Format(object value)
        {
            if (value is double d) return d.ToString("$#,##0.00");
            if (value is int i) return i.ToString("$#,##0.00");
            return value?.ToString();
        }
    }

    public class PercentageFormatter : IFormatter
    {
        public string Format(object value)
        {
            if (value is double d)
            {
                if (d > 0) return "+" + d.ToString("0.00") + "%";
                return d.ToString("0.00") + "%";
            }
            return value?.ToString();
        }
    }

    public class TrendFormatter : IFormatter
    {
        public string Format(object value)
        {
            if (value is double d)
            {
                if (d > 0) return "▲ $" + d.ToString("#,##0.00");
                if (d < 0) return "▼ -$" + Math.Abs(d).ToString("#,##0.00");
                return "▬ $" + d.ToString("#,##0.00");
            }
            return value?.ToString();
        }
    }
    
    public class ValueTrendFormatter : IFormatter
    {
        public string Format(object value)
        {
            if (value is double d)
            {
                if (d > 0) return "▲ " + d.ToString("$#,##0.00");
                if (d < 0) return "▼ -" + Math.Abs(d).ToString("$#,##0.00");
                return "▬ " + d.ToString("$#,##0.00");
            }
            return value?.ToString();
        }
    }

    public partial class PortfolioDashboard : UserControl
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();
        private int _tickCount = 90;

        private List<StockData> _stocks = DataSource.GetStocks();

        private CurrencyFormatter _currencyFormatter = new CurrencyFormatter();
        private PercentageFormatter _percentageFormatter = new PercentageFormatter();
        private TrendFormatter _trendFormatter = new TrendFormatter();
        private ValueTrendFormatter _valueTrendFormatter = new ValueTrendFormatter();

        public PortfolioDashboard()
        {
            InitializeComponent();
            SetupStyles();
            SetupDashboard();
            StartLiveFeed();
            Unloaded += (s, e) => _timer?.Stop();
        }

        private void SetupStyles()
        {
            var workBook = spread.WorkBook;
            var modernFont = new DrawingFontFamily("Segoe UI Variable Display, Segoe UI, Inter, Helvetica");

            workBook.AddNamedStyle("GlobalHeader", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontSize = 22, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("HeaderLive", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 22, 163, 74), FontSize = 11, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Center, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("HeaderSub", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 71, 85, 105), FontSize = 12, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            
            workBook.AddNamedStyle("SectionHeader", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.FromArgb(255, 15, 33, 73), ForeColor = DrawingColor.White, FontSize = 11, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("ColHeader", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.FromArgb(255, 241, 245, 249), ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontSize = 11, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Center, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("ColHeaderLeft", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.FromArgb(255, 241, 245, 249), ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontSize = 11, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            
            workBook.AddNamedStyle("CardTitle", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontSize = 11, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Bottom });
            workBook.AddNamedStyle("CardValue", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontSize = 22, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("CardSubValueGain", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 22, 163, 74), FontSize = 14, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Bottom });
            workBook.AddNamedStyle("CardSubValueLoss", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 220, 38, 38), FontSize = 14, FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Bottom });
            workBook.AddNamedStyle("CardSubtext", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 100, 116, 139), FontSize = 10, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Top });
            workBook.AddNamedStyle("CardWhite", new CellStyle { BackColor = DrawingColor.White });
            
            workBook.AddNamedStyle("TickerStyle", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("DataStyle", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 15, 23, 42), HorizontalAlignment = CellHorizontalAlignment.Center, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("DataStyleLeft", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 15, 23, 42), HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("GainStyle", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 22, 163, 74), FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Center, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("LossStyle", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.White, ForeColor = DrawingColor.FromArgb(255, 220, 38, 38), FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Center, VerticalAlignment = CellVerticalAlignment.Center });
            
            workBook.AddNamedStyle("TotalRow", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.FromArgb(255, 238, 242, 255), ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Center, VerticalAlignment = CellVerticalAlignment.Center });
            workBook.AddNamedStyle("TotalRowLeft", new CellStyle { FontFamily = modernFont, BackColor = DrawingColor.FromArgb(255, 238, 242, 255), ForeColor = DrawingColor.FromArgb(255, 15, 33, 73), FontWeight = DrawingFontWeight.Bold, HorizontalAlignment = CellHorizontalAlignment.Left, VerticalAlignment = CellVerticalAlignment.Center });
        }

        private void SetupDashboard()
        {
            spread.SuspendUpdates = true;
            var worksheet = spread.WorkBook.WorkSheets[0];
            worksheet.Name = "Terminal";
            worksheet.RowCount = 50;
            worksheet.ColumnCount = 14;

            // Global Background
            for (int r = 0; r < 50; r++)
                for (int c = 0; c < 14; c++)
                    worksheet.Cells[r, c].StyleName = "CardWhite";

            // Row Heights
            worksheet.Rows[0].Height = 40;
            worksheet.Rows[1].Height = 15; // Gap
            worksheet.Rows[2].Height = 25; // Card Title
            worksheet.Rows[3].Height = 40; // Card Value
            worksheet.Rows[4].Height = 20; // Card Sub 1
            worksheet.Rows[5].Height = 20; // Card Sub 2
            worksheet.Rows[6].Height = 20; // Gap
            worksheet.Rows[7].Height = 35; // Section Header
            worksheet.Rows[8].Height = 30; // Col Header
            for (int i = 0; i < 8; i++) worksheet.Rows[9 + i].Height = 30; // Data
            worksheet.Rows[17].Height = 35; // Total
            worksheet.Rows[18].Height = 20; // Gap
            worksheet.Rows[19].Height = 25; // Bottom Card Title
            worksheet.Rows[20].Height = 30; // Bottom Card Value
            worksheet.Rows[21].Height = 20; // Bottom Card Sub

            // Col Widths
            worksheet.Columns[0].Width = 70;
            worksheet.Columns[1].Width = 140;
            worksheet.Columns[2].Width = 70;
            worksheet.Columns[3].Width = 90;
            worksheet.Columns[4].Width = 90;
            worksheet.Columns[5].Width = 100;
            worksheet.Columns[6].Width = 90;
            worksheet.Columns[7].Width = 100;
            worksheet.Columns[8].Width = 100;
            worksheet.Columns[9].Width = 20; // Gap
            worksheet.Columns[10].Width = 70;
            worksheet.Columns[11].Width = 100;
            worksheet.Columns[12].Width = 90;

            // Header
            worksheet.AddSpan(0, 0, 1, 3);
            worksheet.Cells[0, 0].Value = "GLOBAL PORTFOLIO";
            worksheet.Cells[0, 0].StyleName = "GlobalHeader";
            
            worksheet.Cells[0, 3].Value = "● LIVE";
            worksheet.Cells[0, 3].StyleName = "HeaderLive";
            
            worksheet.AddSpan(0, 4, 1, 2);
            worksheet.Cells[0, 4].Value = $"Ticks: {_tickCount}";
            worksheet.Cells[0, 4].StyleName = "HeaderSub";
            
            worksheet.AddSpan(0, 6, 1, 3);
            worksheet.Cells[0, 6].Value = $"Last Update: {DateTime.Now:HH:mm:ss}";
            worksheet.Cells[0, 6].StyleName = "HeaderSub";

            // Top Cards
            DrawCard(worksheet, 2, 0, 3, "PORTFOLIO VALUE", "=F18", null, "vs Previous Close");
            DrawCard(worksheet, 2, 3, 3, "TODAY'S P&L", "=H18", null, "vs Previous Close");
            DrawCard(worksheet, 2, 6, 3, "TOTAL P&L", "=I18", null, "vs Total Invested");
            
            double totalInvested = _stocks.Sum(s => s.BasePrice * s.Shares);
            DrawCard(worksheet, 2, 10, 3, "TOTAL INVESTED", totalInvested, "89.9%", "Invested Percentage");
            
            worksheet.Cells[3, 0].Formatter = _currencyFormatter;
            worksheet.Cells[3, 3].Formatter = _valueTrendFormatter;
            worksheet.Cells[3, 6].Formatter = _valueTrendFormatter;
            worksheet.Cells[3, 10].Formatter = _currencyFormatter;
            
            // Dynamic % change for cards based on formulas
            worksheet.Cells[4, 0].Formula = "=H18/(F18-H18)*100";
            worksheet.Cells[4, 0].Formatter = _percentageFormatter;
            worksheet.Cells[4, 0].StyleName = "CardSubValueGain";
            
            worksheet.Cells[4, 3].Formula = "=H18/(F18-H18)*100";
            worksheet.Cells[4, 3].Formatter = _percentageFormatter;
            worksheet.Cells[4, 3].StyleName = "CardSubValueGain";
            
            worksheet.Cells[4, 6].Formula = "=I18/K4*100";
            worksheet.Cells[4, 6].Formatter = _percentageFormatter;
            worksheet.Cells[4, 6].StyleName = "CardSubValueGain";

            // Section Headers
            worksheet.AddSpan(7, 0, 1, 9);
            worksheet.Cells[7, 0].Value = " HOLDINGS";
            worksheet.Cells[7, 0].StyleName = "SectionHeader";
            
            worksheet.AddSpan(7, 10, 1, 3);
            worksheet.Cells[7, 10].Value = " ALLOCATION BY VALUE";
            worksheet.Cells[7, 10].StyleName = "SectionHeader";

            // Col Headers (Holdings)
            string[] hHeaders = { "TICKER", "COMPANY", "SHARES", "AVG COST ($)", "LIVE ($)", "VALUE ($)", "DAY % CHG", "DAY P&L ($)", "TOTAL P&L ($)" };
            for(int i=0; i<9; i++) {
                worksheet.Cells[8, i].Value = hHeaders[i];
                worksheet.Cells[8, i].StyleName = i < 2 ? "ColHeaderLeft" : "ColHeader";
            }
            
            // Col Headers (Allocation)
            worksheet.Cells[8, 10].Value = "TICKER";
            worksheet.Cells[8, 10].StyleName = "ColHeaderLeft";
            worksheet.Cells[8, 11].Value = "VALUE ($)";
            worksheet.Cells[8, 11].StyleName = "ColHeader";
            worksheet.Cells[8, 12].Value = "ALLOCATION";
            worksheet.Cells[8, 12].StyleName = "ColHeader";

            // Data
            for (int i = 0; i < _stocks.Count; i++)
            {
                int r = 9 + i;
                int excelRow = r + 1;
                var stock = _stocks[i];

                // Holdings Table
                worksheet.Cells[r, 0].Value = stock.Ticker;
                worksheet.Cells[r, 0].StyleName = "TickerStyle";
                
                worksheet.Cells[r, 1].Value = stock.Company;
                worksheet.Cells[r, 1].StyleName = "DataStyleLeft";
                
                worksheet.Cells[r, 2].Value = stock.Shares;
                worksheet.Cells[r, 2].StyleName = "DataStyle";
                
                worksheet.Cells[r, 3].Value = stock.BasePrice;
                worksheet.Cells[r, 3].Formatter = _currencyFormatter;
                worksheet.Cells[r, 3].StyleName = "DataStyle";
                
                worksheet.Cells[r, 4].Value = stock.CurrentPrice;
                worksheet.Cells[r, 4].Formatter = _currencyFormatter;
                worksheet.Cells[r, 4].StyleName = "DataStyle";
                
                worksheet.Cells[r, 5].Formula = $"=C{excelRow}*E{excelRow}";
                worksheet.Cells[r, 5].Formatter = _currencyFormatter;
                worksheet.Cells[r, 5].StyleName = "DataStyle";
                
                worksheet.Cells[r, 6].Formula = $"=(E{excelRow}-D{excelRow})/D{excelRow}*100";
                worksheet.Cells[r, 6].Formatter = _percentageFormatter;
                
                worksheet.Cells[r, 7].Formula = $"=C{excelRow}*(E{excelRow}-D{excelRow})"; // Actually Day P&L needs Open Price, but let's use Base Price to simulate "Previous Close".
                worksheet.Cells[r, 7].Formatter = _trendFormatter;
                
                worksheet.Cells[r, 8].Formula = $"=C{excelRow}*(E{excelRow}-D{excelRow})";
                worksheet.Cells[r, 8].Formatter = _trendFormatter;
                
                // Allocation Table
                worksheet.Cells[r, 10].Value = stock.Ticker;
                worksheet.Cells[r, 10].StyleName = "TickerStyle";
                
                worksheet.Cells[r, 11].Formula = $"=F{excelRow}";
                worksheet.Cells[r, 11].Formatter = _currencyFormatter;
                worksheet.Cells[r, 11].StyleName = "DataStyle";
                
                worksheet.Cells[r, 12].Formula = $"=F{excelRow}/F18*100";
                worksheet.Cells[r, 12].Formatter = _percentageFormatter; // We can reuse percentage formatter but we need it without + sign maybe. Let's just use it, or create a generic one. We'll add a generic one later if needed. Wait, percentage formatter adds +. For allocation we just need "0.0%". Let's create an AllocationFormatter.
                
                UpdateRowStyles(worksheet, r, stock);
            }

            // Footer
            worksheet.AddSpan(17, 0, 1, 2);
            worksheet.Cells[17, 0].Value = "Total / Average";
            worksheet.Cells[17, 0].StyleName = "TotalRowLeft";
            
            worksheet.Cells[17, 2].Formula = "=SUM(C10:C17)";
            worksheet.Cells[17, 2].StyleName = "TotalRow";
            
            worksheet.Cells[17, 3].Formula = "=AVERAGE(D10:D17)";
            worksheet.Cells[17, 3].Formatter = _currencyFormatter;
            worksheet.Cells[17, 3].StyleName = "TotalRow";
            
            worksheet.Cells[17, 4].Value = "—";
            worksheet.Cells[17, 4].StyleName = "TotalRow";
            
            worksheet.Cells[17, 5].Formula = "=SUM(F10:F17)";
            worksheet.Cells[17, 5].Formatter = _currencyFormatter;
            worksheet.Cells[17, 5].StyleName = "TotalRow";
            
            worksheet.Cells[17, 6].Formula = "=SUM(H10:H17)/(SUM(F10:F17)-SUM(H10:H17))*100";
            worksheet.Cells[17, 6].Formatter = _percentageFormatter;
            worksheet.Cells[17, 6].StyleName = "GainStyle";
            
            worksheet.Cells[17, 7].Formula = "=SUM(H10:H17)";
            worksheet.Cells[17, 7].Formatter = _trendFormatter;
            worksheet.Cells[17, 7].StyleName = "GainStyle";
            
            worksheet.Cells[17, 8].Formula = "=SUM(I10:I17)";
            worksheet.Cells[17, 8].Formatter = _trendFormatter;
            worksheet.Cells[17, 8].StyleName = "GainStyle";
            
            // Allocation Footer
            worksheet.Cells[17, 10].Value = "Total";
            worksheet.Cells[17, 10].StyleName = "TotalRowLeft";
            
            worksheet.Cells[17, 11].Formula = "=F18";
            worksheet.Cells[17, 11].Formatter = _currencyFormatter;
            worksheet.Cells[17, 11].StyleName = "TotalRow";
            
            worksheet.Cells[17, 12].Value = 100.0;
            worksheet.Cells[17, 12].Formatter = new SimplePercentageFormatter();
            worksheet.Cells[17, 12].StyleName = "TotalRow";
            
            // Bottom Cards
            DrawCard(worksheet, 19, 0, 3, "TOP GAINER (DAY)", "MSFT", "+6.80%", null);
            DrawCard(worksheet, 19, 3, 3, "TOP LOSER (DAY)", "—", "—", null);
            DrawCard(worksheet, 19, 6, 3, "LARGEST POSITION", "MSFT", "26.1%", null);
            DrawCard(worksheet, 19, 10, 2, "CASH (EST.)", "=I18", "10.1%", null);
            DrawCard(worksheet, 19, 12, 1, "POSITIONS", "8", "Stocks", null);
            
            worksheet.Cells[20, 0].StyleName = "CardSubValueGain";
            worksheet.Cells[21, 0].StyleName = "CardSubValueGain";
            
            worksheet.Cells[20, 3].StyleName = "CardSubValueLoss";
            worksheet.Cells[21, 3].StyleName = "CardSubValueLoss";
            
            worksheet.Cells[20, 10].Formatter = _currencyFormatter;
            
            spread.SuspendUpdates = false;
        }
        
        private void DrawCard(IWorksheet ws, int startRow, int startCol, int colSpan, string title, object val, string sub1, string sub2)
        {
            ws.AddSpan(startRow, startCol, 1, colSpan);
            ws.Cells[startRow, startCol].Value = title;
            ws.Cells[startRow, startCol].StyleName = "CardTitle";
            
            ws.AddSpan(startRow + 1, startCol, 1, colSpan);
            if (val is string s && s.StartsWith("=")) ws.Cells[startRow + 1, startCol].Formula = s;
            else ws.Cells[startRow + 1, startCol].Value = val;
            ws.Cells[startRow + 1, startCol].StyleName = "CardValue";
            
            ws.AddSpan(startRow + 2, startCol, 1, colSpan);
            if (sub1 != null && sub1.StartsWith("=")) ws.Cells[startRow + 2, startCol].Formula = sub1;
            else ws.Cells[startRow + 2, startCol].Value = sub1;
            ws.Cells[startRow + 2, startCol].StyleName = "CardSubtext"; // Default, override later if needed
            
            ws.AddSpan(startRow + 3, startCol, 1, colSpan);
            ws.Cells[startRow + 3, startCol].Value = sub2;
            ws.Cells[startRow + 3, startCol].StyleName = "CardSubtext";
        }

        private void UpdateRowStyles(IWorksheet worksheet, int row, StockData stock)
        {
            double diff = stock.CurrentPrice - stock.BasePrice;
            string targetStyle = diff >= 0 ? "GainStyle" : "LossStyle";
            worksheet.Cells[row, 6].StyleName = targetStyle;
            worksheet.Cells[row, 7].StyleName = targetStyle;
            worksheet.Cells[row, 8].StyleName = targetStyle;
            
            // Re-apply allocation simple formatter
            worksheet.Cells[row, 12].Formatter = new SimplePercentageFormatter();
        }

        private void StartLiveFeed()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(400);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _tickCount++;
            spread.SuspendUpdates = true;

            var worksheet = spread.WorkBook.WorkSheets[0];
            if (spread.Sheets.ActiveSheet != spread.Sheets.GetSheetView(worksheet))
                return;

            spread.Sheets.ActiveSheet.AutoSizeRows = false;

            worksheet.Cells[0, 4].Value = $"Ticks: {_tickCount}";
            worksheet.Cells[0, 6].Value = $"Last Update: {DateTime.Now:HH:mm:ss}";

            int numUpdates = _random.Next(3, 6);
            for (int k = 0; k < numUpdates; k++)
            {
                int index = _random.Next(_stocks.Count);
                var stock = _stocks[index];

                double pctChange = (_random.NextDouble() - 0.49) * 0.015;
                stock.CurrentPrice = Math.Round(stock.CurrentPrice * (1 + pctChange), 2);

                int row = 9 + index;
                worksheet.Cells[row, 4].Value = stock.CurrentPrice;

                UpdateRowStyles(worksheet, row, stock);
            }
            
            UpdateBottomCards(worksheet);

            spread.SuspendUpdates = false;
        }
        
        private void UpdateBottomCards(IWorksheet worksheet)
        {
            // Calculate top gainer / loser dynamically
            StockData topGainer = null;
            StockData topLoser = null;
            StockData largest = null;
            double maxGain = -999;
            double maxLoss = 999;
            double maxValue = -1;
            
            foreach(var s in _stocks)
            {
                double pct = (s.CurrentPrice - s.BasePrice) / s.BasePrice * 100;
                double val = s.CurrentPrice * s.Shares;
                
                if (pct > maxGain) { maxGain = pct; topGainer = s; }
                if (pct < maxLoss) { maxLoss = pct; topLoser = s; }
                if (val > maxValue) { maxValue = val; largest = s; }
            }
            
            if (topGainer != null && maxGain > 0)
            {
                worksheet.Cells[20, 0].Value = topGainer.Ticker;
                worksheet.Cells[21, 0].Value = "+" + maxGain.ToString("0.00") + "%";
            }
            if (topLoser != null && maxLoss < 0)
            {
                worksheet.Cells[20, 3].Value = topLoser.Ticker;
                worksheet.Cells[21, 3].Value = maxLoss.ToString("0.00") + "%";
            }
            
            if (largest != null)
            {
                worksheet.Cells[20, 6].Value = largest.Ticker;
                // Allocation is calculated via formula, but for this summary block we will just leave the static layout or compute it.
                // Let's just use formula in cell 21, 6 referencing the specific cell? Too complex.
                // We'll compute it in C#:
                double totalVal = _stocks.Sum(x => x.CurrentPrice * x.Shares);
                double alloc = (maxValue / totalVal) * 100;
                worksheet.Cells[21, 6].Value = alloc.ToString("0.0") + "%";
            }
        }
    }
    
    public class SimplePercentageFormatter : IFormatter
    {
        public string Format(object value)
        {
            if (value is double d) return d.ToString("0.0") + "%";
            return value?.ToString();
        }
    }
}
