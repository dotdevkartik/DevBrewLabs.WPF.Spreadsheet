using System.Windows;
using System.Windows.Controls;
using DevBrewLabs.WPF.Spreadsheet;

namespace SpreadsheetSampleExplorer
{
    /// <summary>
    /// Interaction logic for Testing.xaml
    /// </summary>
    public partial class Testing : UserControl
    {
        public Testing()
        {
            InitializeComponent();
            spread.MouseDoubleClick += Spread_MouseDoubleClick;
            var worksheet = spread.SheetViews.ActiveSheetView.WorkSheet;
            spread.ScrollMode = SheetScrollMode.Pixel;
        }

        private void Spread_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hitTest = spread.HitTest(e.GetPosition(spread));
            if (hitTest != null && hitTest.Element == VisualElement.ColumnHeader)
            {
                spread.SheetViews.ActiveSheetView.AutoSizeColumn(hitTest.Column);
            }
        }

        private void Testing_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
