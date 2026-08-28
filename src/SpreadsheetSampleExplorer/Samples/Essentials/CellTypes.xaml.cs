using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for CellTypes.xaml showcasing standard and custom cell types.
    /// </summary>
    public partial class CellTypes : UserControl
    {
        private RatingCellType _ratingCellType;
        private ButtonCellType _buttonCellType;
        private CheckBoxCellType _checkBoxCellType;
        private NumberCellType _numberCellType;
        private DateCellType _dateCellType;

        public CellTypes()
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
            worksheet.ColumnCount = 7;

            // 1. Initialize Cell Types
            _ratingCellType = new RatingCellType { MaxRating = 5, StarSize = 16.0, StarSpacing = 4.0 };
            _ratingCellType.RatingChanged += OnRatingChanged;

            _buttonCellType = new ButtonCellType { Text = "Execute" };
            _buttonCellType.Click += OnButtonClicked;

            _checkBoxCellType = new CheckBoxCellType { IsThreeState = true };
            _numberCellType = new NumberCellType { Format = "#,##0", ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 100 };
            _dateCellType = new DateCellType();

            // 2. Configure Column Cell Types & Dimensions
            worksheet.Columns[0].CellType = new TextCellType();
            worksheet.Columns[0].Width = 210;

            worksheet.Columns[1].CellType = new TextCellType();
            worksheet.Columns[1].Width = 125;

            worksheet.Columns[2].CellType = _checkBoxCellType;
            worksheet.Columns[2].Width = 90;

            worksheet.Columns[3].CellType = _numberCellType;
            worksheet.Columns[3].Width = 120;

            worksheet.Columns[4].CellType = _dateCellType;
            worksheet.Columns[4].Width = 120;

            worksheet.Columns[5].CellType = _ratingCellType;
            worksheet.Columns[5].Width = 145;

            worksheet.Columns[6].CellType = _buttonCellType;
            worksheet.Columns[6].Width = 95;

            // 3. Set Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Task / Feature";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Category";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Done";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Priority (0-100)";
            worksheet.ColumnHeaders.Cells[0, 4].Value = "Target Date";
            worksheet.ColumnHeaders.Cells[0, 5].Value = "Quality Rating";
            worksheet.ColumnHeaders.Cells[0, 6].Value = "Action";

            // 4. Populate Realistic Data
            PopulateData(worksheet);
        }

        private void PopulateData(IWorksheet worksheet)
        {
            var taskTemplates = new[]
            {
                new { Task = "Real-time Collaboration Engine", Category = "Core Backend", Done = (bool?)true, Priority = 95, Date = new DateTime(2026, 4, 15), Rating = 5 },
                new { Task = "AI Vector Search & Embeddings", Category = "AI & ML", Done = (bool?)null, Priority = 85, Date = new DateTime(2026, 5, 1), Rating = 4 },
                new { Task = "Dark Mode Fluent UI Theme", Category = "Frontend UI", Done = (bool?)true, Priority = 70, Date = new DateTime(2026, 3, 20), Rating = 5 },
                new { Task = "OAuth2 & SSO Single Sign-On", Category = "Security", Done = (bool?)false, Priority = 90, Date = new DateTime(2026, 6, 10), Rating = 3 },
                new { Task = "High-Speed Excel/PDF Exporter", Category = "Data Tools", Done = (bool?)true, Priority = 80, Date = new DateTime(2026, 3, 25), Rating = 5 },
                new { Task = "Payment Webhooks & Stripe Sync", Category = "Billing", Done = (bool?)null, Priority = 75, Date = new DateTime(2026, 4, 30), Rating = 4 },
                new { Task = "Automated Cluster Backups", Category = "DevOps", Done = (bool?)false, Priority = 65, Date = new DateTime(2026, 5, 15), Rating = 2 },
                new { Task = "GraphQL API Gateway Federation", Category = "API Gateway", Done = (bool?)true, Priority = 85, Date = new DateTime(2026, 4, 5), Rating = 4 },
                new { Task = "Mobile Push Notification Service", Category = "Mobile Apps", Done = (bool?)null, Priority = 60, Date = new DateTime(2026, 6, 1), Rating = 3 },
                new { Task = "Real-time Analytics Dashboard", Category = "Analytics", Done = (bool?)false, Priority = 90, Date = new DateTime(2026, 5, 20), Rating = 4 },
                new { Task = "Formula Dependency Graph Optimizer", Category = "Calc Engine", Done = (bool?)true, Priority = 95, Date = new DateTime(2026, 3, 10), Rating = 5 },
                new { Task = "Multi-Region Read Replicas", Category = "Database", Done = (bool?)false, Priority = 80, Date = new DateTime(2026, 6, 25), Rating = 3 },
                new { Task = "Audit Logging & Compliance Vault", Category = "Governance", Done = (bool?)true, Priority = 75, Date = new DateTime(2026, 4, 18), Rating = 4 },
                new { Task = "Accessibility & Screen Reader (ARIA)", Category = "Frontend UI", Done = (bool?)null, Priority = 70, Date = new DateTime(2026, 5, 12), Rating = 3 },
                new { Task = "Edge CDN Cache Invalidation", Category = "DevOps", Done = (bool?)true, Priority = 65, Date = new DateTime(2026, 3, 30), Rating = 5 }
            };

            int totalRows = 3000;
            worksheet.RowCount = totalRows;

            var data = new object[totalRows, 7];

            for (int r = 0; r < totalRows; r++)
            {
                var template = taskTemplates[r % taskTemplates.Length];
                string taskName = (r < taskTemplates.Length) ? template.Task : $"{template.Task} (Item #{r + 1})";

                data[r, 0] = taskName;
                data[r, 1] = template.Category;
                data[r, 2] = (r % 3 == 0) ? (bool?)true : (r % 3 == 1 ? (bool?)null : (bool?)false);
                data[r, 3] = ((template.Priority + (r * 7)) % 100);
                data[r, 4] = template.Date.AddDays(r % 365);
                data[r, 5] = (r % 5) + 1;
                data[r, 6] = "Run";
            }

            worksheet.Load(data, 0, 0);
        }

        private void OnButtonClicked(object sender, CellButtonClickedEventArgs e)
        {
            var taskName = e.SheetView.WorkSheet.GetValue(e.Row, 0);
            if (statusTextBlock != null)
            {
                statusTextBlock.Text = $"🔘 Action triggered on Row {e.Row + 1}: [{taskName}]";
            }
        }

        private void OnRatingChanged(object sender, RatingChangedEventArgs e)
        {
            var taskName = e.SheetView.WorkSheet.GetValue(e.Row, 0);
            if (statusTextBlock != null)
            {
                statusTextBlock.Text = $"⭐ Rating updated on Row {e.Row + 1} to {e.NewRating} Stars! ({taskName})";
            }
        }
    }
}
