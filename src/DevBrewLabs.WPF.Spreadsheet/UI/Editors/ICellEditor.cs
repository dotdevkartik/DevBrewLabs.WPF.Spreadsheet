using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// Contract implemented by controls or wrappers acting as cell editors within a spreadsheet.
    /// </summary>
    public interface ICellEditor
    {
        /// <summary>
        /// Gets the visual framework element to mount onto the spreadsheet interaction layer canvas.
        /// </summary>
        FrameworkElement Element { get; }

        /// <summary>
        /// Initializes the editor with cell data, styling, and trigger state.
        /// </summary>
        /// <param name="context">The editing context.</param>
        void StartEdit(IEditorContext context);

        /// <summary>
        /// Retrieves the current value or formula from the editor to be committed to the cell.
        /// </summary>
        /// <returns>The edited value.</returns>
        object GetValue();

        /// <summary>
        /// Validates the current editor state before committing.
        /// </summary>
        /// <param name="errorMessage">Output error message if validation fails.</param>
        /// <returns>True if the editor state is valid and can be committed; otherwise false.</returns>
        bool Validate(out string errorMessage);

        /// <summary>
        /// Updates the editor size and layout relative to the cell content rectangle and active zoom factor.
        /// </summary>
        /// <param name="contentRect">The available cell content rectangle in surface coordinates.</param>
        /// <param name="zoomFactor">The active zoom factor.</param>
        void UpdateLayout(Rect contentRect, double zoomFactor);

        /// <summary>
        /// Determines whether this editor wants to process the specified key event internally.
        /// </summary>
        /// <param name="e">The key event args.</param>
        /// <returns>True if the editor consumed or handled the key; false to allow standard spreadsheet navigation.</returns>
        bool HandlesKeyDown(KeyEventArgs e);

        /// <summary>
        /// Cleans up the editor state, unhooks events, and closes any associated popups.
        /// </summary>
        void EndEdit();
    }
}
