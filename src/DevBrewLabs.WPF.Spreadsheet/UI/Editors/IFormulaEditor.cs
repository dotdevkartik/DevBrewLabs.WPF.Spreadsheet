using System.Windows.Controls;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// Capability interface for text-based cell editors that support spreadsheet formula autocomplete suggestions.
    /// </summary>
    public interface IFormulaEditor : ITextEditor
    {
        /// <summary>
        /// Gets the underlying TextBox control to attach formula suggestion tooltips and popups.
        /// </summary>
        TextBox TextBox { get; }
    }
}
