using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for SliderSample.xaml showcasing interactive in-cell draggable sliders
    /// driving a real-time financial mortgage and loan scenario simulator.
    /// </summary>
    public partial class SliderSample : UserControl
    {
        private SliderCellType _priceSlider;
        private SliderCellType _downPaymentSlider;
        private SliderCellType _rateSlider;
        private SliderCellType _termSlider;

        public SliderSample()
        {
            InitializeComponent();
            SetupSheet(spread.Sheets.ActiveSheet);

            spread.CellEditEnded += (s, e) =>
            {
                spread.InvalidateVisual();
            };
        }

        private void SetupSheet(ISheetView sheetView)
        {
            var worksheet = sheetView.WorkSheet;
            worksheet.RowCount = 12;
            worksheet.ColumnCount = 4;

            // 1. Configure Row Heights
            worksheet.ColumnHeaders.Rows[0].Height = 32;
            for (int r = 0; r < 12; r++)
            {
                worksheet.Rows[r].Height = (r == 8) ? 42 : 34; // Give primary Monthly Payment KPI generous height
            }

            // 2. Configure Column Widths
            worksheet.Columns[0].Width = 220; // Parameter Label
            worksheet.Columns[1].Width = 280; // Interactive Slider
            worksheet.Columns[2].Width = 140; // Formatted Value
            worksheet.Columns[3].Width = 290; // Context Note

            // Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Financial Metric / Parameter";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Interactive Slider Control";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Calculated Amount";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Notes & Sensitivity Context";

            // 3. Setup Custom Sliders
            // Home Purchase Price Slider (Azure Blue)
            _priceSlider = new SliderCellType
            {
                Minimum = 50000.0,
                Maximum = 1500000.0,
                Step = 10000.0,
                ShowValue = true,
                ValueFormat = "${0:N0}",
                FillBrush = SheetUtils.CreateFrozenBrush("#2563EB"),
                ThumbBorderBrush = SheetUtils.CreateFrozenBrush("#2563EB"),
                ShowTicks = true,
                TickFrequency = 250000.0,
                BarMargin = 6.0
            };
            _priceSlider.ValueChanged += OnSliderValueChanged;

            // Down Payment Rate Slider (Emerald Green)
            _downPaymentSlider = new SliderCellType
            {
                Minimum = 0.0,
                Maximum = 50.0,
                Step = 1.0,
                ShowValue = true,
                ValueFormat = "{0:0}%",
                FillBrush = SheetUtils.CreateFrozenBrush("#059669"),
                ThumbBorderBrush = SheetUtils.CreateFrozenBrush("#059669"),
                ShowTicks = true,
                TickFrequency = 10.0,
                BarMargin = 6.0
            };
            _downPaymentSlider.ValueChanged += OnSliderValueChanged;

            // Interest Rate APR Slider (Warm Amber)
            _rateSlider = new SliderCellType
            {
                Minimum = 2.0,
                Maximum = 12.0,
                Step = 0.1,
                ShowValue = true,
                ValueFormat = "{0:0.0}%",
                FillBrush = SheetUtils.CreateFrozenBrush("#D97706"),
                ThumbBorderBrush = SheetUtils.CreateFrozenBrush("#D97706"),
                ShowTicks = true,
                TickFrequency = 2.0,
                BarMargin = 6.0
            };
            _rateSlider.ValueChanged += OnSliderValueChanged;

            // Loan Term Slider (Royal Indigo with 5-year step ticks)
            _termSlider = new SliderCellType
            {
                Minimum = 5.0,
                Maximum = 30.0,
                Step = 5.0,
                ShowValue = true,
                ValueFormat = "{0:0} yrs",
                FillBrush = SheetUtils.CreateFrozenBrush("#4F46E5"),
                ThumbBorderBrush = SheetUtils.CreateFrozenBrush("#4F46E5"),
                ShowTicks = true,
                TickFrequency = 5.0,
                BarMargin = 6.0
            };
            _termSlider.ValueChanged += OnSliderValueChanged;

            // 4. Assign Cell Types
            worksheet.Cells[1, 1].CellType = _priceSlider;
            worksheet.Cells[2, 1].CellType = _downPaymentSlider;
            worksheet.Cells[3, 1].CellType = _rateSlider;
            worksheet.Cells[4, 1].CellType = _termSlider;

            // 5. Setup Initial Values & Labels
            worksheet.Cells[0, 0].Value = "LOAN PARAMETERS";
            worksheet.Cells[0, 1].Value = "(Drag sliders to adjust)";
            worksheet.Cells[0, 2].Value = "";
            worksheet.Cells[0, 3].Value = "";

            worksheet.Cells[1, 0].Value = "Home Purchase Price";
            worksheet.Cells[1, 1].Value = 450000.0;
            worksheet.Cells[1, 2].Formula = "=B2";
            worksheet.Cells[1, 2].Formatter = new NumberFormatCellFormatter("C0");
            worksheet.Cells[1, 3].Value = "=B2  [Acquisition cost $50k – $1.5M]";

            worksheet.Cells[2, 0].Value = "Down Payment Rate";
            worksheet.Cells[2, 1].Value = 20.0;
            worksheet.Cells[2, 2].Formula = "=B3/100";
            worksheet.Cells[2, 2].Formatter = new NumberFormatCellFormatter("P0");
            worksheet.Cells[2, 3].Value = "=B3/100  [>= 20% waives PMI]";

            worksheet.Cells[3, 0].Value = "Annual Interest Rate (APR)";
            worksheet.Cells[3, 1].Value = 6.5;
            worksheet.Cells[3, 2].Formula = "=B4/100";
            worksheet.Cells[3, 2].Formatter = new NumberFormatCellFormatter("P1");
            worksheet.Cells[3, 3].Value = "=B4/100  [Fixed-rate benchmark]";

            worksheet.Cells[4, 0].Value = "Loan Term (Duration)";
            worksheet.Cells[4, 1].Value = 30.0;
            worksheet.Cells[4, 2].Formula = "=B5";
            worksheet.Cells[4, 2].Formatter = new NumberFormatCellFormatter("0", " Years");
            worksheet.Cells[4, 3].Value = "=B5  [Standard 30-yr or 15-yr amortizations]";

            worksheet.Cells[5, 0].Value = "REAL-TIME AMORTIZATION SUMMARY";
            worksheet.Cells[5, 1].Value = "";
            worksheet.Cells[5, 2].Value = "";
            worksheet.Cells[5, 3].Value = "";

            // 6. Amortization Summary: Real-Time Spreadsheet Formulas
            // Down Payment Amount: =B2*(B3/100)
            worksheet.Cells[6, 0].Value = "Down Payment Amount";
            worksheet.Cells[6, 2].Formula = "=B2*(B3/100)";
            worksheet.Cells[6, 2].Formatter = new NumberFormatCellFormatter("C0");
            worksheet.Cells[6, 3].Value = "=B2*(B3/100)  [Upfront cash equity]";

            // Net Loan Principal: =B2-C7
            worksheet.Cells[7, 0].Value = "Net Loan Principal";
            worksheet.Cells[7, 2].Formula = "=B2-C7";
            worksheet.Cells[7, 2].Formatter = new NumberFormatCellFormatter("C0");
            worksheet.Cells[7, 3].Value = "=B2-C7  [Financed principal borrowing]";

            // Estimated Monthly Payment: =(C8*(B4/1200)*POWER(1+B4/1200,B5*12))/(POWER(1+B4/1200,B5*12)-1)
            worksheet.Cells[8, 0].Value = "Estimated Monthly Payment";
            worksheet.Cells[8, 2].Formula = "=(C8*(B4/1200)*POWER(1+B4/1200,B5*12))/(POWER(1+B4/1200,B5*12)-1)";
            worksheet.Cells[8, 2].Formatter = new NumberFormatCellFormatter("C2", " / mo");
            worksheet.Cells[8, 3].Value = "=(C8*(B4/1200)*POWER(...))/(POWER(...)-1)";

            // Total Lifetime Interest: =C11-C8
            worksheet.Cells[9, 0].Value = "Total Lifetime Interest";
            worksheet.Cells[9, 2].Formula = "=C11-C8";
            worksheet.Cells[9, 2].Formatter = new NumberFormatCellFormatter("C0");
            worksheet.Cells[9, 3].Value = "=C11-C8  [Cumulative interest paid to lender]";

            // Total Overall Loan Cost: =C9*B5*12
            worksheet.Cells[10, 0].Value = "Total Overall Loan Cost";
            worksheet.Cells[10, 2].Formula = "=C9*B5*12";
            worksheet.Cells[10, 2].Formatter = new NumberFormatCellFormatter("C0");
            worksheet.Cells[10, 3].Value = "=C9*B5*12  [Total principal + interest repaid]";
        }

        private void OnSliderValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            spread?.InvalidateVisual();

            if (statusTextBlock != null)
            {
                string metricName = (e.Row == 1) ? "Purchase Price" :
                                    (e.Row == 2) ? "Down Payment" :
                                    (e.Row == 3) ? "Interest Rate" : "Loan Term";
                var monthlyVal = spread.Sheets.ActiveSheet.WorkSheet.GetValue(8, 2);
                string monthlyStr = (monthlyVal is double d) ? d.ToString("C2", CultureInfo.CurrentCulture) + " / mo" : monthlyVal?.ToString();
                statusTextBlock.Text = $"🎚️ Adjusted {metricName}: {e.Value:N1} ➔ Formula Payment: {monthlyStr}";
            }
        }

        private void OnPresetClicked(object sender, RoutedEventArgs e)
        {
            var worksheet = spread.Sheets.ActiveSheet.WorkSheet;

            if (sender == btnStarterHome)
            {
                worksheet.SetValue(1, 1, 320000.0);
                worksheet.SetValue(2, 1, 15.0);
                worksheet.SetValue(3, 1, 6.2);
                worksheet.SetValue(4, 1, 30.0);
            }
            else if (sender == btnLuxuryHome)
            {
                worksheet.SetValue(1, 1, 1250000.0);
                worksheet.SetValue(2, 1, 25.0);
                worksheet.SetValue(3, 1, 5.8);
                worksheet.SetValue(4, 1, 30.0);
            }
            else if (sender == btnCommercial)
            {
                worksheet.SetValue(1, 1, 850000.0);
                worksheet.SetValue(2, 1, 20.0);
                worksheet.SetValue(3, 1, 7.5);
                worksheet.SetValue(4, 1, 20.0);
            }
            else if (sender == btnAutoLoan)
            {
                worksheet.SetValue(1, 1, 45000.0);
                worksheet.SetValue(2, 1, 10.0);
                worksheet.SetValue(3, 1, 4.9);
                worksheet.SetValue(4, 1, 5.0);
            }

            spread?.InvalidateVisual();

            if (statusTextBlock != null && sender is Button btn)
            {
                statusTextBlock.Text = $"⚡ Applied Preset '{btn.Content}' (Formulas recalculated automatically)";
            }
        }

        private void OnOptionChanged(object sender, RoutedEventArgs e)
        {
            if (_priceSlider == null || _downPaymentSlider == null || _rateSlider == null || _termSlider == null) return;

            bool showTicks = chkShowTicks?.IsChecked == true;
            bool isReadOnly = chkReadOnly?.IsChecked == true;

            _priceSlider.ShowTicks = showTicks;
            _downPaymentSlider.ShowTicks = showTicks;
            _rateSlider.ShowTicks = showTicks;
            _termSlider.ShowTicks = showTicks;

            _priceSlider.IsReadOnly = isReadOnly;
            _downPaymentSlider.IsReadOnly = isReadOnly;
            _rateSlider.IsReadOnly = isReadOnly;
            _termSlider.IsReadOnly = isReadOnly;

            spread?.InvalidateVisual();
        }
    }

    /// <summary>
    /// Custom cell formatter applying standard numeric string formatting and optional suffixes.
    /// </summary>
    public class NumberFormatCellFormatter : IFormatter
    {
        private readonly string _format;
        private readonly string _suffix;

        public NumberFormatCellFormatter(string format, string suffix = "")
        {
            _format = format;
            _suffix = suffix;
        }

        public string Format(object value)
        {
            if (value == null) return string.Empty;
            if (value is double d) return d.ToString(_format, CultureInfo.CurrentCulture) + _suffix;
            if (value is float f) return f.ToString(_format, CultureInfo.CurrentCulture) + _suffix;
            if (value is int i) return i.ToString(_format, CultureInfo.CurrentCulture) + _suffix;
            if (value is decimal m) return m.ToString(_format, CultureInfo.CurrentCulture) + _suffix;

            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed) ||
                double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return parsed.ToString(_format, CultureInfo.CurrentCulture) + _suffix;
            }

            return value.ToString();
        }
    }
}
