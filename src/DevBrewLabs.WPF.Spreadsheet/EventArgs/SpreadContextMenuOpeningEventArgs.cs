using DevBrewLabs.Spreadsheet;
using System;
using System.Windows.Controls;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Identifies the spreadsheet region where a context menu was triggered.
    /// </summary>
    public enum SpreadContextMenuRegion
    {
        Cells,
        RowHeader,
        ColumnHeader,
        CornerHeader,
        SheetTab
    }

    /// <summary>
    /// Event arguments for the Spread.ContextMenuOpening event.
    /// </summary>
    public class SpreadContextMenuOpeningEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the active sheet view where the context menu was triggered.
        /// </summary>
        public ISheetView SheetView { get; }

        /// <summary>
        /// Gets the spreadsheet region where the context menu was triggered.
        /// </summary>
        public SpreadContextMenuRegion Region { get; }

        /// <summary>
        /// Gets the detailed hit test result if available.
        /// </summary>
        public SpreadHitTestResult HitTestResult { get; }

        /// <summary>
        /// Gets the current cell selection.
        /// </summary>
        public CellRange Selection { get; }

        /// <summary>
        /// Gets the target sheet index when the region is SheetTab (-1 otherwise).
        /// </summary>
        public int SheetIndex { get; }

        /// <summary>
        /// Gets or sets the context menu that will be displayed.
        /// Can be modified or replaced by event handlers.
        /// </summary>
        public ContextMenu ContextMenu { get; set; }

        /// <summary>
        /// Gets or sets whether to cancel showing the context menu.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets whether the event has been handled.
        /// </summary>
        public bool Handled { get; set; }

        public SpreadContextMenuOpeningEventArgs(
            ISheetView sheetView,
            SpreadContextMenuRegion region,
            SpreadHitTestResult hitTestResult,
            CellRange selection,
            ContextMenu contextMenu,
            int sheetIndex = -1)
        {
            SheetView = sheetView;
            Region = region;
            HitTestResult = hitTestResult;
            Selection = selection;
            ContextMenu = contextMenu;
            SheetIndex = sheetIndex;
        }
    }
}
