using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public abstract class BaseCellType : ICellType
    {
        internal virtual void DrawCell(DrawingContext drawingContext, object value, IStyle style, IFormatter formatter, Rect cellRect, RenderContext renderContext)
        {
            if (style.BackColor != CellColor.Transparent)
            {
                drawingContext.DrawRectangle(Styling.WpfResourceCache.GetBrush(style.BackColor), null, cellRect);
            }
        }

        /// <summary>
        /// Gets the editor for cell type
        /// </summary>
        /// <returns></returns>
        public abstract EditorBase GetEditor(IStyle style);
    }
}


