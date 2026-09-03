using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using System;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for AdvancedCellTypes.xaml showcasing rich visual indicators and interactive cell widgets:
    /// Progress Bars (Standard and Auto-Color KPI) and Hyperlinks (Hover & Always Underline with Hand cursor).
    /// </summary>
    public partial class AdvancedCellTypes : UserControl
    {
        private ProgressBarCellType _standardProgressBar;
        private ProgressBarCellType _autoColorProgressBar;
        private HyperlinkCellType _docHyperlinkCellType;
        private HyperlinkCellType _repoHyperlinkCellType;

        public AdvancedCellTypes()
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
            worksheet.ColumnCount = 4;

            // Row Heights: 28px gives modern breathing room for progress capsules and links
            worksheet.ColumnHeaders.Rows[0].Height = 32;
            for (int r = 0; r < worksheet.RowCount; r++)
            {
                worksheet.Rows[r].Height = 28;
            }

            // 1. Initialize Advanced Cell Types
            _standardProgressBar = new ProgressBarCellType
            {
                Minimum = 0,
                Maximum = 100,
                AutoColor = false,
                Format = "{0:0}%",
                TextPlacement = ProgressBarTextPlacement.Right
            };

            _autoColorProgressBar = new ProgressBarCellType
            {
                Minimum = 0,
                Maximum = 100,
                AutoColor = true,
                Format = "{0:0}%",
                TextPlacement = ProgressBarTextPlacement.Right
            };

            _docHyperlinkCellType = new HyperlinkCellType
            {
                UnderlineMode = HyperlinkUnderlineMode.Always,
                OpenUrlOnClick = true
            };
            _docHyperlinkCellType.Click += OnHyperlinkClicked;

            _repoHyperlinkCellType = new HyperlinkCellType
            {
                UnderlineMode = HyperlinkUnderlineMode.Hover,
                OpenUrlOnClick = true
            };
            _repoHyperlinkCellType.Click += OnHyperlinkClicked;

            // 2. Configure Column Cell Types & Widths
            worksheet.Columns[0].CellType = _standardProgressBar;
            worksheet.Columns[0].Width = 190;

            worksheet.Columns[1].CellType = _autoColorProgressBar;
            worksheet.Columns[1].Width = 200;

            worksheet.Columns[2].CellType = _docHyperlinkCellType;
            worksheet.Columns[2].Width = 260;

            worksheet.Columns[3].CellType = _repoHyperlinkCellType;
            worksheet.Columns[3].Width = 230;

            // 3. Set Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Milestone Progress (Standard)";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Health SLA (Auto-Color)";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "Documentation Link (Always Underline)";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Repository Link (Hover Underline)";

            // 4. Populate Data
            PopulateData(worksheet);
        }

        private void PopulateData(IWorksheet worksheet)
        {
            var projectTemplates = new[]
            {
                new { Progress = 75, SLA = 92, Doc = "https://github.com/dotdevkartik/DevBrewLabs.WPF.Spreadsheet", Repo = "https://github.com/dotdevkartik" },
                new { Progress = 85, SLA = 65, Doc = "https://cloud.google.com/vertex-ai", Repo = "https://github.com/google" },
                new { Progress = 95, SLA = 98, Doc = "https://learn.microsoft.com/dotnet", Repo = "https://github.com/dotnet/wpf" },
                new { Progress = 40, SLA = 30, Doc = "https://oauth.net/2", Repo = "https://github.com/identity" },
                new { Progress = 100, SLA = 100, Doc = "https://github.com/OfficeDev/Open-XML-SDK", Repo = "https://github.com/OfficeDev" },
                new { Progress = 25, SLA = 28, Doc = "https://stripe.com/docs/webhooks", Repo = "https://github.com/stripe" },
                new { Progress = 15, SLA = 20, Doc = "https://kubernetes.io/docs", Repo = "https://github.com/kubernetes" },
                new { Progress = 60, SLA = 85, Doc = "https://graphql.org", Repo = "https://github.com/graphql" },
                new { Progress = 80, SLA = 60, Doc = "https://firebase.google.com", Repo = "https://github.com/firebase" },
                new { Progress = 100, SLA = 99, Doc = "https://github.com/dotdevkartik", Repo = "https://github.com/dotdevkartik" }
            };

            int totalRows = 3000;
            worksheet.RowCount = totalRows;

            var data = new object[totalRows, 4];

            for (int r = 0; r < totalRows; r++)
            {
                var template = projectTemplates[r % projectTemplates.Length];

                data[r, 0] = Math.Max(0, Math.Min(100, template.Progress + ((r % 7) * 4) - 12));
                data[r, 1] = Math.Max(0, Math.Min(100, template.SLA + ((r % 9) * 5) - 20));
                data[r, 2] = template.Doc;
                data[r, 3] = template.Repo;
            }

            worksheet.Load(data, 0, 0);
        }

        private void OnHyperlinkClicked(object sender, CellHyperlinkClickedEventArgs e)
        {
            if (statusTextBlock != null)
            {
                statusTextBlock.Text = $"🔗 Hyperlink opened: {e.Url} (Row {e.Row + 1})";
            }
        }
    }
}
