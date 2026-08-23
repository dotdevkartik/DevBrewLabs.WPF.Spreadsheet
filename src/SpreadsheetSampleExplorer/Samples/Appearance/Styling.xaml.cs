using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet;
using SpreadsheetSampleExplorer.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Interaction logic for Styling.xaml
    /// </summary>
    public partial class Styling : UserControl
    {
        private bool _isInitializing = true;

        public Styling()
        {
            InitializeComponent();
            _isInitializing = false;

            // Setup main spread
            Setup(spreadMain, (view) => UpdateSheetStyle(view, GetSelectedThemeKey(), GetAltRowsEnabled(), GetSelectedAlignment()));

            // Setup grid showcase spreads
            Setup(spread1, (view) => UpdateSheetStyle(view, "Slate", true, CellHorizontalAlignment.Center));
            Setup(spread2, (view) => UpdateSheetStyle(view, "Excel", true, CellHorizontalAlignment.Center));
            Setup(spread3, (view) => UpdateSheetStyle(view, "Indigo", true, CellHorizontalAlignment.Center));
            Setup(spread4, (view) => UpdateSheetStyle(view, "Corporate", true, CellHorizontalAlignment.Center));
        }

        private void Setup(Spread spread, Action<ISheetView> styleAction)
        {
            var sheet = spread.WorkBook.WorkSheets.GetSheet(0);
            SetupSheetDataSource(sheet);
            styleAction(spread.Sheets.GetSheetView(sheet));

            spread.WorkBook.WorkSheets.SheetAdded += (s, e) =>
            {
                styleAction(spread.Sheets.GetSheetView(e.AddedSheet));
            };
        }

        private void UpdateSheetStyle(ISheetView sheetView, string themeKey, bool useAltRows, CellHorizontalAlignment alignment)
        {
            var worksheet = sheetView.WorkSheet;

            DrawingColor headerBg, headerFg, altRowBg, altRowFg;

            switch (themeKey)
            {
                case "Excel":
                    headerBg = DrawingColor.FromArgb(255, 16, 124, 65); // #107C41 Excel Green
                    headerFg = DrawingColor.FromArgb(255, 255, 255, 255);
                    altRowBg = DrawingColor.FromArgb(255, 240, 253, 244);
                    altRowFg = DrawingColor.FromArgb(255, 24, 24, 27);
                    break;

                case "Emerald":
                    headerBg = DrawingColor.FromArgb(255, 230, 244, 234);
                    headerFg = DrawingColor.FromArgb(255, 13, 101, 45);
                    altRowBg = DrawingColor.FromArgb(255, 246, 251, 247);
                    altRowFg = DrawingColor.FromArgb(255, 24, 24, 27);
                    break;

                case "Indigo":
                    headerBg = DrawingColor.FromArgb(255, 238, 242, 255);
                    headerFg = DrawingColor.FromArgb(255, 55, 48, 163);
                    altRowBg = DrawingColor.FromArgb(255, 248, 250, 252);
                    altRowFg = DrawingColor.FromArgb(255, 24, 24, 27);
                    break;

                case "Corporate":
                    headerBg = DrawingColor.FromArgb(255, 30, 58, 138);
                    headerFg = DrawingColor.FromArgb(255, 255, 255, 255);
                    altRowBg = DrawingColor.FromArgb(255, 240, 246, 255);
                    altRowFg = DrawingColor.FromArgb(255, 24, 24, 27);
                    break;

                case "Slate":
                default:
                    headerBg = DrawingColor.FromArgb(255, 241, 245, 249);
                    headerFg = DrawingColor.FromArgb(255, 15, 23, 42);
                    altRowBg = DrawingColor.FromArgb(255, 248, 250, 252);
                    altRowFg = DrawingColor.FromArgb(255, 24, 24, 27);
                    break;
            }

            // Headers Styling
            string headerStyleName = "HeaderStyle_" + themeKey + "_" + alignment.ToString();
            if (worksheet.WorkBook.GetNamedStyle(headerStyleName) == null)
            {
                var headerStyle = new CellStyle
                {
                    BackColor = headerBg,
                    ForeColor = headerFg,
                    HorizontalAlignment = alignment,
                    FontWeight = DrawingFontWeight.Bold
                };
                worksheet.WorkBook.AddNamedStyle(headerStyleName, headerStyle);
            }

            worksheet.ColumnHeaders.Rows[0].StyleName = headerStyleName;
            worksheet.RowHeaders.Columns[0].StyleName = headerStyleName;
            worksheet.TopLeft.StyleName = headerStyleName;

            // Alternating Row Styling
            string altRowStyleName = "AltRowStyle_" + themeKey;
            string normalRowStyleName = "NormalRowStyle_" + themeKey;

            if (worksheet.WorkBook.GetNamedStyle(altRowStyleName) == null)
            {
                var rowStyle = new CellStyle
                {
                    BackColor = altRowBg,
                    ForeColor = altRowFg
                };
                worksheet.WorkBook.AddNamedStyle(altRowStyleName, rowStyle);
            }

            if (worksheet.WorkBook.GetNamedStyle(normalRowStyleName) == null)
            {
                var normalStyle = new CellStyle
                {
                    BackColor = DrawingColor.White,
                    ForeColor = altRowFg
                };
                worksheet.WorkBook.AddNamedStyle(normalRowStyleName, normalStyle);
            }

            for (int row = 0; row < worksheet.RowCount; row++)
            {
                if (useAltRows && row % 2 != 0)
                {
                    worksheet.Rows[row].StyleName = altRowStyleName;
                }
                else
                {
                    worksheet.Rows[row].StyleName = normalRowStyleName;
                }
            }
        }

        private void SetupSheetDataSource(IWorksheet worksheet)
        {
            var customers = DataSource.GetCustomers().ToList();
            worksheet.DataSource = customers;
            worksheet.Columns[0].DataMap = new PropertyDataMap("Id");
            worksheet.Columns[1].DataMap = new PropertyDataMap("Age");
            worksheet.Columns[2].DataMap = new PropertyDataMap("FirstName");
            worksheet.Columns[3].DataMap = new PropertyDataMap("LastName");
            worksheet.Columns[4].DataMap = new PropertyDataMap("Gender");
            worksheet.Columns[5].DataMap = new PropertyDataMap("Email");
            worksheet.Columns[5].Width = 200;
            worksheet.Columns[6].DataMap = new PropertyDataMap("Phone");
            worksheet.Columns[6].Width = 120;
        }

        private string GetSelectedThemeKey()
        {
            if (_cmbTheme?.SelectedItem is ComboBoxItem item && item.Tag != null)
                return item.Tag.ToString();
            return "Slate";
        }

        private bool GetAltRowsEnabled()
        {
            return _chkAltRows?.IsChecked == true;
        }

        private CellHorizontalAlignment GetSelectedAlignment()
        {
            if (_cmbAlign?.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                switch (item.Tag.ToString())
                {
                    case "Left": return CellHorizontalAlignment.Left;
                    case "Right": return CellHorizontalAlignment.Right;
                }
            }
            return CellHorizontalAlignment.Center;
        }

        private void ApplyMainTheme()
        {
            if (_isInitializing || spreadMain == null)
                return;

            var sheetView = spreadMain.Sheets.GetSheetView(spreadMain.WorkBook.WorkSheets.GetSheet(0));
            UpdateSheetStyle(sheetView, GetSelectedThemeKey(), GetAltRowsEnabled(), GetSelectedAlignment());
        }

        private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyMainTheme();
        }

        private void OnOptionChanged(object sender, RoutedEventArgs e)
        {
            ApplyMainTheme();
        }

        private void OnOptionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyMainTheme();
        }

        private void OnViewModeChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || _bdrSingleView == null || _gridQuadView == null)
                return;

            if (_radioSingleView.IsChecked == true)
            {
                _bdrSingleView.Visibility = Visibility.Visible;
                _gridQuadView.Visibility = Visibility.Collapsed;
            }
            else
            {
                _bdrSingleView.Visibility = Visibility.Collapsed;
                _gridQuadView.Visibility = Visibility.Visible;
            }
        }
    }
}


