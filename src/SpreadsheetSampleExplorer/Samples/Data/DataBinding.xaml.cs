using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.WPF.Spreadsheet;
using SpreadsheetSampleExplorer.Data;
using System.Linq;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for CollectionDataBinding.xaml
    /// </summary>
    public partial class DataBinding : UserControl
    {
        public DataBinding()
        {
            InitializeComponent();
            SetupListBinding(spread.Sheets.ActiveSheet);
            SetuDataTableBinding(spread2.Sheets.ActiveSheet);
        }

        private void SetuDataTableBinding(ISheetView sheetView)
        {
            var customers = DataSource.GetCustomersTable(5000);
            var worksheet = sheetView.WorkSheet;
            worksheet.DataSource = customers;
            worksheet.Columns[0].DataMap = new DataColumnDataMap("Id");
            worksheet.Columns[1].DataMap = new DataColumnDataMap("Age");
            worksheet.Columns[2].DataMap = new DataColumnDataMap("FirstName");
            worksheet.Columns[3].DataMap = new DataColumnDataMap("LastName");
            worksheet.Columns[4].DataMap = new DataColumnDataMap("Gender");
            worksheet.Columns[5].DataMap = new DataColumnDataMap("Email");
            worksheet.Columns[5].Width = 200;
            worksheet.Columns[6].DataMap = new DataColumnDataMap("Phone");
            worksheet.Columns[6].Width = 100;
        }

        private void SetupListBinding(ISheetView sheetView)
        {
            var customers = DataSource.GetCustomers(5000).ToList();
            var worksheet = sheetView.WorkSheet;
            worksheet.DataSource = customers;
            worksheet.Columns[0].DataMap = new PropertyDataMap("Id");
            worksheet.Columns[1].DataMap = new PropertyDataMap("Age");
            worksheet.Columns[2].DataMap = new PropertyDataMap("FirstName");
            worksheet.Columns[3].DataMap = new PropertyDataMap("LastName");
            worksheet.Columns[4].DataMap = new PropertyDataMap("Gender");
            worksheet.Columns[5].DataMap = new PropertyDataMap("Email");
            worksheet.Columns[5].Width = 200;
            worksheet.Columns[6].DataMap = new PropertyDataMap("Phone");
            worksheet.Columns[6].Width = 100;
        }
    }
}
