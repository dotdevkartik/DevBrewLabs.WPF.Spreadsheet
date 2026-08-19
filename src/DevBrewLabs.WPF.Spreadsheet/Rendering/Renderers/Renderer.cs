using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    /// <summary>
    /// This is an abstract base class for sheet component renderers.
    /// </summary>
    internal abstract class RendererBase
    {
        /// <summary>
        /// Provides the rendering logic.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="topRow"></param>
        /// <param name="leftColumn"></param>
        /// <param name="bottomRow"></param>
        /// <param name="rightColumn"></param>
        public abstract void OnRender(RenderContext context, int topRow, int leftColumn, int bottomRow, int rightColumn);
    }
}
