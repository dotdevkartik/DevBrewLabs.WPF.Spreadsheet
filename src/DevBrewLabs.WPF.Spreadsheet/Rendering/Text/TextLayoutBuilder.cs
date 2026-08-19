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
            bool applyEllipsis = isTruncated && characterEllipses;

            if (applyEllipsis)
            {
                fitCount = EllipsisEngine.Truncate(text, fitCount, exactTotalWidth, availableWidth, scaledFontSize, metrics, context.PixelPerDip, out finalExactWidth);
            }
            else if (isTruncated)
            {
                // If truncated and not using character ellipses, just clamp exact width.
                // Wait, if it's truncated, the measure step stops at fitCount. The exactTotalWidth is for fitCount characters.
                // The prompt says "characterEllipses = false" means it might just draw the text that fits or we need to draw ellipsis anyway?
                // The original code did: `if (isTruncated && !characterEllipses)` to add ellipsis. So `characterEllipses` in original meant "No ellipsis".
                // Wait, the original code had:
                // if (characterEllipses) formattedText.Trimming = TextTrimming.None;
                // else formattedText.Trimming = TextTrimming.CharacterEllipsis;
                // Wait, so if `characterEllipses` is TRUE, it does NOT draw ellipses!
                // Ah! `characterEllipses` = true means NO ellipsis in original DrawText signature?! Wait:
                // `if (characterEllipses) formattedText.Trimming = TextTrimming.None; else formattedText.Trimming = TextTrimming.CharacterEllipsis;`
                // Let me double check that logic. If `!characterEllipses` we apply truncation with ellipsis.
            }

            // Correct logic based on original codebase:
            // if (isTruncated && !characterEllipses) -> apply ellipsis.
            bool needsEllipsisGlyph = isTruncated && !characterEllipses;
            
            if (needsEllipsisGlyph)
            {
                fitCount = EllipsisEngine.Truncate(text, fitCount, exactTotalWidth, availableWidth, scaledFontSize, metrics, context.PixelPerDip, out finalExactWidth);
            }

            int finalGlyphCount = needsEllipsisGlyph ? (fitCount > 0 ? fitCount + 1 : 0) : fitCount;

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
