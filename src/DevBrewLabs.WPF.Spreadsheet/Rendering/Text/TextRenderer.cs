using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Utils;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal static class TextRenderer
    {
        public static void DrawText(
            RenderContext renderContext,
            string text,
            Rect bounds,
            DrawingFontFamily fontFamily,
            double fontSize,
            DrawingFontWeight fontWeight,
            DrawingFontStyle fontStyle,
            DrawingColor foreColor,
            CellHorizontalAlignment horizontalAlignment = CellHorizontalAlignment.Left,
            CellVerticalAlignment verticalAlignment = CellVerticalAlignment.Bottom,
            CellTextTrimming textTrimming = CellTextTrimming.None,
            bool allowMultiLineText = false)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (!allowMultiLineText)
            {
                text = TextUtils.NormalizeToSingleLine(text);
            }

            double textPadding = renderContext.TextPadding * renderContext.Zoom;
            double availableWidth = bounds.Width - (2 * textPadding);
            if (availableWidth <= 0)
                return;

            var fontResources = Styling.WpfResourceCache.GetFontResources(fontFamily, fontWeight, fontStyle);

            if (!CharacterAnalyzer.IsSupported(text))
            {
                // Fallback behavior for unsupported scripts (e.g. Emoji, Arabic).
                // We convert it to a string of '?' if we have a replacement glyph.
                if (fontResources.GlyphMetrics.ReplacementGlyph != 0)
                {
                    text = new string('?', text.Length);
                }
                else
                {
                    // Skip rendering
                    return;
                }
            }

            double scaledFontSize = fontSize * renderContext.Zoom;

            string[] lines = allowMultiLineText 
                ? TextUtils.GetLines(text) 
                : new[] { text };

            // We pre-calculate total height to support Vertical alignment
            double totalHeight = 0;
            TextLayout[] layouts = new TextLayout[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                layouts[i] = TextLayoutCache.GetOrCreate(
                    line,
                    availableWidth,
                    scaledFontSize,
                    fontFamily,
                    fontSize,
                    fontWeight,
                    fontStyle,
                    renderContext,
                    textTrimming == CellTextTrimming.Character);
                totalHeight += layouts[i].Height;
            }

            double startY;
            switch (verticalAlignment)
            {
                case CellVerticalAlignment.Top:
                    startY = bounds.Top + textPadding;
                    break;
                case CellVerticalAlignment.Center:
                    startY = bounds.Top + (bounds.Height - totalHeight) / 2;
                    if (startY < bounds.Top)
                        startY = bounds.Top;
                    break;
                default: // Bottom
                    startY = bounds.Bottom - textPadding - totalHeight;
                    if (startY < bounds.Top)
                        startY = bounds.Top;
                    break;
            }

            startY = PixelSnapper.Snap(startY, renderContext.PixelPerDip);
            double currentY = startY;
            double ascent = fontResources.GlyphMetrics.Baseline * scaledFontSize;

            for (int i = 0; i < layouts.Length; i++)
            {
                var layout = layouts[i];
                if (layout.GlyphCount > 0)
                {
                    double x;
                    switch (horizontalAlignment)
                    {
                        case CellHorizontalAlignment.Center:
                            x = bounds.Left + (bounds.Width - layout.Width) / 2;
                            break;
                        case CellHorizontalAlignment.Right:
                            x = bounds.Right - textPadding - layout.Width;
                            break;
                        default: // Left
                            x = bounds.Left + textPadding;
                            break;
                    }

                    if (x < bounds.Left + textPadding)
                        x = bounds.Left + textPadding;

                    x = PixelSnapper.Snap(x, renderContext.PixelPerDip);

                    Point baselineOrigin = new Point(x, PixelSnapper.Snap(currentY + ascent, renderContext.PixelPerDip));
                    
                    var glyphRun = GlyphRunFactory.Create(layout, fontResources.GlyphMetrics, scaledFontSize, renderContext, baselineOrigin);
                    
                    if (glyphRun != null)
                    {
                        renderContext.DrawGlyphRun(foreColor, glyphRun);
                    }
                }

                currentY += layout.Height;
            }
        }
    }
}



