using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    /// <summary>
    /// Specifies the primary placement direction of a spreadsheet popup.
    /// </summary>
    public enum PopupPlacementMode
    {
        /// <summary>
        /// Places the popup below the target element/cell.
        /// </summary>
        Bottom,

        /// <summary>
        /// Places the popup above the target element/cell.
        /// </summary>
        Top
    }

    /// <summary>
    /// Specifies the horizontal alignment of the popup relative to the target element/cell.
    /// </summary>
    public enum PopupAlignment
    {
        /// <summary>
        /// Aligns the left edge of the popup content with the left edge of the target.
        /// </summary>
        Left,

        /// <summary>
        /// Aligns the right edge of the popup content with the right edge of the target.
        /// </summary>
        Right,

        /// <summary>
        /// Centers the popup content horizontally relative to the target.
        /// </summary>
        Center
    }

    /// <summary>
    /// Configuration options for positioning and displaying a spreadsheet popup.
    /// </summary>
    public class PopupPlacementOptions
    {
        /// <summary>
        /// Gets or sets the preferred vertical placement mode (Bottom or Top).
        /// </summary>
        public PopupPlacementMode Placement { get; set; } = PopupPlacementMode.Bottom;

        /// <summary>
        /// Gets or sets the horizontal alignment relative to the target.
        /// </summary>
        public PopupAlignment Alignment { get; set; } = PopupAlignment.Left;

        /// <summary>
        /// Gets or sets the margins reserved for drop shadow effects around the content.
        /// Default is (8, 6, 8, 14).
        /// </summary>
        public Thickness ShadowMargin { get; set; } = new Thickness(8, 6, 8, 14);

        /// <summary>
        /// Gets or sets whether the popup should automatically flip to the opposite vertical position if it would overflow the visible viewport.
        /// </summary>
        public bool AutoFlip { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the popup closes automatically when the viewport is scrolled.
        /// </summary>
        public bool CloseOnScroll { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the popup closes automatically when switching active worksheets.
        /// </summary>
        public bool CloseOnSheetChange { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the popup stays open when clicking outside. Default is false (outside click closes).
        /// </summary>
        public bool StaysOpen { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the content should be automatically wrapped in the standard <see cref="DevBrewLabs.WPF.Spreadsheet.Components.PopupContainer"/> card.
        /// Set to false if the hosted content already provides its own drop shadow and container borders.
        /// </summary>
        public bool UseStandardContainer { get; set; } = true;

        /// <summary>
        /// Gets or sets an optional specific UIElement that should receive keyboard focus when the popup closes.
        /// If null, focus is restored to the element that was focused before opening or the active sheet CellsSurface.
        /// </summary>
        public UIElement RestoreFocusTarget { get; set; }
    }
}
