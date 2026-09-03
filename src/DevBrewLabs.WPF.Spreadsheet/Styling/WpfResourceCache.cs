using System;
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
        private static readonly Dictionary<DrawingColor, Brush> _brushCache = new Dictionary<DrawingColor, Brush>();
        private static readonly Dictionary<DrawingPen, Pen> _penCache = new Dictionary<DrawingPen, Pen>();
        private static readonly Dictionary<BrushPenKey, Pen> _brushPenCache = new Dictionary<BrushPenKey, Pen>();
        private static readonly Dictionary<FontCacheKey, WpfFontResources> _fontCache = new Dictionary<FontCacheKey, WpfFontResources>();

        public static Brush GetBrush(DrawingColor color)
        {
            if (!_brushCache.TryGetValue(color, out Brush brush))
            {
                brush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                brush.Freeze();
                _brushCache[color] = brush;
            }
            return brush;
        }

        public static Pen GetPen(DrawingPen drawingPen)
        {
            if (!_penCache.TryGetValue(drawingPen, out Pen pen))
            {
                pen = new Pen(GetBrush(drawingPen.Color), drawingPen.Thickness);
                pen.Freeze();
                _penCache[drawingPen] = pen;
            }
            return pen;
        }

        public static Pen GetPen(Brush brush, double thickness, PenLineCap lineCap = PenLineCap.Flat, PenLineJoin lineJoin = PenLineJoin.Miter)
        {
            if (brush == null) return null;
            var key = new BrushPenKey(brush, thickness, lineCap, lineJoin);
            if (!_brushPenCache.TryGetValue(key, out Pen pen))
            {
                pen = new Pen(brush, thickness)
                {
                    StartLineCap = lineCap,
                    EndLineCap = lineCap,
                    LineJoin = lineJoin
                };
                if (pen.CanFreeze) pen.Freeze();
                _brushPenCache[key] = pen;
            }
            return pen;
        }

        public static WpfFontResources GetFontResources(DrawingFontFamily fontFamily, DrawingFontWeight weight, DrawingFontStyle style)
        {
            string familyName = fontFamily?.FamilyName;
            if (string.IsNullOrEmpty(familyName))
                familyName = "Segoe UI";
            return GetFontResources(familyName, weight, style);
        }

        public static WpfFontResources GetFontResources(string fontFamily, DrawingFontWeight weight, DrawingFontStyle style)
        {
            if (string.IsNullOrEmpty(fontFamily))
                fontFamily = "Segoe UI";

            var key = new FontCacheKey(fontFamily, weight, style);

            if (!_fontCache.TryGetValue(key, out WpfFontResources resources))
            {
                var wpfFontFamily = new FontFamily(fontFamily);
                var wpfFontWeight = ToWpfFontWeight(weight);
                var wpfFontStyle = ToWpfFontStyle(style);

                var typeface = new Typeface(wpfFontFamily, wpfFontStyle, wpfFontWeight, FontStretches.Normal, new FontFamily("Arial"));
                
                typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface);
                GlyphMetrics metrics = glyphTypeface != null ? new GlyphMetrics(glyphTypeface) : null;

                resources = new WpfFontResources(typeface, glyphTypeface, metrics);
                _fontCache[key] = resources;
            }

            return resources;
        }

        public static WpfFontResources GetFontResources(IStyle style)
        {
            return GetFontResources(style.FontFamily.FamilyName, style.FontWeight, style.FontStyle);
        }

        internal static FontFamily ToWpfFontFamily(DrawingFontFamily fontFamily)
        {
            return new FontFamily(fontFamily.FamilyName);
        }

        internal static FontWeight ToWpfFontWeight(DrawingFontWeight weight)
        {
            switch (weight)
            {
                case DrawingFontWeight.Bold:
                    return FontWeights.Bold;
                case DrawingFontWeight.Normal:
                    return FontWeights.Normal;
                default:
                    return FontWeights.Regular;
            }
        }

        internal static FontStyle ToWpfFontStyle(DrawingFontStyle style)
        {
            switch (style)
            {
                case DrawingFontStyle.Italic:
                    return FontStyles.Italic;
                case DrawingFontStyle.Oblique:
                    return FontStyles.Oblique;
                default:
                    return FontStyles.Normal;
            }
        }
    }

    internal readonly struct BrushPenKey : IEquatable<BrushPenKey>
    {
        public Brush Brush { get; }
        public double Thickness { get; }
        public PenLineCap LineCap { get; }
        public PenLineJoin LineJoin { get; }

        public BrushPenKey(Brush brush, double thickness, PenLineCap lineCap = PenLineCap.Flat, PenLineJoin lineJoin = PenLineJoin.Miter)
        {
            Brush = brush;
            Thickness = thickness;
            LineCap = lineCap;
            LineJoin = lineJoin;
        }

        public bool Equals(BrushPenKey other)
        {
            return Equals(Brush, other.Brush) &&
                   Thickness.Equals(other.Thickness) &&
                   LineCap == other.LineCap &&
                   LineJoin == other.LineJoin;
        }

        public override bool Equals(object obj)
        {
            return obj is BrushPenKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (Brush != null ? Brush.GetHashCode() : 0);
                hash = (hash * 397) ^ Thickness.GetHashCode();
                hash = (hash * 397) ^ (int)LineCap;
                hash = (hash * 397) ^ (int)LineJoin;
                return hash;
            }
        }
    }
}
