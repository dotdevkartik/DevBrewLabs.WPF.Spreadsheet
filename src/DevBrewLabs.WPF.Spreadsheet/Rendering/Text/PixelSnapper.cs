using System;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal static class PixelSnapper
    {
        public static double Snap(double value, double pixelsPerDip)
        {
            return Math.Round(value * pixelsPerDip) / pixelsPerDip;
        }

        public static double SnapLine(double value, double pixelsPerDip, double penThickness = 1.0)
        {
            double halfPenDip = penThickness / 2.0;
            double halfPenPx = halfPenDip * pixelsPerDip;
            return (Math.Round((value + halfPenDip) * pixelsPerDip, MidpointRounding.AwayFromZero) - halfPenPx) / pixelsPerDip;
        }
    }
}
