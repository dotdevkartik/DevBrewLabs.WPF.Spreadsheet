using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for BasicCellTypes.xaml showcasing standard data entry cell types.
    /// </summary>
    public partial class BasicCellTypes : UserControl
    {
        private ComboBoxCellType _departmentComboBoxCellType;
        private ComboBoxCellType _employmentTypeComboBoxCellType;
        private ButtonCellType _buttonCellType;
        private CheckBoxCellType _checkBoxCellType;
        private NumberCellType _experienceNumberCellType;
        private NumberCellType _rateNumberCellType;
        private DateCellType _dateCellType;

        public BasicCellTypes()
        {
            InitializeComponent();
            SetupSheet(spread.Sheets.ActiveSheet);

            spread.CellEditEnded += (s, e) =>
            {
                if (statusTextBlock != null)
                {
                    var val = e.SheetView.WorkSheet.GetValue(e.Row, e.Column);
                    statusTextBlock.Text = $"✏️ Edited Row {e.Row + 1}, Col {e.Column + 1}: '{val}'";
                }
            };
        }

        private void SetupSheet(ISheetView sheetView)
        {
            var worksheet = sheetView.WorkSheet;
            worksheet.RowCount = 3000;
            worksheet.ColumnCount = 9;

            // 1. Initialize Cell Types
            _departmentComboBoxCellType = new ComboBoxCellType
            {
                ItemsSource = new[]
                {
                    "Engineering", "Product Design", "Cloud Infrastructure", "Security & Ops",
                    "Data Analytics", "Finance", "Legal & Compliance", "Sales", "Customer Success"
                }
            };

            _employmentTypeComboBoxCellType = new ComboBoxCellType
            {
                IsEditable = true,
                ItemsSource = new[]
                {
                    "Full-Time", "Contract", "Part-Time", "Intern", "Advisory"
                }
            };

            _buttonCellType = new ButtonCellType { Text = "Review" };
            _buttonCellType.Click += OnButtonClicked;

            _checkBoxCellType = new CheckBoxCellType { IsThreeState = true };

            _experienceNumberCellType = new NumberCellType
            {
                Format = "#,##0 Yrs",
                ShowSpinners = true,
                Step = 1,
                Minimum = 0,
                Maximum = 40
            };

            _rateNumberCellType = new NumberCellType
            {
                Format = "$#,##0.00",
                ShowSpinners = true,
                Step = 5,
                Minimum = 15,
                Maximum = 350
            };

            _dateCellType = new DateCellType { Format = "yyyy-MM-dd" };

            // 2. Configure Column Cell Types & Widths
            worksheet.Columns[0].CellType = new TextCellType();
            worksheet.Columns[0].Width = 100;

            worksheet.Columns[1].CellType = new TextCellType();
            worksheet.Columns[1].Width = 175;

            worksheet.Columns[2].CellType = _departmentComboBoxCellType;
            worksheet.Columns[2].Width = 150;

            worksheet.Columns[3].CellType = _employmentTypeComboBoxCellType;
            worksheet.Columns[3].Width = 120;

            worksheet.Columns[4].CellType = _checkBoxCellType;
            worksheet.Columns[4].Width = 70;

            worksheet.Columns[5].CellType = _experienceNumberCellType;
            worksheet.Columns[5].Width = 125;

            worksheet.Columns[6].CellType = _dateCellType;
            worksheet.Columns[6].Width = 115;

            worksheet.Columns[7].CellType = _rateNumberCellType;
            worksheet.Columns[7].Width = 115;

            worksheet.Columns[8].CellType = _buttonCellType;
            worksheet.Columns[8].Width = 85;

            // 3. Set Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Employee ID";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Full Name";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Department";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Type";
            worksheet.ColumnHeaders.Cells[0, 4].Value = "Active";
            worksheet.ColumnHeaders.Cells[0, 5].Value = "Experience";
            worksheet.ColumnHeaders.Cells[0, 6].Value = "Hire Date";
            worksheet.ColumnHeaders.Cells[0, 7].Value = "Hourly Rate";
            worksheet.ColumnHeaders.Cells[0, 8].Value = "Action";

            // 4. Populate Data
            PopulateData(worksheet);
        }

        private void PopulateData(IWorksheet worksheet)
        {
            var staffTemplates = new[]
            {
                new { Name = "Elena Rostova", Dept = "Engineering", Type = "Full-Time", Active = (bool?)true, Exp = 8, Hire = new DateTime(2018, 5, 14), Rate = 85.0 },
                new { Name = "Marcus Vance", Dept = "Cloud Infrastructure", Type = "Full-Time", Active = (bool?)true, Exp = 12, Hire = new DateTime(2015, 2, 1), Rate = 110.0 },
                new { Name = "Aria Montgomery", Dept = "Product Design", Type = "Contract", Active = (bool?)null, Exp = 5, Hire = new DateTime(2021, 9, 15), Rate = 75.0 },
                new { Name = "David Chen", Dept = "Data Analytics", Type = "Full-Time", Active = (bool?)true, Exp = 6, Hire = new DateTime(2020, 3, 10), Rate = 90.0 },
                new { Name = "Sophia Lindqvist", Dept = "Security & Ops", Type = "Full-Time", Active = (bool?)true, Exp = 10, Hire = new DateTime(2017, 11, 20), Rate = 125.0 },
                new { Name = "Tariq Al-Mansoor", Dept = "Finance", Type = "Part-Time", Active = (bool?)false, Exp = 4, Hire = new DateTime(2022, 1, 12), Rate = 60.0 },
                new { Name = "Clara Dupont", Dept = "Legal & Compliance", Type = "Advisory", Active = (bool?)true, Exp = 15, Hire = new DateTime(2014, 8, 30), Rate = 160.0 },
                new { Name = "Kenji Sato", Dept = "Engineering", Type = "Full-Time", Active = (bool?)true, Exp = 7, Hire = new DateTime(2019, 6, 18), Rate = 95.0 },
                new { Name = "Chloe Bailey", Dept = "Sales", Type = "Full-Time", Active = (bool?)true, Exp = 3, Hire = new DateTime(2023, 4, 5), Rate = 55.0 },
                new { Name = "Lucas Meyer", Dept = "Customer Success", Type = "Contract", Active = (bool?)null, Exp = 2, Hire = new DateTime(2024, 2, 10), Rate = 45.0 }
            };

            int totalRows = 3000;
            worksheet.RowCount = totalRows;

            var data = new object[totalRows, 9];

            for (int r = 0; r < totalRows; r++)
            {
                var template = staffTemplates[r % staffTemplates.Length];
                data[r, 0] = $"EMP-{(1001 + r):D4}";
                data[r, 1] = (r < staffTemplates.Length) ? template.Name : $"{template.Name} ({r + 1})";
                data[r, 2] = template.Dept;
                data[r, 3] = template.Type;
                data[r, 4] = (r % 5 == 0) ? (bool?)null : (r % 6 == 0 ? (bool?)false : (bool?)true);
                data[r, 5] = template.Exp;
                data[r, 6] = template.Hire.AddDays((r * 17) % 1800);
                data[r, 7] = template.Rate + ((r % 10) * 5);
                data[r, 8] = "Review";
            }

            worksheet.Load(data, 0, 0);
        }

        private void OnButtonClicked(object sender, CellButtonClickedEventArgs e)
        {
            var empName = e.SheetView.WorkSheet.GetValue(e.Row, 1);
            if (statusTextBlock != null)
            {
                statusTextBlock.Text = $"🔘 Action triggered for Row {e.Row + 1}: [{empName}]";
            }
        }
    }
}
