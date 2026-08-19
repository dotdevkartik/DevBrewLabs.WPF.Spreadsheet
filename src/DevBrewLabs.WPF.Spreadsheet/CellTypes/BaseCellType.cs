using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System.Windows;
using System.Windows.Media;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.WPF.Spreadsheet.Rendering;

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
        /// Gets the editor for cell type
        /// </summary>
        /// <returns></returns>
        public abstract EditorBase GetEditor(IStyle style);
    }
}


