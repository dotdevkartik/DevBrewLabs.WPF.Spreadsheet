using DevBrewLabs.Spreadsheet.Drawing;
using System;
using System.Collections.Concurrent;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal readonly struct TextLayoutCacheKey : IEquatable<TextLayoutCacheKey>
    {
        public string Text { get; }
        public string FontFamily { get; }
        public double FontSize { get; }
        public DrawingFontWeight FontWeight { get; }
        public DrawingFontStyle FontStyle { get; }
        public double Zoom { get; }
        public double AvailableWidth { get; }
        public bool CharacterEllipses { get; }
        public double PixelsPerDip { get; }

        public TextLayoutCacheKey(
            string text,
            string fontFamily,
            double fontSize,
            DrawingFontWeight fontWeight,
            DrawingFontStyle fontStyle,
            double zoom,
            double availableWidth,
            bool characterEllipses,
            double pixelsPerDip)
        {
            Text = text;
            FontFamily = fontFamily;
            FontSize = fontSize;
            FontWeight = fontWeight;
            FontStyle = fontStyle;
            Zoom = zoom;
            AvailableWidth = availableWidth;
            CharacterEllipses = characterEllipses;
            PixelsPerDip = pixelsPerDip;
        }

        public bool Equals(TextLayoutCacheKey other)
        {
            return Text == other.Text &&
                   FontFamily == other.FontFamily &&
                   FontSize == other.FontSize &&
                   FontWeight == other.FontWeight &&
                   FontStyle == other.FontStyle &&
                   Zoom == other.Zoom &&
                   AvailableWidth == other.AvailableWidth &&
                   CharacterEllipses == other.CharacterEllipses &&
                   PixelsPerDip == other.PixelsPerDip;
        }

        public override bool Equals(object obj)
        {
            return obj is TextLayoutCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (Text != null ? Text.GetHashCode() : 0);
                hash = hash * 31 + (FontFamily != null ? FontFamily.GetHashCode() : 0);
                hash = hash * 31 + FontSize.GetHashCode();
                hash = hash * 31 + FontWeight.GetHashCode();
                hash = hash * 31 + FontStyle.GetHashCode();
                hash = hash * 31 + Zoom.GetHashCode();
                hash = hash * 31 + AvailableWidth.GetHashCode();
                hash = hash * 31 + CharacterEllipses.GetHashCode();
                hash = hash * 31 + PixelsPerDip.GetHashCode();
                return hash;
            }
        }
    }

    internal static class TextLayoutCache
    {
        private static readonly ConcurrentDictionary<TextLayoutCacheKey, TextLayout> _cache = new ConcurrentDictionary<TextLayoutCacheKey, TextLayout>();

        public static TextLayout GetOrCreate(
            string text,
            double availableWidth,
            double scaledFontSize,
            DrawingFontFamily fontFamily,
            double fontSize,
            DrawingFontWeight fontWeight,
            DrawingFontStyle fontStyle,
            RenderContext context,
            bool characterEllipses)
        {
            var key = new TextLayoutCacheKey(text, fontFamily.FamilyName, fontSize, fontWeight, fontStyle, context.Zoom, availableWidth, characterEllipses, context.PixelPerDip);

            if (_cache.TryGetValue(key, out var cachedLayout))
            {
                return cachedLayout;
            }

            var fontResources = Styling.WpfResourceCache.GetFontResources(fontFamily, fontWeight, fontStyle);
            var layout = TextLayoutBuilder.Build(text, availableWidth, scaledFontSize, fontResources.GlyphMetrics, context, characterEllipses);
            _cache.TryAdd(key, layout);
            return layout;
        }

        public static void Clear()
        {
            _cache.Clear();
        }
    }
}


