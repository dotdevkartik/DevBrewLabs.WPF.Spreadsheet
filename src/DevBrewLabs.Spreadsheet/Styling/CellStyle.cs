using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.Spreadsheet.Styling
{
    public class CellStyle : IStyle
    {
        public CellStyle()
        {
            ForeColor = DrawingColor.Black;
            BackColor = DrawingColor.White;
            FontSize = 14;
            FontFamily = new DrawingFontFamily("Calibri");
            FontWeight = DrawingFontWeight.Regular;
            FontStyle = DrawingFontStyle.Normal;
            Padding = new DrawingThickness(5, 5);
            HorizontalAlignment = CellHorizontalAlignment.Auto;
            VerticalAlignment = CellVerticalAlignment.Auto;
            AllowMultiLineText = false;
            TextTrimming = CellTextTrimming.None;
            TextWrapping = CellTextWrapping.NoWrap;
        }

        public DrawingColor ForeColor { get; set; }
        public DrawingColor BackColor { get; set; }
        public double FontSize { get; set; }
        public DrawingFontFamily FontFamily { get; set; }
        public DrawingFontWeight FontWeight { get; set; }
        public DrawingFontStyle FontStyle { get; set; }
        public DrawingThickness Padding { get; set; }
        public CellVerticalAlignment VerticalAlignment { get; set; }
        public CellHorizontalAlignment HorizontalAlignment { get; set; }
        public bool AllowMultiLineText { get; set; }
        public CellTextTrimming TextTrimming { get; set; }
        public CellTextWrapping TextWrapping { get; set; }

        public IStyle Clone()
        {
            return new CellStyle
            {
                ForeColor = ForeColor,
                BackColor = BackColor,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                Padding = Padding,
                HorizontalAlignment = HorizontalAlignment,
                VerticalAlignment = VerticalAlignment,
                AllowMultiLineText = AllowMultiLineText,
                TextTrimming = TextTrimming,
                TextWrapping = TextWrapping
            };
        }
    }
}