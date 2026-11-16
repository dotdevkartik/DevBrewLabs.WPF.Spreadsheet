using DevBrewLabs.Parserly;
using System;

namespace DevBrewLabs.Spreadsheet.Drawing
{
    /// <summary>
    /// Represents a font family definition.
    /// </summary>
    public class DrawingFontFamily : IEquatable<DrawingFontFamily>
    {
        /// <summary>
        /// Gets the font family name.
        /// </summary>
        public string FamilyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingFontFamily"/> class.
        /// </summary>
        /// <param name="familyName">The font family name.</param>
        public DrawingFontFamily(string familyName)
        {
            FamilyName = familyName;
        }

        /// <inheritdoc />
        public bool Equals(DrawingFontFamily other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FamilyName, other.FamilyName, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as DrawingFontFamily);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FamilyName != null ? StringComparer.Ordinal.GetHashCode(FamilyName) : 0;
        }

        /// <summary>
        /// Determines whether two <see cref="DrawingFontFamily"/> instances have the same value.
        /// </summary>
        public static bool operator ==(DrawingFontFamily left, DrawingFontFamily right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two <see cref="DrawingFontFamily"/> instances have different values.
        /// </summary>
        public static bool operator !=(DrawingFontFamily left, DrawingFontFamily right)
        {
            return !Equals(left, right);
        }
    }
}
