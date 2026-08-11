using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Utils;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal static class TextRenderer
    {
        public static void DrawText(
            DrawingContext drawingContext,
            string text,
            Rect bounds,
            IStyle style,
            RenderContext renderContext,
            CellHorizontalAlignment? alignment = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (!style.AllowMultiLineText)
            {
                text = TextUtils.NormalizeToSingleLine(text);
            }

            double availableWidth = bounds.Width - (2 * renderContext.TextPadding);
            if (availableWidth <= 0)
                return;

            if (!CharacterAnalyzer.IsSupported(text))
            {
                // Fallback behavior for unsupported scripts (e.g. Emoji, Arabic).
                // We convert it to a string of '?' if we have a replacement glyph.
                if (Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics.ReplacementGlyph != 0)
                {
                    text = new string('?', text.Length);
                }
                else
                {
                    // Skip rendering
                    return;
                }
            }

            double scaledFontSize = style.FontSize * renderContext.Zoom;

            string[] lines = style.AllowMultiLineText 
                ? TextUtils.GetLines(text) 
                : new[] { text };

            // We pre-calculate total height to support Vertical alignment
            double totalHeight = 0;
            TextLayout[] layouts = new TextLayout[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                layouts[i] = TextLayoutCache.GetOrCreate(line, availableWidth, scaledFontSize, style, renderContext, style.TextTrimming == CellTextTrimming.Character);
                totalHeight += layouts[i].Height;
            }

            double startY;
            switch (style.VerticalAlignment)
            {
                case CellVerticalAlignment.Top:
                    startY = bounds.Top + renderContext.TextPadding;
                    break;
                case CellVerticalAlignment.Center:
                    startY = bounds.Top + (bounds.Height - totalHeight) / 2;
                    if (startY < bounds.Top)
                        startY = bounds.Top;
                    break;
                default: // Bottom
                    startY = bounds.Bottom - renderContext.TextPadding - totalHeight;
                    if (startY < bounds.Top)
                        startY = bounds.Top;
                    break;
            }

            startY = PixelSnapper.Snap(startY, renderContext.PixelsPerDip);
            double currentY = startY;
            double ascent = Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics.Baseline * scaledFontSize;

            for (int i = 0; i < layouts.Length; i++)
            {
                var layout = layouts[i];
                if (layout.GlyphCount > 0)
                {
                    double x;
                    var effectiveAlignment = alignment ?? style.HorizontalAlignment;
                    switch (effectiveAlignment)
                    {
                        case CellHorizontalAlignment.Center:
                            x = bounds.Left + (bounds.Width - layout.Width) / 2;
                            break;
                        case CellHorizontalAlignment.Right:
                            x = bounds.Right - renderContext.TextPadding - layout.Width;
                            break;
                        default: // Left
                            x = bounds.Left + renderContext.TextPadding;
                            break;
                    }

                    if (x < bounds.Left + renderContext.TextPadding)
                        x = bounds.Left + renderContext.TextPadding;

                    x = PixelSnapper.Snap(x, renderContext.PixelsPerDip);

                    Point baselineOrigin = new Point(x, PixelSnapper.Snap(currentY + ascent, renderContext.PixelsPerDip));
                    
                    var glyphRun = GlyphRunFactory.Create(layout, Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics, scaledFontSize, renderContext, baselineOrigin);
                    
                    if (glyphRun != null)
                    {
                        drawingContext.DrawGlyphRun(Styling.WpfResourceCache.GetBrush(style.ForeColor), glyphRun);
                    }
                }

                currentY += layout.Height;
            }
        }
    }
}



