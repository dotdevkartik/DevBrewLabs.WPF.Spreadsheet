using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for CustomCellTypes.xaml showcasing custom user-defined cell types and cell elements:
    /// 1. MultiOptionCellType (Custom rendered radio button groups with sub-element hit testing)
    /// 2. RatingCellType (Interactive Star Ratings with sub-element hit-testing and hover glow)
    /// </summary>
    public partial class CustomCellTypes : UserControl
    {
        private MultiOptionCellType _priorityRadioCellType;
        private MultiOptionCellType _billingCycleRadioCellType;
        private RatingCellType _standard5StarRating;
        private RatingCellType _editorial10StarRating;

        public CustomCellTypes()
        {
            InitializeComponent();
            SetupSheet(spread.Sheets.ActiveSheet);
        }

        private void SetupSheet(ISheetView sheetView)
        {
            var worksheet = sheetView.WorkSheet;
            worksheet.RowCount = 2000;
            worksheet.ColumnCount = 4;

            // 1. Initialize MultiOptionCellType (Custom Radio Button Groups)
            // Priority Options: Low, Med, High
            _priorityRadioCellType = new MultiOptionCellType
            {
                Items = new[] { "Low", "Med", "High" },
                RadioSize = 13.0,
                DotSize = 6.0,
                ItemSpacing = 16.0,
                TextGap = 5.0,
                SelectedBrush = SheetUtils.CreateFrozenBrush("#059669"), // Emerald-600
                HoverHaloBrush = SheetUtils.CreateFrozenBrush(Color.FromArgb(35, 5, 150, 105)),
                HoverHaloPen = SheetUtils.CreateFrozenPen(SheetUtils.CreateFrozenBrush(Color.FromArgb(80, 16, 185, 129)), 1.0)
            };
            _priorityRadioCellType.SelectionChanged += (s, e) =>
            {
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"🔘 Priority changed on Row {e.Row + 1} to [{e.NewValue}] (Col {e.Column + 1})";
                }
            };

            // Billing Cycle Options: Monthly, Annual
            _billingCycleRadioCellType = new MultiOptionCellType
            {
                Items = new[] { "Monthly", "Annual" },
                RadioSize = 13.0,
                DotSize = 6.0,
                ItemSpacing = 20.0,
                TextGap = 6.0,
                SelectedBrush = SheetUtils.CreateFrozenBrush("#2563EB"), // Blue-600
                HoverHaloBrush = SheetUtils.CreateFrozenBrush(Color.FromArgb(35, 37, 99, 235)),
                HoverHaloPen = SheetUtils.CreateFrozenPen(SheetUtils.CreateFrozenBrush(Color.FromArgb(80, 59, 130, 246)), 1.0)
            };
            _billingCycleRadioCellType.SelectionChanged += (s, e) =>
            {
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"🔘 Billing Cycle changed on Row {e.Row + 1} to [{e.NewValue}] (Col {e.Column + 1})";
                }
            };

            // 2. Initialize RatingCellType (Custom Star Ratings)
            // Standard 5-Star Rating (Golden Amber)
            _standard5StarRating = new RatingCellType
            {
                MaxRating = 5,
                StarSize = 16.0,
                StarSpacing = 4.0,
                FilledStarBrush = SheetUtils.CreateFrozenBrush("#F59E0B"), // Amber-500
                HoverStarBrush = SheetUtils.CreateFrozenBrush("#D97706")   // Amber-600
            };
            _standard5StarRating.RatingChanged += (s, e) =>
            {
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"⭐ 5-Star Rating changed on Row {e.Row + 1} to {e.NewRating}/5 Stars!";
                }
            };

            // Editorial 10-Star Rating (Purple / Violet)
            _editorial10StarRating = new RatingCellType
            {
                MaxRating = 10,
                StarSize = 12.0,
                StarSpacing = 3.0,
                FilledStarBrush = SheetUtils.CreateFrozenBrush("#8B5CF6"), // Violet-500
                HoverStarBrush = SheetUtils.CreateFrozenBrush("#7C3AED")   // Violet-600
            };
            _editorial10StarRating.RatingChanged += (s, e) =>
            {
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"🌟 Editorial Score changed on Row {e.Row + 1} to {e.NewRating}/10 Stars!";
                }
            };

            // 3. Configure Columns (Purely Custom Cell Types)
            worksheet.Columns[0].CellType = _priorityRadioCellType;
            worksheet.Columns[0].Width = 190;

            worksheet.Columns[1].CellType = _billingCycleRadioCellType;
            worksheet.Columns[1].Width = 180;

            worksheet.Columns[2].CellType = _standard5StarRating;
            worksheet.Columns[2].Width = 160;

            worksheet.Columns[3].CellType = _editorial10StarRating;
            worksheet.Columns[3].Width = 200;

            // 4. Set Column Headers
            worksheet.ColumnHeaders.Cells[0, 0].Value = "Priority (Multi-Option Radio)";
            worksheet.ColumnHeaders.Cells[0, 1].Value = "Billing (Multi-Option Radio)";
            worksheet.ColumnHeaders.Cells[0, 2].Value = "User Rating (5★)";
            worksheet.ColumnHeaders.Cells[0, 3].Value = "Editorial Review (10★)";

            // 5. Populate Data
            PopulateData(worksheet);
        }

        private void PopulateData(IWorksheet worksheet)
        {
            var productTemplates = new[]
            {
                new { Priority = "High", Billing = "Annual", Stars5 = 5, Stars10 = 10 },
                new { Priority = "Med", Billing = "Monthly", Stars5 = 5, Stars10 = 9 },
                new { Priority = "Low", Billing = "Annual", Stars5 = 4, Stars10 = 8 },
                new { Priority = "High", Billing = "Annual", Stars5 = 5, Stars10 = 10 },
                new { Priority = "Med", Billing = "Monthly", Stars5 = 4, Stars10 = 8 },
                new { Priority = "Low", Billing = "Monthly", Stars5 = 4, Stars10 = 7 },
                new { Priority = "High", Billing = "Annual", Stars5 = 5, Stars10 = 9 },
                new { Priority = "High", Billing = "Monthly", Stars5 = 5, Stars10 = 9 },
                new { Priority = "Med", Billing = "Annual", Stars5 = 4, Stars10 = 8 },
                new { Priority = "Low", Billing = "Monthly", Stars5 = 3, Stars10 = 6 },
                new { Priority = "Low", Billing = "Monthly", Stars5 = 2, Stars10 = 4 },
                new { Priority = "High", Billing = "Annual", Stars5 = 4, Stars10 = 8 }
            };

            int totalRows = 2000;
            worksheet.RowCount = totalRows;

            var data = new object[totalRows, 4];

            for (int r = 0; r < totalRows; r++)
            {
                var template = productTemplates[r % productTemplates.Length];

                int stars5 = Math.Max(1, Math.Min(5, template.Stars5 - ((r % 4) == 0 ? 1 : 0)));
                int stars10 = Math.Max(1, Math.Min(10, template.Stars10 - (r % 3)));

                data[r, 0] = template.Priority;
                data[r, 1] = template.Billing;
                data[r, 2] = stars5;
                data[r, 3] = stars10;
            }

            worksheet.Load(data, 0, 0);
        }
    }
}
