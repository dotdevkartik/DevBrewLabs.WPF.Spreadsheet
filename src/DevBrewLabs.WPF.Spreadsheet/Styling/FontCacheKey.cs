using System;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.Styling
{
    internal readonly struct FontCacheKey : IEquatable<FontCacheKey>
    {
        public string FontFamily { get; }
        public DrawingFontWeight FontWeight { get; }
        public DrawingFontStyle FontStyle { get; }

        public FontCacheKey(string fontFamily, DrawingFontWeight weight, DrawingFontStyle style)
        {
            FontFamily = fontFamily;
            FontWeight = weight;
            FontStyle = style;
        }

        public bool Equals(FontCacheKey other) =>
            FontFamily == other.FontFamily && FontWeight == other.FontWeight && FontStyle == other.FontStyle;

        public override bool Equals(object obj) => obj is FontCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (FontFamily != null ? FontFamily.GetHashCode() : 0);
                hash = hash * 31 + FontWeight.GetHashCode();
                hash = hash * 31 + FontStyle.GetHashCode();
                return hash;
            }
        }
    }
}
