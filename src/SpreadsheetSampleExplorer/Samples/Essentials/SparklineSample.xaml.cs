using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for SparklineSample.xaml showcasing Line, Area, Column, and Win/Loss micro-charts
    /// in an executive KPI performance dashboard.
    /// </summary>
    public partial class SparklineSample : UserControl
    {
        private SparklineCellType _lineSparkline;
        private SparklineCellType _areaSparkline;
        private SparklineCellType _columnSparkline;
        private SparklineCellType _winLossSparkline;

        private Random _random = new Random(42);

        public SparklineSample()
        {
            InitializeComponent();
            SetupSheet(spread.Sheets.ActiveSheet);
        }

        private void SetupSheet(ISheetView sheetView)
        {
            var worksheet = sheetView.WorkSheet;
            int totalRows = 200;
            worksheet.RowCount = totalRows;
            worksheet.ColumnCount = 8;

            // 1. Configure Generous Row Heights (36px makes sparklines look sharp and legible)
            worksheet.ColumnHeaders.Rows[0].Height = 34;
            for (int r = 0; r < totalRows; r++)
            {
                worksheet.Rows[r].Height = 36;
            }

            // 2. Initialize Themed Sparkline Cell Types
            // 7-Day Line Trend (Azure with Emerald High Point and Crimson Low Point)
            _lineSparkline = new SparklineCellType
            {
                Type = SparklineType.Line,
                SeriesBrush = SheetUtils.CreateFrozenBrush("#2563EB"),     // Blue-600
                HighPointBrush = SheetUtils.CreateFrozenBrush("#16A34A"),  // Emerald-600
                LowPointBrush = SheetUtils.CreateFrozenBrush("#DC2626"),   // Red-600
                LastPointBrush = SheetUtils.CreateFrozenBrush("#1D4ED8"),  // Dark Blue
                ShowHighPoint = true,
                ShowLowPoint = true,
                ShowLastPoint = true,
                ShowZeroAxis = true,
                LineThickness = 1.8
            };

            // 30-Day Area Volume (Emerald with soft translucent fill)
            _areaSparkline = new SparklineCellType
            {
                Type = SparklineType.Area,
                SeriesBrush = SheetUtils.CreateFrozenBrush("#059669"),     // Emerald-600
                AreaBrush = SheetUtils.CreateFrozenBrush(System.Windows.Media.Color.FromArgb(45, 16, 185, 129)),
                HighPointBrush = SheetUtils.CreateFrozenBrush("#047857"),  // Emerald-700
                LowPointBrush = SheetUtils.CreateFrozenBrush("#F87171"),   // Light Red
                ShowHighPoint = true,
                ShowLowPoint = true,
                ShowZeroAxis = true,
                LineThickness = 1.6
            };

            // Daily Variance (Column micro-bars with Green positive and Red negative)
            _columnSparkline = new SparklineCellType
            {
                Type = SparklineType.Column,
                SeriesBrush = SheetUtils.CreateFrozenBrush("#10B981"),     // Emerald for positive
                NegativeBrush = SheetUtils.CreateFrozenBrush("#EF4444"),   // Red for negative
                HighPointBrush = SheetUtils.CreateFrozenBrush("#059669"),
                LowPointBrush = SheetUtils.CreateFrozenBrush("#DC2626"),
                ShowHighPoint = true,
                ShowLowPoint = true,
                ShowNegativePoints = true,
                ShowZeroAxis = true
            };

            // Target Win / Loss (Binary milestone indicators)
            _winLossSparkline = new SparklineCellType
            {
                Type = SparklineType.WinLoss,
                SeriesBrush = SheetUtils.CreateFrozenBrush("#6366F1"),     // Indigo-500 (Win)
                NegativeBrush = SheetUtils.CreateFrozenBrush("#F43F5E")    // Rose-500 (Loss)
            };

            // 3. Configure Columns
            worksheet.Columns[0].CellType = new TextCellType();
            worksheet.Columns[0].Width = 140;

            worksheet.Columns[1].CellType = new TextCellType();
            worksheet.Columns[1].Width = 195;

            worksheet.Columns[2].CellType = new NumberCellType { Format = "#,##0.0" };
            worksheet.Columns[2].Width = 100;

            worksheet.Columns[3].CellType = _lineSparkline;
            worksheet.Columns[3].Width = 145;

            worksheet.Columns[4].CellType = _areaSparkline;
            worksheet.Columns[4].Width = 145;

            worksheet.Columns[5].CellType = _columnSparkline;
            worksheet.Columns[5].Width = 135;

            worksheet.Columns[6].CellType = _winLossSparkline;
            worksheet.Columns[6].Width = 125;

            worksheet.Columns[7].CellType = new TextCellType();
            worksheet.Columns[7].Width = 105;

            // 4. Set Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Division";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Core KPI Metric";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Current";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "7D Trend (Line)";
            worksheet.ColumnHeaders.Cells[0, 4].Value = "30D Volume (Area)";
            worksheet.ColumnHeaders.Cells[0, 5].Value = "Daily Variance";
            worksheet.ColumnHeaders.Cells[0, 6].Value = "SLA Target";
            worksheet.ColumnHeaders.Cells[0, 7].Value = "Change %";

            // 5. Populate Data
            PopulateKpiData(worksheet, "normal");
        }

        private void PopulateKpiData(IWorksheet worksheet, string pattern)
        {
            var templates = new[]
            {
                new { Division = "Cloud Infra", Metric = "Active Instances", Current = 1420.0, Base = 1200.0, Volatility = 0.08, IsGain = true },
                new { Division = "FinTech", Metric = "ARR Run-Rate ($M)", Current = 28.5, Base = 22.0, Volatility = 0.06, IsGain = true },
                new { Division = "Platform", Metric = "API Latency P99 (ms)", Current = 42.5, Base = 58.0, Volatility = 0.12, IsGain = false },
                new { Division = "AI Services", Metric = "GPU Cluster Load (%)", Current = 88.4, Base = 70.0, Volatility = 0.09, IsGain = true },
                new { Division = "SaaS Growth", Metric = "Net Retention Rate (%)", Current = 118.2, Base = 105.0, Volatility = 0.05, IsGain = true },
                new { Division = "Operations", Metric = "Customer Churn (%)", Current = 1.15, Base = 2.4, Volatility = 0.15, IsGain = false },
                new { Division = "E-Commerce", Metric = "Gross GMV Volume ($M)", Current = 64.2, Base = 50.0, Volatility = 0.07, IsGain = true },
                new { Division = "Security", Metric = "Intrusion Blocks (k)", Current = 94.6, Base = 80.0, Volatility = 0.11, IsGain = true },
                new { Division = "Core Engine", Metric = "Memory Usage (%)", Current = 71.3, Base = 85.0, Volatility = 0.08, IsGain = false },
                new { Division = "Support", Metric = "Unresolved Escalations", Current = 8.0, Base = 24.0, Volatility = 0.18, IsGain = false },
                new { Division = "Billing", Metric = "Failed Invoices", Current = 3.0, Base = 12.0, Volatility = 0.20, IsGain = false },
                new { Division = "Data Eng", Metric = "Pipeline Throughput (TB)", Current = 485.0, Base = 390.0, Volatility = 0.06, IsGain = true }
            };

            int totalRows = worksheet.RowCount;
            var data = new object[totalRows, 8];

            for (int r = 0; r < totalRows; r++)
            {
                var tmpl = templates[r % templates.Length];
                string div = tmpl.Division;
                string name = (r < templates.Length) ? tmpl.Metric : $"{tmpl.Metric} (Cluster {r / templates.Length + 1})";

                // Generate 8-point 7D Line Trend Series
                double[] lineSeries = GenerateSeries(tmpl.Base, tmpl.Current, 8, tmpl.Volatility, pattern);

                // Generate 12-point 30D Area Volume Series
                double[] areaSeries = GenerateSeries(tmpl.Base * 0.9, tmpl.Current, 12, tmpl.Volatility * 0.8, pattern);

                // Generate 8-point Daily Variance Series (positive & negative swings)
                double[] varianceSeries = GenerateVariance(lineSeries);

                // Generate 8-point Win/Loss Series (+1 for target met, -1 for missed)
                double[] winLossSeries = GenerateWinLoss(lineSeries, tmpl.Base);

                // Calculate change percentage
                double startVal = lineSeries[0];
                double endVal = lineSeries[lineSeries.Length - 1];
                double changePct = (startVal != 0) ? (endVal - startVal) / startVal * 100.0 : 0.0;
                string changeStr = (changePct >= 0) ? $"+{changePct:F1}%" : $"{changePct:F1}%";

                data[r, 0] = div;
                data[r, 1] = name;
                data[r, 2] = Math.Round(endVal, 1);
                data[r, 3] = lineSeries;
                data[r, 4] = areaSeries;
                data[r, 5] = varianceSeries;
                data[r, 6] = winLossSeries;
                data[r, 7] = changeStr;
            }

            worksheet.Load(data, 0, 0);
        }

        private double[] GenerateSeries(double startVal, double endVal, int points, double vol, string pattern)
        {
            var res = new double[points];
            res[0] = startVal;

            for (int i = 1; i < points - 1; i++)
            {
                double t = (double)i / (points - 1);
                double baseVal = startVal + (endVal - startVal) * t;

                switch (pattern.ToLowerInvariant())
                {
                    case "bullish":
                        // Upward curve with compounding trajectory
                        double growth = Math.Pow(1.0 + t, 1.5) - 1.0;
                        baseVal = startVal + (endVal - startVal) * (t + 0.3 * growth);
                        double jitterB = (_random.NextDouble() - 0.35) * vol * baseVal;
                        res[i] = Math.Round(Math.Max(1.0, baseVal + jitterB), 2);
                        break;

                    case "bearish":
                        // Downward dip
                        double decline = Math.Pow(1.0 - t, 1.4);
                        baseVal = endVal + (startVal - endVal) * decline;
                        double jitterD = (_random.NextDouble() - 0.65) * vol * baseVal;
                        res[i] = Math.Round(Math.Max(1.0, baseVal + jitterD), 2);
                        break;

                    case "cyclical":
                        // Sine-wave seasonal fluctuation
                        double wave = Math.Sin(t * Math.PI * 2.5) * 0.25 * baseVal;
                        res[i] = Math.Round(Math.Max(1.0, baseVal + wave), 2);
                        break;

                    case "volatile":
                        // Erratic high/low swings
                        double shock = (_random.NextDouble() - 0.5) * vol * 2.5 * baseVal;
                        res[i] = Math.Round(Math.Max(1.0, baseVal + shock), 2);
                        break;

                    default:
                        // Normal realistic random walk
                        double jitter = (_random.NextDouble() - 0.48) * vol * baseVal;
                        res[i] = Math.Round(Math.Max(1.0, baseVal + jitter), 2);
                        break;
                }
            }

            res[points - 1] = endVal;
            return res;
        }

        private double[] GenerateVariance(double[] series)
        {
            var varSeries = new double[series.Length - 1];
            for (int i = 0; i < series.Length - 1; i++)
            {
                varSeries[i] = Math.Round(series[i + 1] - series[i], 2);
            }
            return varSeries;
        }

        private double[] GenerateWinLoss(double[] series, double target)
        {
            var wl = new double[series.Length];
            for (int i = 0; i < series.Length; i++)
            {
                wl[i] = series[i] >= target ? 1.0 : -1.0;
            }
            return wl;
        }

        private void OnPatternClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string pattern = "normal";
                if (btn == btnBullish) pattern = "bullish";
                else if (btn == btnBearish) pattern = "bearish";
                else if (btn == btnCyclical) pattern = "cyclical";
                else if (btn == btnVolatile) pattern = "volatile";
                else if (btn == btnRandomize) pattern = "normal";

                PopulateKpiData(spread.Sheets.ActiveSheet.WorkSheet, pattern);

                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"⚡ Applied '{btn.Content}' trend simulation across all in-cell sparklines!";
                }

                spread?.InvalidateVisual();
            }
        }

        private void OnOptionChanged(object sender, RoutedEventArgs e)
        {
            if (_lineSparkline == null || _areaSparkline == null || _columnSparkline == null) return;

            bool showHighLow = chkHighLowPoints?.IsChecked == true;
            bool showMarkers = chkShowMarkers?.IsChecked == true;
            bool showZeroAxis = chkZeroAxis?.IsChecked == true;
            double thickness = chkThickLines?.IsChecked == true ? 2.6 : 1.8;

            _lineSparkline.ShowHighPoint = showHighLow;
            _lineSparkline.ShowLowPoint = showHighLow;
            _lineSparkline.ShowMarkers = showMarkers;
            _lineSparkline.ShowZeroAxis = showZeroAxis;
            _lineSparkline.LineThickness = thickness;

            _areaSparkline.ShowHighPoint = showHighLow;
            _areaSparkline.ShowLowPoint = showHighLow;
            _areaSparkline.ShowMarkers = showMarkers;
            _areaSparkline.ShowZeroAxis = showZeroAxis;
            _areaSparkline.LineThickness = thickness;

            _columnSparkline.ShowHighPoint = showHighLow;
            _columnSparkline.ShowLowPoint = showHighLow;
            _columnSparkline.ShowZeroAxis = showZeroAxis;

            spread?.InvalidateVisual();
        }
    }
}
