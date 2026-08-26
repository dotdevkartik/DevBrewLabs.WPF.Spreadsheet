using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;
using System.Windows.Controls;
using SpreadsheetSampleExplorer.Data;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for CellTypes.xaml
    /// </summary>
    public partial class CellTypes : UserControl
    {
        public CellTypes()
        {
            InitializeComponent();
            SetupSheet(spread.Sheets.ActiveSheet);
        }

        private void SetupSheet(ISheetView sheetView)
        {
            var worksheet = sheetView.WorkSheet;

            worksheet.Columns[0].CellType = new TextCellType();
            worksheet.Columns[1].CellType = new CheckBoxCellType() { IsThreeState = true };
            worksheet.Columns[2].CellType = new NumberCellType() { Format = "#,##0", ShowSpinners = true, Step = 10, Minimum = -1000, Maximum = 100000 };
            worksheet.Columns[3].CellType = new DateCellType();
            worksheet.Columns[4].CellType = new ButtonCellType() { Text = "Button" };

            var data = DataSource.GetCellTypesData(50, 4);
            worksheet.Load(data);
        }
    }
}
