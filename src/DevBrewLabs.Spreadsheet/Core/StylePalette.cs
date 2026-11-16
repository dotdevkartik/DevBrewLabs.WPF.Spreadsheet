using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.Core
{
    /// <summary>
    /// Shared style palette mapping IStyle instances to 16-bit integer IDs.
    /// </summary>
    public class StylePalette : IStylePalette
    {
        private readonly Dictionary<ushort, IStyle> _idToStyleMap;
        private readonly Dictionary<IStyle, ushort> _styleToIdMap;
        private ushort _nextId;

        public const ushort DefaultStyleId = 0;

        public StylePalette()
        {
            _idToStyleMap = new Dictionary<ushort, IStyle>();
            _styleToIdMap = new Dictionary<IStyle, ushort>(StyleValueComparer.Instance);
            _nextId = 1;

            _idToStyleMap[DefaultStyleId] = null;
        }

        /// <summary>
        /// Gets the IStyle instance corresponding to the style ID.
        /// </summary>
        public IStyle GetStyle(ushort styleId)
        {
            if (_idToStyleMap.TryGetValue(styleId, out var style))
                return style;

            return null;
        }

        /// <summary>
        /// Gets or registers a style in the palette, returning its 16-bit ID.
        /// </summary>
        public ushort GetOrAdd(IStyle style)
        {
            if (style == null)
                throw new ArgumentNullException(nameof(style));

            if (_styleToIdMap.TryGetValue(style, out var existingId))
                return existingId;

            ushort id = _nextId++;
            _idToStyleMap[id] = style;
            _styleToIdMap[style] = id;
            return id;
        }

        /// <summary>
        /// Clears all registered styles in the palette.
        /// </summary>
        public void Clear()
        {
            _idToStyleMap.Clear();
            _styleToIdMap.Clear();
            _idToStyleMap[DefaultStyleId] = null;
            _nextId = 1;
        }

        private sealed class StyleValueComparer : IEqualityComparer<IStyle>
        {
            public static readonly StyleValueComparer Instance = new StyleValueComparer();

            public bool Equals(IStyle x, IStyle y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x == null || y == null) return false;

                return x.ForeColor.Equals(y.ForeColor) &&
                       x.BackColor.Equals(y.BackColor) &&
                       x.FontSize.Equals(y.FontSize) &&
                       Equals(x.FontFamily, y.FontFamily) &&
                       x.FontWeight == y.FontWeight &&
                       x.FontStyle == y.FontStyle &&
                       x.Padding.Equals(y.Padding) &&
                       x.VerticalAlignment == y.VerticalAlignment &&
                       x.HorizontalAlignment == y.HorizontalAlignment &&
                       x.AllowMultiLineText == y.AllowMultiLineText &&
                       x.TextTrimming == y.TextTrimming &&
                       x.TextWrapping == y.TextWrapping;
            }

            public int GetHashCode(IStyle obj)
            {
                if (obj == null) return 0;

                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + obj.ForeColor.GetHashCode();
                    hash = hash * 31 + obj.BackColor.GetHashCode();
                    hash = hash * 31 + obj.FontSize.GetHashCode();
                    hash = hash * 31 + (obj.FontFamily != null ? obj.FontFamily.GetHashCode() : 0);
                    hash = hash * 31 + (int)obj.FontWeight;
                    hash = hash * 31 + (int)obj.FontStyle;
                    hash = hash * 31 + obj.Padding.GetHashCode();
                    hash = hash * 31 + (int)obj.VerticalAlignment;
                    hash = hash * 31 + (int)obj.HorizontalAlignment;
                    hash = hash * 31 + obj.AllowMultiLineText.GetHashCode();
                    hash = hash * 31 + (int)obj.TextTrimming;
                    hash = hash * 31 + (int)obj.TextWrapping;
                    return hash;
                }
            }
        }
    }
}
