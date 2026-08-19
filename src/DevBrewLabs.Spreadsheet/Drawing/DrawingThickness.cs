namespace DevBrewLabs.Spreadsheet.Drawing
{
    public struct DrawingThickness
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Right { get; set; }

        public DrawingThickness(double left, double top, double bottom, double right)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public DrawingThickness(double left, double top)
        {
            Left = left;
            Top = top;
            Right = 0;
            Bottom = 0;
        }

        public DrawingThickness(double thickness)
        {
            Left = thickness;
            Top = thickness;
            Right = thickness;
            Bottom = thickness;
        }
    }
}
