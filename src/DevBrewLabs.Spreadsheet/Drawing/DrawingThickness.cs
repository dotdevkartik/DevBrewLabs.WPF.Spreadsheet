using System;

namespace DevBrewLabs.Spreadsheet.Drawing
{
    /// <summary>
    /// Describes the thickness of a frame or padding around a rectangle.
    /// </summary>
    public struct DrawingThickness : IEquatable<DrawingThickness>
    {
        /// <summary>Gets or sets the left thickness.</summary>
        public double Left { get; set; }
        /// <summary>Gets or sets the top thickness.</summary>
        public double Top { get; set; }
        /// <summary>Gets or sets the bottom thickness.</summary>
        public double Bottom { get; set; }
        /// <summary>Gets or sets the right thickness.</summary>
        public double Right { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingThickness"/> struct with individual side lengths.
        /// </summary>
        public DrawingThickness(double left, double top, double bottom, double right)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingThickness"/> struct with symmetric horizontal and vertical lengths.
        /// </summary>
        public DrawingThickness(double left, double top)
        {
            Left = left;
            Top = top;
            Right = left;
            Bottom = top;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingThickness"/> struct with a uniform thickness.
        /// </summary>
        public DrawingThickness(double thickness)
        {
            Left = thickness;
            Top = thickness;
            Right = thickness;
            Bottom = thickness;
        }

        /// <inheritdoc />
        public bool Equals(DrawingThickness other)
        {
            return Left.Equals(other.Left) &&
                   Top.Equals(other.Top) &&
                   Right.Equals(other.Right) &&
                   Bottom.Equals(other.Bottom);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is DrawingThickness other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Left.GetHashCode();
                hash = hash * 31 + Top.GetHashCode();
                hash = hash * 31 + Right.GetHashCode();
                hash = hash * 31 + Bottom.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="DrawingThickness"/> values are equal.
        /// </summary>
        public static bool operator ==(DrawingThickness left, DrawingThickness right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="DrawingThickness"/> values are not equal.
        /// </summary>
        public static bool operator !=(DrawingThickness left, DrawingThickness right)
        {
            return !left.Equals(right);
        }
    }
}
