using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using System;
using System.Windows;
using System.Windows.Controls;
using SpreadsheetSampleExplorer.Data;

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

            var data = DataSource.GetZoomingData(21, 6);
            sheet.Load(data);

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
