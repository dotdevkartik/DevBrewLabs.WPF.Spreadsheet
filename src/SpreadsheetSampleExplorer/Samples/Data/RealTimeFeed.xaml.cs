using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for RealTimeFeed.xaml
    /// Demonstrates live streaming data feeds, real-time formula recalculations, and dynamic conditional cell styling using NamedStyles.
    /// </summary>
    public partial class RealTimeFeed : UserControl
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();
        private int _tickCount = 0;
        private bool _isLive = true;

        private class StockData
        {
            public string Ticker { get; set; }
            public string Company { get; set; }
            public int Shares { get; set; }
            public double BasePrice { get; set; }
            public double CurrentPrice { get; set; }
        }

        private List<StockData> _stocks = new List<StockData>()
        {
            new StockData { Ticker = "MSFT", Company = "Microsoft Corp.", Shares = 250, BasePrice = 415.50, CurrentPrice = 415.50 },
            new StockData { Ticker = "AAPL", Company = "Apple Inc.", Shares = 300, BasePrice = 224.20, CurrentPrice = 224.20 },
            new StockData { Ticker = "NVDA", Company = "NVIDIA Corp.", Shares = 180, BasePrice = 122.80, CurrentPrice = 122.80 },
            new StockData { Ticker = "GOOGL", Company = "Alphabet Inc.", Shares = 200, BasePrice = 175.40, CurrentPrice = 175.40 },
            new StockData { Ticker = "AMZN", Company = "Amazon.com Inc.", Shares = 150, BasePrice = 186.10, CurrentPrice = 186.10 },
            new StockData { Ticker = "TSLA", Company = "Tesla Inc.", Shares = 120, BasePrice = 248.50, CurrentPrice = 248.50 },
            new StockData { Ticker = "META", Company = "Meta Platforms", Shares = 100, BasePrice = 485.30, CurrentPrice = 485.30 }
        };

        public RealTimeFeed()
        {
            InitializeComponent();
            RegisterNamedStyles();
            SetupDashboard();
            StartLiveFeed();

            Unloaded += (s, e) => _timer?.Stop();
        }

        private void RegisterNamedStyles()
        {
            var workBook = spread.WorkBook;

            // Title Style
            if (workBook.GetNamedStyle("TitleStyle") == null)
            {
                var titleStyle = new CellStyle
                {
                    FontSize = 14,
                    FontWeight = CellFontWeight.Bold,
                    ForeColor = CellColor.FromArgb(255, 15, 118, 110)
                };
                workBook.AddNamedStyle("TitleStyle", titleStyle);
            }

            // Table Header Style (Teal Header)
            if (workBook.GetNamedStyle("HeaderStyle") == null)
            {
                var headerStyle = new CellStyle
                {
                    FontWeight = CellFontWeight.Bold,
                    BackColor = CellColor.FromArgb(255, 15, 118, 110),
                    ForeColor = CellColor.White
                };
                workBook.AddNamedStyle("HeaderStyle", headerStyle);
            }

            // Total Label Style
            if (workBook.GetNamedStyle("TotalLabelStyle") == null)
            {
                var totalLabelStyle = new CellStyle
                {
                    FontWeight = CellFontWeight.Bold
                };
                workBook.AddNamedStyle("TotalLabelStyle", totalLabelStyle);
            }

            // Summary Totals Style
            if (workBook.GetNamedStyle("SummaryStyle") == null)
            {
                var summaryStyle = new CellStyle
                {
                    FontWeight = CellFontWeight.Bold,
                    BackColor = CellColor.FromArgb(255, 240, 253, 244),
                    ForeColor = CellColor.FromArgb(255, 15, 118, 110)
                };
                workBook.AddNamedStyle("SummaryStyle", summaryStyle);
            }

            // Real-time Gain Style (Emerald Green)
            if (workBook.GetNamedStyle("GainStyle") == null)
            {
                var gainStyle = new CellStyle
                {
                    FontWeight = CellFontWeight.Bold,
                    ForeColor = CellColor.FromArgb(255, 16, 185, 129),
                    BackColor = CellColor.White
                };
                workBook.AddNamedStyle("GainStyle", gainStyle);
            }

            // Real-time Loss Style (Rose Red)
            if (workBook.GetNamedStyle("LossStyle") == null)
            {
                var lossStyle = new CellStyle
                {
                    FontWeight = CellFontWeight.Bold,
                    ForeColor = CellColor.FromArgb(255, 239, 68, 68),
                    BackColor = CellColor.White
                };
                workBook.AddNamedStyle("LossStyle", lossStyle);
            }
        }

        private void SetupDashboard()
        {
            spread.SuspendUpdates = true;

            var worksheet = spread.WorkBook.WorkSheets[0];
            worksheet.Name = "Live Portfolio Tracker";

            worksheet.Cells[0, 0].Value = "REAL-TIME PORTFOLIO & LIVE MARKET FEED";
            worksheet.Cells[0, 0].StyleName = "TitleStyle";

            string[] headers = { "Ticker", "Company", "Holdings (Qty)", "Prev Close ($)", "Live Price ($)", "Change ($)", "Total Value ($)", "Unrealized P&L ($)" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = worksheet.Cells[3, col]; // Row 4 in Excel
                cell.Value = headers[col];
                cell.StyleName = "HeaderStyle";
            }

            // Populate Initial Stock Data
            for (int i = 0; i < _stocks.Count; i++)
            {
                int row = 4 + i; // Rows 5 to 11 in Excel
                int excelRow = row + 1;
                var stock = _stocks[i];

                worksheet.Cells[row, 0].Value = stock.Ticker;
                worksheet.Cells[row, 1].Value = stock.Company;
                worksheet.Cells[row, 2].Value = stock.Shares;
                worksheet.Cells[row, 3].Value = stock.BasePrice;
                worksheet.Cells[row, 4].Value = stock.CurrentPrice;

                // Dependent Formulas
                // Change ($) = Live Price (E) - Prev Close (D)
                worksheet.Cells[row, 5].Formula = $"=E{excelRow}-D{excelRow}";

                // Total Value ($) = Holdings (C) * Live Price (E)
                worksheet.Cells[row, 6].Formula = $"=C{excelRow}*E{excelRow}";

                // Unrealized P&L ($) = Holdings (C) * Change (F)
                worksheet.Cells[row, 7].Formula = $"=C{excelRow}*F{excelRow}";

                // Apply initial gain/loss style name
                string initialStyle = stock.CurrentPrice >= stock.BasePrice ? "GainStyle" : "LossStyle";
                worksheet.Cells[row, 5].StyleName = initialStyle;
                worksheet.Cells[row, 7].StyleName = initialStyle;
            }

            // Summary Totals Row (Row 13 in Excel -> Row 12)
            int summaryRow = 4 + _stocks.Count + 1; // Row 13 in Excel
            int firstStockRow = 5;
            int lastStockRow = 4 + _stocks.Count;

            worksheet.Cells[summaryRow, 1].Value = "PORTFOLIO TOTALS:";
            worksheet.Cells[summaryRow, 1].StyleName = "TotalLabelStyle";

            // Portfolio Total Value Formula
            worksheet.Cells[summaryRow, 6].Formula = $"=SUM(G{firstStockRow}:G{lastStockRow})";
            worksheet.Cells[summaryRow, 6].StyleName = "SummaryStyle";

            // Total Unrealized P&L Formula
            worksheet.Cells[summaryRow, 7].Formula = $"=SUM(H{firstStockRow}:H{lastStockRow})";
            worksheet.Cells[summaryRow, 7].StyleName = "SummaryStyle";

            spread.SuspendUpdates = false;
        }

        private void StartLiveFeed()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(700);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isLive) return;

            _tickCount++;
            txtStatus.Text = $"Status: Live Streaming (Ticks: {_tickCount})";

            spread.SuspendUpdates = true;

            var worksheet = spread.WorkBook.WorkSheets[0];

            if (spread.SheetViews.ActiveSheetView != spread.SheetViews.GetSheetView(worksheet))
            {
                return;
            }

            // Pick 2-3 random stocks to update price
            int numUpdates = _random.Next(2, 4);
            for (int k = 0; k < numUpdates; k++)
            {
                int index = _random.Next(_stocks.Count);
                var stock = _stocks[index];

                // Fluctuate price by -1.2% to +1.2%
                double pctChange = (_random.NextDouble() - 0.49) * 0.024;
                stock.CurrentPrice = Math.Round(stock.CurrentPrice * (1 + pctChange), 2);

                int row = 4 + index;
                worksheet.Cells[row, 4].Value = stock.CurrentPrice;

                // Apply registered NamedStyles based on Price Change
                double change = Math.Round(stock.CurrentPrice - stock.BasePrice, 2);
                string styleName = change >= 0 ? "GainStyle" : "LossStyle";

                worksheet.Cells[row, 5].StyleName = styleName;
                worksheet.Cells[row, 7].StyleName = styleName;
            }

            spread.SuspendUpdates = false;
        }

        private void OnToggleFeedClicked(object sender, RoutedEventArgs e)
        {
            _isLive = !_isLive;
            if (_isLive)
            {
                btnToggleFeed.Content = "⏸️ Pause Feed";
                txtStatus.Text = $"Status: Live Streaming (Ticks: {_tickCount})";
            }
            else
            {
                btnToggleFeed.Content = "▶️ Resume Feed";
                txtStatus.Text = "Status: Feed Paused";
            }
        }

        private void OnSimulateMarketClicked(object sender, RoutedEventArgs e)
        {
            // Trigger a market-wide shift event
            spread.SuspendUpdates = true;
            var worksheet = spread.WorkBook.WorkSheets[0];

            bool isBullish = _random.Next(2) == 0;
            double multiplier = isBullish ? 1.025 : 0.975;

            for (int i = 0; i < _stocks.Count; i++)
            {
                var stock = _stocks[i];
                stock.CurrentPrice = Math.Round(stock.CurrentPrice * multiplier, 2);

                int row = 4 + i;
                worksheet.Cells[row, 4].Value = stock.CurrentPrice;

                double change = stock.CurrentPrice - stock.BasePrice;
                string styleName = change >= 0 ? "GainStyle" : "LossStyle";

                worksheet.Cells[row, 5].StyleName = styleName;
                worksheet.Cells[row, 7].StyleName = styleName;
            }

            spread.SuspendUpdates = false;
            _tickCount++;
            txtStatus.Text = $"Status: Market Shift Triggered ({ (isBullish ? "+2.5% Surge" : "-2.5% Dip") })";
        }
    }
}


