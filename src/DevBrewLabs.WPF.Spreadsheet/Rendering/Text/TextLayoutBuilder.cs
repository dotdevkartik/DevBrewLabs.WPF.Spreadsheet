using System;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal static class TextLayoutBuilder
    {
        public static TextLayout Build(
            string text,
            double availableWidth,
            double scaledFontSize,
            GlyphMetrics metrics,
            RenderContext context,
            bool characterEllipses)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new TextLayout(Array.Empty<ushort>(), Array.Empty<double>(), 0, 0, 0, false);
            }

            TextMeasurer.Measure(text, availableWidth, scaledFontSize, metrics, context.PixelPerDip, out int fitCount, out double exactTotalWidth, out bool isTruncated);

            double finalExactWidth = exactTotalWidth;
            bool needsEllipsisGlyph = isTruncated && characterEllipses;

            if (needsEllipsisGlyph)
            {
                fitCount = EllipsisEngine.Truncate(text, fitCount, exactTotalWidth, availableWidth, scaledFontSize, metrics, context.PixelPerDip, out finalExactWidth);
            }

            int finalGlyphCount = fitCount;
            if (needsEllipsisGlyph)
            {
                if (fitCount > 0 || (fitCount == 0 && finalExactWidth > 0))
                {
                    finalGlyphCount = fitCount + 1;
                }
                else
                {
                    finalGlyphCount = 0;
                }
            }

            if (finalGlyphCount == 0)
            {
                return new TextLayout(Array.Empty<ushort>(), Array.Empty<double>(), 0, 0, 0, isTruncated);
            }

            ushort[] glyphIndices = new ushort[finalGlyphCount];
            double[] advanceWidths = new double[finalGlyphCount];

            double runningExactX = 0;
            
            for (int i = 0; i < fitCount; i++)
            {
                char c = text[i];
                ushort glyph;
                double exactAdvance;

                if (c < 128)
                {
                    glyph = metrics.AsciiGlyphs[c];
                    exactAdvance = metrics.AsciiAdvances[c] * scaledFontSize;
                }
                else if (metrics.CharacterToGlyphMap.TryGetValue(c, out glyph))
                {
                    exactAdvance = metrics.AdvanceWidthMap[glyph] * scaledFontSize;
                }
                else
                {
                    glyph = metrics.ReplacementGlyph;
                    exactAdvance = metrics.ReplacementAdvance * scaledFontSize;
                }

                glyphIndices[i] = glyph;

                double exactNextX = runningExactX + exactAdvance;
                double snappedCurrentX = Math.Round(runningExactX * context.PixelPerDip) / context.PixelPerDip;
                double snappedNextX = Math.Round(exactNextX * context.PixelPerDip) / context.PixelPerDip;

                advanceWidths[i] = snappedNextX - snappedCurrentX;
                runningExactX = exactNextX;
            }

            if (needsEllipsisGlyph && fitCount < finalGlyphCount)
            {
                glyphIndices[fitCount] = metrics.EllipsisGlyph;
                
                double exactAdvance = metrics.EllipsisAdvance * scaledFontSize;
                double exactNextX = runningExactX + exactAdvance;
                double snappedCurrentX = Math.Round(runningExactX * context.PixelPerDip) / context.PixelPerDip;
                double snappedNextX = Math.Round(exactNextX * context.PixelPerDip) / context.PixelPerDip;

                advanceWidths[fitCount] = snappedNextX - snappedCurrentX;
                runningExactX = exactNextX;
            }

            double totalSnappedWidth = Math.Round(runningExactX * context.PixelPerDip) / context.PixelPerDip;
            double height = metrics.Height * scaledFontSize;

            return new TextLayout(glyphIndices, advanceWidths, totalSnappedWidth, height, finalGlyphCount, isTruncated);
        }
    }
}
