using System;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// Capability interface for cell editors that provide text-based editing and synchronize with the formula bar.
    /// </summary>
    public interface ITextEditor : ICellEditor
    {
        /// <summary>
        /// Gets or sets the text content of the editor.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Occurs when the text content changes within the editor.
        /// </summary>
        event EventHandler TextChanged;
    }
}
