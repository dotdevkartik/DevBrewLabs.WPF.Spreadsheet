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
            _styleToIdMap = new Dictionary<IStyle, ushort>();
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
    }
}
