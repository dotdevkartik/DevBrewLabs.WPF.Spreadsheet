using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal static class GlyphRunFactory
    {
        public static GlyphRun Create(
            TextLayout layout,
            GlyphMetrics metrics,
            double scaledFontSize,
            RenderContext context,
            Point baselineOrigin)
        {
            if (layout.GlyphCount == 0)
                return null;

            return new GlyphRun(
                metrics.GlyphTypeface,
                0,                                  // bidiLevel
                false,                              // sideways
                scaledFontSize,                     // renderingEmSize
                (float)context.PixelPerDip,
                layout.GlyphIndices,
                baselineOrigin,
                layout.AdvanceWidths,
                null,                               // glyphOffsets
                null,                               // characters
                null,                               // deviceFontName
                null,                               // clusterMap
                null,                               // caretStops
                null);                              // language
        }
    }
}
