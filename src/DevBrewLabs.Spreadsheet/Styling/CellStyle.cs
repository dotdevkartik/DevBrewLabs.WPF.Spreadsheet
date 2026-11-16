using System;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.Spreadsheet.Styling
{
    /// <summary>
    /// Represents the visual style applied to cells, rows, columns, or headers.
    /// </summary>
    public class CellStyle : IStyle, IEquatable<IStyle>, IEquatable<CellStyle>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellStyle"/> class with default values.
        /// </summary>
        public CellStyle()
        {
            ForeColor = DrawingColor.Black;
            BackColor = DrawingColor.White;
            FontSize = 14;
            FontFamily = new DrawingFontFamily("Calibri");
            FontWeight = DrawingFontWeight.Regular;
            FontStyle = DrawingFontStyle.Normal;
            Padding = new DrawingThickness(5, 5);
            HorizontalAlignment = CellHorizontalAlignment.Auto;
            VerticalAlignment = CellVerticalAlignment.Auto;
            AllowMultiLineText = false;
            TextTrimming = CellTextTrimming.None;
            TextWrapping = CellTextWrapping.NoWrap;
        }

        /// <inheritdoc />
        public DrawingColor ForeColor { get; set; }
        /// <inheritdoc />
        public DrawingColor BackColor { get; set; }
        /// <inheritdoc />
        public double FontSize { get; set; }
        /// <inheritdoc />
        public DrawingFontFamily FontFamily { get; set; }
        /// <inheritdoc />
        public DrawingFontWeight FontWeight { get; set; }
        /// <inheritdoc />
        public DrawingFontStyle FontStyle { get; set; }
        /// <inheritdoc />
        public DrawingThickness Padding { get; set; }
        /// <inheritdoc />
        public CellVerticalAlignment VerticalAlignment { get; set; }
        /// <inheritdoc />
        public CellHorizontalAlignment HorizontalAlignment { get; set; }
        /// <inheritdoc />
        public bool AllowMultiLineText { get; set; }
        /// <inheritdoc />
        public CellTextTrimming TextTrimming { get; set; }
        /// <inheritdoc />
        public CellTextWrapping TextWrapping { get; set; }

        /// <inheritdoc />
        public IStyle Clone()
        {
            return new CellStyle
            {
                ForeColor = ForeColor,
                BackColor = BackColor,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                Padding = Padding,
                HorizontalAlignment = HorizontalAlignment,
                VerticalAlignment = VerticalAlignment,
                AllowMultiLineText = AllowMultiLineText,
                TextTrimming = TextTrimming,
                TextWrapping = TextWrapping
            };
        }

        /// <inheritdoc />
        public bool Equals(CellStyle other)
        {
            return Equals((IStyle)other);
        }

        /// <inheritdoc />
        public bool Equals(IStyle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return ForeColor.Equals(other.ForeColor) &&
                   BackColor.Equals(other.BackColor) &&
                   FontSize.Equals(other.FontSize) &&
                   Equals(FontFamily, other.FontFamily) &&
                   FontWeight == other.FontWeight &&
                   FontStyle == other.FontStyle &&
                   Padding.Equals(other.Padding) &&
                   VerticalAlignment == other.VerticalAlignment &&
                   HorizontalAlignment == other.HorizontalAlignment &&
                   AllowMultiLineText == other.AllowMultiLineText &&
                   TextTrimming == other.TextTrimming &&
                   TextWrapping == other.TextWrapping;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is IStyle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ForeColor.GetHashCode();
                hash = hash * 31 + BackColor.GetHashCode();
                hash = hash * 31 + FontSize.GetHashCode();
                hash = hash * 31 + (FontFamily != null ? FontFamily.GetHashCode() : 0);
                hash = hash * 31 + (int)FontWeight;
                hash = hash * 31 + (int)FontStyle;
                hash = hash * 31 + Padding.GetHashCode();
                hash = hash * 31 + (int)VerticalAlignment;
                hash = hash * 31 + (int)HorizontalAlignment;
                hash = hash * 31 + AllowMultiLineText.GetHashCode();
                hash = hash * 31 + (int)TextTrimming;
                hash = hash * 31 + (int)TextWrapping;
                return hash;
            }
        }
    }
}