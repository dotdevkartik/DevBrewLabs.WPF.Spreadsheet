using DevBrewLabs.WPF.Spreadsheet;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer.Samples
{
    public partial class SpreadProperties : UserControl
    {
        public SpreadProperties()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            PopulateSampleData();
            
            _cmbScrollMode.SelectedIndex = 0;
            _cmbSelectionBackground.SelectedIndex = 0;
            _cmbGridLineBrush.SelectedIndex = 0;
            _cmbSelectionBorder.SelectedIndex = 0;
        }

        private void PopulateSampleData()
        {
            var sheet = spread.WorkBook.WorkSheets.ActiveSheet;

            try
            {
                // 1. Tell the UI to ignore updates temporarily
                spread.SuspendUpdates = true;

                sheet.ColumnCount = 20;
                sheet.RowCount = 50;

                object[,] data = new object[20, 10];
                for (int r = 0; r < 20; r++)
                {
                    for (int c = 0; c < 10; c++)
                    {
                        data[r, c] = $"Data {r},{c}";
                    }
                }

                sheet.Load(data);
            }
            finally
            {
                // 2. Turn updates back on. This will trigger ONE massive layout pass 
                // instead of 200 tiny ones.
                spread.SuspendUpdates = false;
            }
        }

        private void OnScrollModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spread == null) return;
            switch (_cmbScrollMode.SelectedIndex)
            {
                case 0: spread.ScrollMode = SheetScrollMode.Item; break;
                case 1: spread.ScrollMode = SheetScrollMode.Pixel; break;
                case 2: spread.ScrollMode = SheetScrollMode.Deferred; break;
            }
        }

        private void OnSelectionBackgroundChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spread == null) return;
            switch (_cmbSelectionBackground.SelectedIndex)
            {
                case 0: spread.SelectionBackground = new SolidColorBrush(Color.FromArgb(50, 25, 25, 25)); break;
                case 1: spread.SelectionBackground = new SolidColorBrush(Color.FromArgb(50, 0, 120, 215)); break;
                case 2: spread.SelectionBackground = new SolidColorBrush(Color.FromArgb(50, 16, 124, 65)); break;
            }
        }

        private void OnGridLineBrushChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spread == null) return;
            switch (_cmbGridLineBrush.SelectedIndex)
            {
                case 0: spread.GridLineBrush = new SolidColorBrush(Color.FromArgb(255, 212, 212, 212)); break;
                case 1: spread.GridLineBrush = Brushes.LightSalmon; break;
                case 2: spread.GridLineBrush = Brushes.LightBlue; break;
            }
        }

        private void OnSelectionBorderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spread == null) return;
            switch (_cmbSelectionBorder.SelectedIndex)
            {
                case 0: spread.SelectionBorderBrush = new SolidColorBrush(Color.FromArgb(255, 33, 115, 70)); break;
                case 1: spread.SelectionBorderBrush = Brushes.DarkGreen; break;
                case 2: spread.SelectionBorderBrush = Brushes.Orange; break;
            }
        }
    }
}
