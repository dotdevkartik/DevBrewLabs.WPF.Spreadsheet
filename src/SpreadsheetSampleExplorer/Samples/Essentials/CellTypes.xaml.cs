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
        private ComboBoxCellType _categoryComboBoxCellType;
        private ComboBoxCellType _statusComboBoxCellType;
        private RatingCellType _ratingCellType;
        private ButtonCellType _buttonCellType;
        private CheckBoxCellType _checkBoxCellType;
        private NumberCellType _numberCellType;
        private DateCellType _dateCellType;
        private ProgressBarCellType _progressBarCellType;
        private HyperlinkCellType _hyperlinkCellType;

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
            worksheet.ColumnCount = 10;

            // 1. Initialize Cell Types
            _categoryComboBoxCellType = new ComboBoxCellType
            {
                ItemsSource = new[]
                {
                    "Core Backend", "AI & ML", "Frontend UI", "Security", "Data Tools",
                    "Billing", "DevOps", "API Gateway", "Mobile Apps", "Analytics", "Calc Engine", "Database", "Governance"
                }
            };

            _statusComboBoxCellType = new ComboBoxCellType
            {
                IsEditable = true,
                ItemsSource = new[]
                {
                    "Backlog", "In Progress", "In Review", "Approved", "Deployed", "Blocked"
                }
            };

            _ratingCellType = new RatingCellType { MaxRating = 5, StarSize = 16.0, StarSpacing = 4.0 };
            _ratingCellType.RatingChanged += OnRatingChanged;

            _buttonCellType = new ButtonCellType { Text = "Execute" };
            _buttonCellType.Click += OnButtonClicked;

            _checkBoxCellType = new CheckBoxCellType { IsThreeState = true };
            _numberCellType = new NumberCellType { Format = "#,##0", ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 100 };
            _dateCellType = new DateCellType();

            _progressBarCellType = new ProgressBarCellType
            {
                Minimum = 0,
                Maximum = 100,
                AutoColor = true,
                Format = "{0:0}%"
            };

            _hyperlinkCellType = new HyperlinkCellType
            {
                UnderlineMode = HyperlinkUnderlineMode.Always,
                OpenUrlOnClick = true
            };
            _hyperlinkCellType.Click += OnHyperlinkClicked;

            // 2. Configure Column Cell Types & Dimensions
            worksheet.Columns[0].CellType = new TextCellType();
            worksheet.Columns[0].Width = 190;

            worksheet.Columns[1].CellType = _categoryComboBoxCellType;
            worksheet.Columns[1].Width = 130;

            worksheet.Columns[2].CellType = _statusComboBoxCellType;
            worksheet.Columns[2].Width = 110;

            worksheet.Columns[3].CellType = _checkBoxCellType;
            worksheet.Columns[3].Width = 65;

            worksheet.Columns[4].CellType = _numberCellType;
            worksheet.Columns[4].Width = 110;

            worksheet.Columns[5].CellType = _progressBarCellType;
            worksheet.Columns[5].Width = 125;

            worksheet.Columns[6].CellType = _dateCellType;
            worksheet.Columns[6].Width = 105;

            worksheet.Columns[7].CellType = _ratingCellType;
            worksheet.Columns[7].Width = 130;

            worksheet.Columns[8].CellType = _hyperlinkCellType;
            worksheet.Columns[8].Width = 175;

            worksheet.Columns[9].CellType = _buttonCellType;
            worksheet.Columns[9].Width = 85;

            // 3. Set Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Task / Feature";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Category";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Status";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Done";
            worksheet.ColumnHeaders.Cells[0, 4].Value = "Priority (0-100)";
            worksheet.ColumnHeaders.Cells[0, 5].Value = "Progress";
            worksheet.ColumnHeaders.Cells[0, 6].Value = "Target Date";
            worksheet.ColumnHeaders.Cells[0, 7].Value = "Quality Rating";
            worksheet.ColumnHeaders.Cells[0, 8].Value = "Reference / Link";
            worksheet.ColumnHeaders.Cells[0, 9].Value = "Action";

            // 4. Populate Realistic Data
            PopulateData(worksheet);
        }

        private void PopulateData(IWorksheet worksheet)
        {
            var taskTemplates = new[]
            {
                new { Task = "Real-time Collaboration Engine", Category = "Core Backend", Status = "In Progress", Done = (bool?)true, Priority = 95, Progress = 65, Date = new DateTime(2026, 4, 15), Rating = 5, Url = "https://github.com/dotdevkartik" },
                new { Task = "AI Vector Search & Embeddings", Category = "AI & ML", Status = "In Review", Done = (bool?)null, Priority = 85, Progress = 80, Date = new DateTime(2026, 5, 1), Rating = 4, Url = "https://cloud.google.com/vertex-ai" },
                new { Task = "Dark Mode Fluent UI Theme", Category = "Frontend UI", Status = "Deployed", Done = (bool?)true, Priority = 70, Progress = 100, Date = new DateTime(2026, 3, 20), Rating = 5, Url = "https://fluent2.microsoft.design" },
                new { Task = "OAuth2 & SSO Single Sign-On", Category = "Security", Status = "Approved", Done = (bool?)false, Priority = 90, Progress = 40, Date = new DateTime(2026, 6, 10), Rating = 3, Url = "https://oauth.net/2" },
                new { Task = "High-Speed Excel/PDF Exporter", Category = "Data Tools", Status = "Deployed", Done = (bool?)true, Priority = 80, Progress = 100, Date = new DateTime(2026, 3, 25), Rating = 5, Url = "https://learn.microsoft.com/dotnet" },
                new { Task = "Payment Webhooks & Stripe Sync", Category = "Billing", Status = "Blocked", Done = (bool?)null, Priority = 75, Progress = 25, Date = new DateTime(2026, 4, 30), Rating = 4, Url = "https://stripe.com/docs/webhooks" },
                new { Task = "Automated Cluster Backups", Category = "DevOps", Status = "Backlog", Done = (bool?)false, Priority = 65, Progress = 10, Date = new DateTime(2026, 5, 15), Rating = 2, Url = "https://kubernetes.io/docs" },
                new { Task = "GraphQL API Gateway Federation", Category = "API Gateway", Status = "In Progress", Done = (bool?)true, Priority = 85, Progress = 55, Date = new DateTime(2026, 4, 5), Rating = 4, Url = "https://graphql.org" },
                new { Task = "Mobile Push Notification Service", Category = "Mobile Apps", Status = "In Review", Done = (bool?)null, Priority = 60, Progress = 85, Date = new DateTime(2026, 6, 1), Rating = 3, Url = "https://firebase.google.com" },
                new { Task = "Real-time Analytics Dashboard", Category = "Analytics", Status = "Approved", Done = (bool?)false, Priority = 90, Progress = 45, Date = new DateTime(2026, 5, 20), Rating = 4, Url = "https://apache.org" },
                new { Task = "Formula Dependency Graph Optimizer", Category = "Calc Engine", Status = "Deployed", Done = (bool?)true, Priority = 95, Progress = 100, Date = new DateTime(2026, 3, 10), Rating = 5, Url = "https://github.com/dotdevkartik" },
                new { Task = "Multi-Region Read Replicas", Category = "Database", Status = "In Progress", Done = (bool?)false, Priority = 80, Progress = 50, Date = new DateTime(2026, 6, 25), Rating = 3, Url = "https://postgresql.org" },
                new { Task = "Audit Logging & Compliance Vault", Category = "Governance", Status = "Approved", Done = (bool?)true, Priority = 75, Progress = 30, Date = new DateTime(2026, 4, 18), Rating = 4, Url = "https://csrc.nist.gov" },
                new { Task = "Accessibility & Screen Reader (ARIA)", Category = "Frontend UI", Status = "In Review", Done = (bool?)null, Priority = 70, Progress = 75, Date = new DateTime(2026, 5, 12), Rating = 3, Url = "https://w3.org/WAI/standards-guidelines/aria" },
                new { Task = "Edge CDN Cache Invalidation", Category = "DevOps", Status = "Deployed", Done = (bool?)true, Priority = 65, Progress = 100, Date = new DateTime(2026, 3, 30), Rating = 5, Url = "https://cloudflare.com" }
            };

            int totalRows = 3000;
            worksheet.RowCount = totalRows;

            var data = new object[totalRows, 10];

            for (int r = 0; r < totalRows; r++)
            {
                var template = taskTemplates[r % taskTemplates.Length];
                string taskName = (r < taskTemplates.Length) ? template.Task : $"{template.Task} (Item #{r + 1})";

                data[r, 0] = taskName;
                data[r, 1] = template.Category;
                data[r, 2] = template.Status;
                data[r, 3] = (r % 3 == 0) ? (bool?)true : (r % 3 == 1 ? (bool?)null : (bool?)false);
                data[r, 4] = ((template.Priority + (r * 7)) % 100);
                data[r, 5] = Math.Max(0, Math.Min(100, template.Progress + ((r % 7) * 5) - 15));
                data[r, 6] = template.Date.AddDays(r % 365);
                data[r, 7] = (r % 5) + 1;
                data[r, 8] = template.Url;
                data[r, 9] = "Run";
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

        private void OnHyperlinkClicked(object sender, CellHyperlinkClickedEventArgs e)
        {
            if (statusTextBlock != null)
            {
                statusTextBlock.Text = $"🔗 Hyperlink clicked on Row {e.Row + 1}: {e.Url}";
            }
        }
    }
}
