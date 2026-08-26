using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// Provides context and cell metadata to an editor when starting or managing an edit session.
    /// </summary>
    public interface IEditorContext
    {
        /// <summary>
        /// Gets the active sheet view hosting the edit session.
        /// </summary>
        ISheetView SheetView { get; }

        /// <summary>
        /// Gets the worksheet containing the cell being edited.
        /// </summary>
        IWorksheet Worksheet { get; }

        /// <summary>
        /// Gets the row index of the cell being edited.
        /// </summary>
        int Row { get; }

        /// <summary>
        /// Gets the column index of the cell being edited.
        /// </summary>
        int Column { get; }

        /// <summary>
        /// Gets the cell bounding rectangle in unscaled surface coordinates.
        /// </summary>
        Rect CellBounds { get; }

        /// <summary>
        /// Gets the active zoom factor of the sheet view.
        /// </summary>
        double ZoomFactor { get; }

        /// <summary>
        /// Gets the raw cell value before editing began.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the cell formula if the cell contains one, otherwise null.
        /// </summary>
        string Formula { get; }

        /// <summary>
        /// Gets the formatted display text of the cell.
        /// </summary>
        string FormattedText { get; }

        /// <summary>
        /// Gets the resolved style of the cell.
        /// </summary>
        IStyle Style { get; }

        /// <summary>
        /// Gets the cell formatter if configured, otherwise null.
        /// </summary>
        IFormatter Formatter { get; }

        /// <summary>
        /// Gets how this editing session was triggered.
        /// </summary>
        EditTrigger Trigger { get; }

        /// <summary>
        /// Gets the initial text or character typed by the user when triggered via direct typing.
        /// </summary>
        string InitialInput { get; }
    }
}
