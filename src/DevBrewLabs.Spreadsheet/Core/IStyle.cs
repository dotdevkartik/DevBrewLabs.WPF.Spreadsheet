using DevBrewLabs.Parserly;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.Spreadsheet
{
    public interface IStyle : ICloneable<IStyle>
    {
        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        DrawingColor ForeColor { get; set; }
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        DrawingColor BackColor { get; set; }
        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        double FontSize { get; set; }
        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        DrawingFontFamily FontFamily { get; set; }
        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        DrawingFontWeight FontWeight { get; set; }
        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        DrawingFontStyle FontStyle { get; set; }
        /// <summary>
        /// Gets or sets the padding.
        /// </summary>
        DrawingThickness Padding { get; set; }
        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        CellVerticalAlignment VerticalAlignment { get; set; }
        /// <summary>
        /// Gets or sets the horizontal aligment.
        /// </summary>
        CellHorizontalAlignment HorizontalAlignment { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to allow multi-line text.
        /// </summary>
        bool AllowMultiLineText { get; set; }
        /// <summary>
        /// Gets or sets the text trimming mode.
        /// </summary>
        CellTextTrimming TextTrimming { get; set; }
        /// <summary>
        /// Gets or sets the text wrapping mode.
        /// </summary>
        CellTextWrapping TextWrapping { get; set; }
    }
}
