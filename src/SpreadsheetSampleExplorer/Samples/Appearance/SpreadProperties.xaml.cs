using DevBrewLabs.WPF.Spreadsheet;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpreadsheetSampleExplorer.Data;

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

                object[,] data = DataSource.GetSpreadPropertiesData(20, 10);
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

        private void OnCustomContextMenuChecked(object sender, RoutedEventArgs e)
        {
            if (spread == null) return;

            var customMenu = new ContextMenu();
            
            var dict = new ResourceDictionary { Source = new System.Uri("pack://application:,,,/DevBrewLabs.WPF.Spreadsheet;component/Themes/ContextMenuStyle.xaml") };
            var menuStyle = dict["SpreadContextMenuStyle"] as Style;
            var menuItemStyle = dict["SpreadMenuItemStyle"] as Style;
            var separatorStyle = dict["SpreadMenuSeparatorStyle"] as Style;

            if (menuStyle != null) customMenu.Style = menuStyle;

            var item1 = new MenuItem { Header = "Analyze Data", Style = menuItemStyle };
            item1.Click += (s, args) => MessageBox.Show("Data analysis complete!");

            var item2 = new MenuItem { Header = "Generate Chart", Style = menuItemStyle };
            item2.Click += (s, args) => MessageBox.Show("Chart generated!");

            var item3 = new MenuItem { Header = "Export to PDF", Style = menuItemStyle };
            item3.Click += (s, args) => MessageBox.Show("Exported to PDF successfully!");

            var sep = new Separator { Style = separatorStyle };

            var item4 = new MenuItem { Header = "Clear Formatting", Style = menuItemStyle };
            item4.Click += (s, args) => spread.ClearContents(); // Re-use existing Spread functionality

            customMenu.Items.Add(item1);
            customMenu.Items.Add(item2);
            customMenu.Items.Add(item3);
            customMenu.Items.Add(sep);
            customMenu.Items.Add(item4);

            spread.CellContextMenu = customMenu;
        }

        private void OnCustomContextMenuUnchecked(object sender, RoutedEventArgs e)
        {
            if (spread == null) return;
            // Setting it to null allows the Spread control to fallback to its default context menu
            spread.CellContextMenu = null;
        }
    }
}
