using System.Windows;
using System.Windows.Controls;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    /// <summary>
    /// A standardized popup card container for spreadsheet dropdowns, popups, and pickers.
    /// Provides consistent borders, rounded corners, drop shadows, and background styling via Themes.
    /// </summary>
    public class PopupContainer : ContentControl
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(PopupContainer),
                new FrameworkPropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static PopupContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PopupContainer), 
                new FrameworkPropertyMetadata(typeof(PopupContainer)));
        }

        public PopupContainer()
        {
            Focusable = false;
        }
    }
}
