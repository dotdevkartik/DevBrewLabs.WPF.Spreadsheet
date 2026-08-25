using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Represents an interactive or visual sub-element within a spreadsheet cell.
    /// </summary>
    public abstract class CellElement
    {
        /// <summary>
        /// Gets the cursor displayed when hovering over this element.
        /// </summary>
        public virtual Cursor Cursor => Cursors.Hand;

        /// <summary>
        /// Calculates the bounding rectangle of this element relative to the cell surface.
        /// </summary>
        /// <param name="cellRect">The cell's bounding rectangle in surface coordinates.</param>
        /// <param name="zoom">The active zoom factor.</param>
        /// <returns>The element bounding rectangle.</returns>
        public abstract Rect GetBounds(Rect cellRect, double zoom);

        /// <summary>
        /// Renders this element on the provided DrawingContext.
        /// </summary>
        /// <param name="dc">The DrawingContext to draw on.</param>
        /// <param name="bounds">The bounding box of the element.</param>
        /// <param name="state">The current interaction state (Normal, Hover, Pressed, Disabled).</param>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        public abstract void Draw(DrawingContext dc, Rect bounds, CellElementState state, ISheetView view, int row, int col);

        /// <summary>
        /// Handles click events on this element.
        /// </summary>
        public virtual void OnClick(ISheetView view, int row, int col) { }

        /// <summary>
        /// Handles mouse down events on this element.
        /// </summary>
        public virtual void OnMouseDown(ISheetView view, int row, int col) { }

        /// <summary>
        /// Handles mouse up events on this element.
        /// </summary>
        public virtual void OnMouseUp(ISheetView view, int row, int col) { }
    }
}
