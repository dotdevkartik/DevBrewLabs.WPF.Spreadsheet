using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.Styling
{
    internal static class WpfResourceCache
    {
        private static readonly Dictionary<CellColor, Brush> _brushCache = new Dictionary<CellColor, Brush>();
        private static readonly Dictionary<FontCacheKey, WpfFontResources> _fontCache = new Dictionary<FontCacheKey, WpfFontResources>();

        public static Brush GetBrush(CellColor color)
        {
            if (!_brushCache.TryGetValue(color, out Brush brush))
            {
                brush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                brush.Freeze();
                _brushCache[color] = brush;
            }
            return brush;
        }

        public static WpfFontResources GetFontResources(IStyle style)
        {
            var key = new FontCacheKey(style.FontFamily.FamilyName, style.FontWeight, style.FontStyle);

            if (!_fontCache.TryGetValue(key, out WpfFontResources resources))
            {
                var wpfFontFamily = new FontFamily(style.FontFamily.FamilyName);
                var wpfFontWeight = ToWpfFontWeight(style.FontWeight);
                var wpfFontStyle = ToWpfFontStyle(style.FontStyle);

                var typeface = new Typeface(wpfFontFamily, wpfFontStyle, wpfFontWeight, FontStretches.Normal, new FontFamily("Arial"));
                
                typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface);
                GlyphMetrics metrics = glyphTypeface != null ? new GlyphMetrics(glyphTypeface) : null;

                resources = new WpfFontResources(typeface, glyphTypeface, metrics);
                _fontCache[key] = resources;
            }

            return resources;
        }

        internal static FontFamily ToWpfFontFamily(CellFontFamily fontFamily)
        {
            return new FontFamily(fontFamily.FamilyName);
        }

        internal static FontWeight ToWpfFontWeight(CellFontWeight weight)
        {
            switch (weight)
            {
                case CellFontWeight.Bold:
                    return FontWeights.Bold;
                case CellFontWeight.Normal:
                    return FontWeights.Normal;
                default:
                    return FontWeights.Regular;
            }
        }

        internal static FontStyle ToWpfFontStyle(CellFontStyle style)
        {
            switch (style)
            {
                case CellFontStyle.Italic:
                    return FontStyles.Italic;
                case CellFontStyle.Oblique:
                    return FontStyles.Oblique;
                default:
                    return FontStyles.Normal;
            }
        }
    }
}
