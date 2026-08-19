using System;

namespace DevBrewLabs.Spreadsheet.Drawing
{
    public struct DrawingPen : IEquatable<DrawingPen>
    {
        public DrawingColor Color { get; }
        public double Thickness { get; }

        public DrawingPen(DrawingColor color, double thickness)
        {
            Color = color;
            Thickness = thickness;
        }

        public bool Equals(DrawingPen pen)
        {
            return Thickness == pen.Thickness && Color == pen.Color;
        }

        public static bool operator ==(DrawingPen pen1, DrawingPen pen2)
        {
            return pen1.Equals(pen2);
        }

        public static bool operator !=(DrawingPen pen1, DrawingPen pen2)
        {
            return !pen1.Equals(pen2);
        }

        public override bool Equals(object obj)
        {
            return obj is DrawingColor && Equals((DrawingColor)obj);
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode() ^ Thickness.GetHashCode();
        }
    }
}
