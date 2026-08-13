using SpreadsheetSampleExplorer.Models;
using SpreadsheetSampleExplorer.Samples;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SampleItem _currentSampleItem;

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            InitializeComponent();
            DisplayRuntimeInfo();
            RegisterAllSamples();
        }

        private void DisplayRuntimeInfo()
        {
            try
            {
                string desc = RuntimeInformation.FrameworkDescription;
                _txtRuntimeInfo.Text = $"Runtime: {desc}";
            }
            catch
            {
                _txtRuntimeInfo.Text = "Engine Status: Ready";
            }
        }

        private void RegisterAllSamples()
        {
            // Category: ESSENTIALS
            _samplesSideBar.RegisterSample(
                "Testing",
                "Formula Bar & Editor",
                "Interactive formula bar linked with spreadsheet cell selection.",
                "ESSENTIALS",
                "M3,17.25V21h3.75L17.81,9.94l-3.75-3.75L3,17.25z M20.71,7.04c0.39-0.39,0.39-1.02,0-1.41l-2.34-2.34c-0.39-0.39-1.02-0.39-1.41,0l-1.83,1.83l3.75,3.75L20.71,7.04z",
                typeof(Testing)
            );

            _samplesSideBar.RegisterSample(
                "CellTypes",
                "Custom Cell Renderers",
                "Demonstration of Checkbox, Button, ComboBox, and Text cell types.",
                "ESSENTIALS",
                "M3 3h8v8H3V3zm10 0h8v8h-8V3zM3 13h8v8H3v-8zm10 0h8v8h-8v-8z",
                typeof(CellTypes)
            );

            _samplesSideBar.RegisterSample(
                "Performance",
                "Performance",
                "Benchmark dataset load times, rendering speed, and compare scroll modes.",
                "ESSENTIALS",
                "M9 3L5 7h3v7h2V7h3L9 3zm6 18l4-4h-3V10h-2v7h-3l4 4z",
                typeof(Performance)
            );

            _samplesSideBar.RegisterSample(
                "Sorting",
                "Data Sorting Engine",
                "Fast ascending and descending multi-range data sorting.",
                "ESSENTIALS",
                "M3 18h6v-2H3v2zM3 6v2h18V6H3zm0 7h12v-2H3v2z",
                typeof(Sorting)
            );

            // Category: DATA & CALCULATIONS
            _samplesSideBar.RegisterSample(
                "Formulas",
                "Multi-Sheet Formulas",
                "Cross-worksheet formula engine with real-time dependency recalculation.",
                "DATA & CALCULATIONS",
                "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 14h-2v-4H8v-2h2V9h2v2h2v2h-2v4z",
                typeof(Formulas)
            );

            _samplesSideBar.RegisterSample(
                "DataBinding",
                "List & DataTable Binding",
                "Automatic two-way binding to custom C# object lists and DataTables.",
                "DATA & CALCULATIONS",
                "M12 3C7.58 3 4 4.79 4 7v10c0 2.21 3.58 4 8 4s8-1.79 8-4V7c0-2.21-3.58-4-8-4zm0 5c-3.87 0-7-1.34-7-3s3.13-3 7-3 7 1.34 7 3-3.13 3-7 3z",
                typeof(DataBinding)
            );

            _samplesSideBar.RegisterSample(
                "PortfolioDashboard",
                "Portfolio Dashboard",
                "Industry-grade mesmerising dashboard with custom formatters, spanning, and live streams.",
                "DATA & CALCULATIONS",
                "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 14h-2v-4H8v-2h2V9h2v2h2v2h-2v4z",
                typeof(PortfolioDashboard)
            );

            // Category: APPEARANCE & STYLING
            _samplesSideBar.RegisterSample(
                "Spanning",
                "Cell Spanning",
                "Merge and unmerge cells, spanning multiple rows and columns.",
                "APPEARANCE & STYLING",
                "M3 3h18v18H3V3zm16 16V5H5v14h14zm-4-4H9v-4h6v4z",
                typeof(Spanning)
            );

            _samplesSideBar.RegisterSample(
                "Styling",
                "Grid Styling & Themes",
                "Custom cell background colors, borders, fonts, and gridlines.",
                "APPEARANCE & STYLING",
                "M12 3c-4.97 0-9 4.03-9 9 0 2.12.74 4.07 1.97 5.61L4.35 19.4c-.39.39-.39 1.02 0 1.41.39.39 1.02.39 1.41 0l2.25-2.25C9.56 18.8 10.74 19 12 19c4.97 0 9-4.03 9-9s-4.03-9-9-9z",
                typeof(Styling)
            );

            _samplesSideBar.RegisterSample(
                "Zooming",
                "Worksheet Zooming",
                "Zoom in and out on active worksheet using slider, presets, or Ctrl+MouseWheel.",
                "APPEARANCE & STYLING",
                "M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14zm2.5-4h-2v2H9v-2H7V9h2V7h1v2h2v1z",
                typeof(Zooming)
            );
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void OnSampleSelected(object sender, SampleSelectedEventArgs e)
        {
            if (e.SampleItem == null || e.Sample == null)
                return;

            _currentSampleItem = e.SampleItem;
            _txtSampleTitle.Text = e.SampleItem.Title;
            _txtSampleDesc.Text = e.SampleItem.Description;
            _txtCategory.Text = e.SampleItem.Category.ToUpper();

            if (!string.IsNullOrEmpty(e.SampleItem.IconData))
            {
                try
                {
                    _sampleIconPath.Data = Geometry.Parse(e.SampleItem.IconData);
                }
                catch { }
            }

            LoadCurrentSample();
        }

        private void LoadCurrentSample()
        {
            if (_currentSampleItem?.SampleType == null)
                return;

            if (_samplesViewerBdr.Child != null)
                _samplesViewerBdr.Child = null;

            _samplesViewerBdr.Child = (FrameworkElement)Activator.CreateInstance(_currentSampleItem.SampleType);
        }

        private void OnReloadSampleClicked(object sender, RoutedEventArgs e)
        {
            LoadCurrentSample();
        }
    }
}
