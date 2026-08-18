using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.Spreadsheet.Styling
{
    public class CellStyle : IStyle
    {
        public CellStyle()
        {
            ForeColor = CellColor.Black;
            BackColor = CellColor.White;
            FontSize = 14;
            FontFamily = new CellFontFamily("Calibri");
            FontWeight = CellFontWeight.Regular;
            FontStyle = CellFontStyle.Normal;
            Padding = new CellThickness(5, 5);
            HorizontalAlignment = CellHorizontalAlignment.Auto;
            VerticalAlignment = CellVerticalAlignment.Auto;
            AllowMultiLineText = false;
            TextTrimming = CellTextTrimming.None;
            TextWrapping = CellTextWrapping.NoWrap;
        }

        public CellColor ForeColor { get; set; }
        public CellColor BackColor { get; set; }
        public double FontSize { get; set; }
        public CellFontFamily FontFamily { get; set; }
        public CellFontWeight FontWeight { get; set; }
        public CellFontStyle FontStyle { get; set; }
        public CellThickness Padding { get; set; }
        public CellVerticalAlignment VerticalAlignment { get; set; }
        public CellHorizontalAlignment HorizontalAlignment { get; set; }
        public bool AllowMultiLineText { get; set; }
        public CellTextTrimming TextTrimming { get; set; }
        public CellTextWrapping TextWrapping { get; set; }
    }
}