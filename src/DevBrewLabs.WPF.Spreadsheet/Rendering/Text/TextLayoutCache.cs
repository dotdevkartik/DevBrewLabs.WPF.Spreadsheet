using DevBrewLabs.Spreadsheet;
using System;
using System.Collections.Concurrent;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering.Text
{
    internal readonly struct TextLayoutCacheKey : IEquatable<TextLayoutCacheKey>
    {
        public string Text { get; }
        public IStyle Style { get; }
        public double Zoom { get; }
        public double AvailableWidth { get; }
        public bool CharacterEllipses { get; }
        public double PixelsPerDip { get; }

        public TextLayoutCacheKey(string text, IStyle style, double zoom, double availableWidth, bool characterEllipses, double pixelsPerDip)
        {
            Text = text;
            Style = style;
            Zoom = zoom;
            AvailableWidth = availableWidth;
            CharacterEllipses = characterEllipses;
            PixelsPerDip = pixelsPerDip;
        }

        public bool Equals(TextLayoutCacheKey other)
        {
            return Text == other.Text &&
                   Style == other.Style &&
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
                hash = hash * 31 + (Style != null ? Style.GetHashCode() : 0);
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
            IStyle style,
            RenderContext context,
            bool characterEllipses)
        {
            var key = new TextLayoutCacheKey(text, style, context.Zoom, availableWidth, characterEllipses, context.PixelPerDip);

            if (_cache.TryGetValue(key, out var cachedLayout))
            {
                return cachedLayout;
            }

            var layout = TextLayoutBuilder.Build(text, availableWidth, scaledFontSize, Styling.WpfResourceCache.GetFontResources(style).GlyphMetrics, context, characterEllipses);
            _cache.TryAdd(key, layout);
            return layout;
        }

        public static void Clear()
        {
            _cache.Clear();
        }
    }
}


