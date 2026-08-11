using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for Zooming.xaml
    /// </summary>
    public partial class Zooming : UserControl
    {
        public Zooming()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            PopulateSampleData();
            spread.ZoomChanged += OnZoomChanged;
            UpdateZoomText(spread.ZoomFactor);
        }

        private void PopulateSampleData()
        {
            var sheet = spread.WorkBook.WorkSheets.ActiveSheet;
            sheet.ColumnCount = 100;
            sheet.RowCount = 200;

            sheet.SetValue(0, 0, "Quarter");
            sheet.SetValue(0, 1, "Region");
            sheet.SetValue(0, 2, "Sales Rep");
            sheet.SetValue(0, 3, "Revenue ($)");
            sheet.SetValue(0, 4, "Target ($)");
            sheet.SetValue(0, 5, "Status");

            string[] regions = { "North", "South", "East", "West" };
            string[] reps = { "Alice Smith", "Bob Jones", "Carol Vance", "David Miller", "Eva Green" };

            Random rand = new Random(42);
            for (int r = 1; r <= 20; r++)
            {
                sheet.SetValue(r, 0, $"Q{(r % 4) + 1}");
                sheet.SetValue(r, 1, regions[rand.Next(regions.Length)]);
                sheet.SetValue(r, 2, reps[rand.Next(reps.Length)]);
                int rev = rand.Next(15000, 95000);
                int tgt = rand.Next(20000, 80000);
                sheet.SetValue(r, 3, rev);
                sheet.SetValue(r, 4, tgt);
                sheet.SetValue(r, 5, rev >= tgt ? "Met Target" : "Under Target");
            }

            spread.Invalidate();
        }

        private void OnZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            UpdateZoomText(e.NewZoomFactor);
        }

        private void UpdateZoomText(double zoomFactor)
        {
            if (_txtZoomPercent != null)
            {
                _txtZoomPercent.Text = $"{(int)(Math.Round(zoomFactor, 2) * 100)}%";
            }
        }

        private void OnZoomInClicked(object sender, RoutedEventArgs e)
        {
            spread.ZoomFactor = Math.Min(4.0, Math.Round(spread.ZoomFactor + 0.1, 2));
        }

        private void OnZoomOutClicked(object sender, RoutedEventArgs e)
        {
            spread.ZoomFactor = Math.Max(0.1, Math.Round(spread.ZoomFactor - 0.1, 2));
        }

        private void OnPresetZoomClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && double.TryParse(btn.Tag.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double targetZoom))
            {
                spread.ZoomFactor = targetZoom;
            }
        }
    }
}
