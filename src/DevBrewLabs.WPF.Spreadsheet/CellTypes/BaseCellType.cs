using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet.Rendering;

using System.Collections.Generic;
using System.Linq;
using DevBrewLabs.WPF.Spreadsheet.Elements;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public abstract class BaseCellType : ICellType
    {
        internal virtual void DrawCell(RenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            if (style.BackColor != DrawingColor.Transparent)
            {
                renderContext.DrawRectangle(style.BackColor, null, cellRect);
            }
        }

        /// <summary>
        /// Gets any interactive or visual sub-elements for the cell.
        /// </summary>
        public virtual IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            return Enumerable.Empty<CellElement>();
        }

        /// <summary>
        /// Returns the available content area for cell text rendering and in-place editors, excluding cell elements.
        /// </summary>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        /// <param name="cellRect">The cell rectangle in surface coordinates.</param>
        /// <param name="zoom">The active zoom factor.</param>
        /// <returns>The available content rectangle.</returns>
        public virtual Rect GetContentRect(ISheetView view, int row, int col, Rect cellRect, double zoom)
        {
            return cellRect;
        }

        /// <summary>
        /// Handles click events routed from sub-elements attached to this cell.
        /// </summary>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        /// <param name="element">The element that was clicked.</param>
        public virtual void OnElementClick(ISheetView view, int row, int col, CellElement element)
        {
        }

        /// <summary>
        /// Handles mouse down events routed from sub-elements attached to this cell.
        /// </summary>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        /// <param name="element">The element where mouse down occurred.</param>
        public virtual void OnElementMouseDown(ISheetView view, int row, int col, CellElement element)
        {
        }

        /// <summary>
        /// Handles mouse up events routed from sub-elements attached to this cell.
        /// </summary>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        /// <param name="element">The element where mouse up occurred.</param>
        public virtual void OnElementMouseUp(ISheetView view, int row, int col, CellElement element)
        {
        }

        /// <summary>
        /// Gets the editor for cell type
        /// </summary>
        /// <returns></returns>
        public abstract EditorBase GetEditor(IStyle style);
    }
}


