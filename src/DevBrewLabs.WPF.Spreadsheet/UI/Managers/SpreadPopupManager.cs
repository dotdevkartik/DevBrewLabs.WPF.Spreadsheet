using DevBrewLabs.WPF.Spreadsheet.Components;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    /// <summary>
    /// Centralized popup manager that coordinates positioning, lifecycle, boundary checking, 
    /// window tracking, container styling, and focus restoration for all spreadsheet popups and dropdowns.
    /// </summary>
    internal class SpreadPopupManager : UIManager
    {
        private Popup _popup;
        private PopupContainer _container;
        private UIElement _currentContent;
        private PopupPlacementOptions _currentOptions;
        private Window _parentWindow;
        private UIElement _previousFocusedElement;
        private bool _isClosingInternally;

        /// <summary>
        /// Gets whether a spreadsheet popup is currently open.
        /// </summary>
        public bool IsPopupOpen => _popup != null && _popup.IsOpen;

        /// <summary>
        /// Gets the content currently hosted inside the active popup, if any.
        /// </summary>
        public UIElement CurrentContent => _currentContent;

        public SpreadPopupManager(Spread spread) : base(spread)
        {
        }

        private void EnsurePopup()
        {
            if (_popup == null)
            {
                _popup = new Popup
                {
                    AllowsTransparency = true,
                    PopupAnimation = PopupAnimation.Fade,
                    StaysOpen = false
                };

                _popup.Closed += OnPopupClosed;
            }
        }

        /// <summary>
        /// Displays a popup positioned relative to a specific grid cell in the given sheet view.
        /// </summary>
        public void ShowForCell(
            SheetView sheetView,
            int row,
            int column,
            UIElement content,
            PopupPlacementOptions options = null)
        {
            if (sheetView == null || content == null)
                return;

            options = options ?? new PopupPlacementOptions();

            // Calculate unzoomed and zoomed cell coordinates
            var unzoomedRect = sheetView.ViewPort.GetCellRect(row, column);
            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;

            double x = (unzoomedRect.X - sheetView.ViewPort.LeftColumnLocation) * zoom;
            double y = (unzoomedRect.Y - sheetView.ViewPort.TopRowLocation) * zoom;
            double cellWidth = unzoomedRect.Width * zoom;
            double cellHeight = unzoomedRect.Height * zoom;

            var targetRect = new Rect(x, y, cellWidth, cellHeight);
            ShowCore(sheetView.CellsSurface, targetRect, content, options, sheetView);
        }

        /// <summary>
        /// Displays a popup positioned relative to a specific WPF UIElement (such as an editor or button).
        /// </summary>
        public void ShowForElement(
            UIElement placementTarget,
            UIElement content,
            PopupPlacementOptions options = null)
        {
            if (placementTarget == null || content == null)
                return;

            options = options ?? new PopupPlacementOptions();
            double width = (placementTarget as FrameworkElement)?.ActualWidth ?? 0;
            double height = (placementTarget as FrameworkElement)?.ActualHeight ?? 0;

            var targetRect = new Rect(0, 0, width, height);
            ShowCore(placementTarget, targetRect, content, options, null);
        }

        /// <summary>
        /// Displays a popup positioned relative to a specific bounding rectangle on a placement target.
        /// </summary>
        public void ShowAtRect(
            UIElement placementTarget,
            Rect targetRect,
            UIElement content,
            PopupPlacementOptions options = null)
        {
            if (placementTarget == null || content == null)
                return;

            options = options ?? new PopupPlacementOptions();
            ShowCore(placementTarget, targetRect, content, options, null);
        }

        private void ShowCore(
            UIElement placementTarget,
            Rect targetRect,
            UIElement content,
            PopupPlacementOptions options,
            SheetView sheetView)
        {
            // Close any existing popup cleanly before showing the new one
            if (IsPopupOpen)
            {
                ClosePopup();
            }

            EnsurePopup();

            _currentContent = content;
            _currentOptions = options;
            _previousFocusedElement = options.RestoreFocusTarget ?? (Keyboard.FocusedElement as UIElement);

            // Wrap content in PopupContainer if requested
            UIElement popupChild;
            if (options.UseStandardContainer)
            {
                if (_container == null)
                {
                    _container = new PopupContainer();
                }
                _container.Content = content;
                popupChild = _container;
            }
            else
            {
                popupChild = content;
            }

            _popup.Child = popupChild;
            _popup.StaysOpen = options.StaysOpen;
            _popup.PlacementTarget = placementTarget;
            _popup.PlacementRectangle = targetRect;

            // Measure child to obtain actual dimensions for placement calculations
            popupChild.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desiredSize = popupChild.DesiredSize;
            double contentWidth = desiredSize.Width;
            double contentHeight = desiredSize.Height;

            if (contentWidth <= 0 && content is FrameworkElement feW && !double.IsNaN(feW.Width) && feW.Width > 0)
                contentWidth = feW.Width;
            if (contentHeight <= 0 && content is FrameworkElement feH && !double.IsNaN(feH.Height) && feH.Height > 0)
                contentHeight = feH.Height;

            if (contentWidth <= 0) contentWidth = 260;
            if (contentHeight <= 0) contentHeight = 300;

            var shadow = options.ShadowMargin;
            double effectiveHeight = contentHeight - (shadow.Top + shadow.Bottom);

            // Determine vertical placement & auto-flip
            double surfaceHeight = (placementTarget as FrameworkElement)?.ActualHeight ?? 0;
            if (surfaceHeight <= 0 && sheetView?.CellsSurface != null)
            {
                surfaceHeight = sheetView.CellsSurface.ActualHeight;
            }

            bool placeTop = options.Placement == PopupPlacementMode.Top;

            if (options.AutoFlip && surfaceHeight > 0)
            {
                if (options.Placement == PopupPlacementMode.Bottom)
                {
                    // Check if bottom placement overflows and top placement has space
                    if ((targetRect.Y + targetRect.Height + effectiveHeight > surfaceHeight) && 
                        (targetRect.Y - effectiveHeight >= 0))
                    {
                        placeTop = true;
                    }
                }
                else if (options.Placement == PopupPlacementMode.Top)
                {
                    // Check if top placement overflows and bottom has space
                    if ((targetRect.Y - effectiveHeight < 0) && 
                        (targetRect.Y + targetRect.Height + effectiveHeight <= surfaceHeight))
                    {
                        placeTop = false;
                    }
                }
            }

            if (placeTop)
            {
                _popup.Placement = PlacementMode.Top;
                _popup.VerticalOffset = shadow.Bottom;
            }
            else
            {
                _popup.Placement = PlacementMode.Bottom;
                _popup.VerticalOffset = -shadow.Top;
            }

            // Determine horizontal alignment
            switch (options.Alignment)
            {
                case PopupAlignment.Right:
                    double rightAlignedOffset = targetRect.Width - contentWidth + shadow.Right;
                    // Check left boundary clipping
                    if (targetRect.X + rightAlignedOffset >= -shadow.Left)
                    {
                        _popup.HorizontalOffset = rightAlignedOffset;
                    }
                    else
                    {
                        _popup.HorizontalOffset = -targetRect.X - shadow.Left;
                    }
                    break;

                case PopupAlignment.Center:
                    _popup.HorizontalOffset = (targetRect.Width - contentWidth) / 2.0;
                    break;

                case PopupAlignment.Left:
                default:
                    _popup.HorizontalOffset = -shadow.Left;
                    break;
            }

            AttachWindowEvents(placementTarget);
            _popup.IsOpen = true;
        }

        /// <summary>
        /// Closes the currently active popup if open.
        /// </summary>
        public void ClosePopup()
        {
            if (_popup != null && _popup.IsOpen)
            {
                _popup.IsOpen = false;
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (_isClosingInternally)
                return;

            _isClosingInternally = true;
            try
            {
                DetachWindowEvents();

                var focusTarget = _currentOptions?.RestoreFocusTarget ?? _previousFocusedElement;
                _currentContent = null;
                _currentOptions = null;
                _previousFocusedElement = null;

                if (_container != null)
                {
                    _container.Content = null;
                }

                // Restore keyboard focus
                if (focusTarget != null && focusTarget.Focusable && ((focusTarget as FrameworkElement)?.IsLoaded ?? true))
                {
                    focusTarget.Focus();
                }
                else if (Spread?.Sheets?.ActiveSheet != null)
                {
                    var activeSheetView = Spread.Sheets.ActiveSheet.As<SheetView>();
                    activeSheetView?.CellsSurface?.Focus();
                }
            }
            finally
            {
                _isClosingInternally = false;
            }
        }

        #region Window Event Management

        private void AttachWindowEvents(UIElement element)
        {
            DetachWindowEvents();

            _parentWindow = Window.GetWindow(element ?? Spread);
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged += OnWindowMovedOrResized;
                _parentWindow.SizeChanged += OnWindowMovedOrResized;
                _parentWindow.Deactivated += OnWindowDeactivated;
                _parentWindow.StateChanged += OnWindowMovedOrResized;
            }
        }

        private void DetachWindowEvents()
        {
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged -= OnWindowMovedOrResized;
                _parentWindow.SizeChanged -= OnWindowMovedOrResized;
                _parentWindow.Deactivated -= OnWindowDeactivated;
                _parentWindow.StateChanged -= OnWindowMovedOrResized;
                _parentWindow = null;
            }
        }

        private void OnWindowMovedOrResized(object sender, EventArgs e)
        {
            ClosePopup();
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            ClosePopup();
        }

        #endregion

        public override void Dispose()
        {
            ClosePopup();

            if (_popup != null)
            {
                _popup.Closed -= OnPopupClosed;
                _popup.Child = null;
                _popup = null;
            }

            if (_container != null)
            {
                _container.Content = null;
                _container = null;
            }

            _currentContent = null;
            _currentOptions = null;
            _previousFocusedElement = null;

            base.Dispose();
        }
    }
}
