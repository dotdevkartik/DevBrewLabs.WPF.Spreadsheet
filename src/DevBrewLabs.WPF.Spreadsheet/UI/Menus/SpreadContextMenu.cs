using DevBrewLabs.Spreadsheet;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Menus
{
    internal abstract class SpreadContextMenu : ContextMenu
    {
        protected Spread Spread { get; }

        protected static readonly SolidColorBrush CutBrush = new SolidColorBrush(Color.FromRgb(217, 119, 6));      // Amber
        protected static readonly SolidColorBrush CopyBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235));     // Azure Blue
        protected static readonly SolidColorBrush PasteBrush = new SolidColorBrush(Color.FromRgb(16, 124, 65));    // Excel Green
        protected static readonly SolidColorBrush ClearBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));    // Crimson Red
        protected static readonly SolidColorBrush MergeBrush = new SolidColorBrush(Color.FromRgb(13, 148, 136));   // Teal
        protected static readonly SolidColorBrush UnmergeBrush = new SolidColorBrush(Color.FromRgb(100, 116, 139)); // Slate
        protected static readonly SolidColorBrush SortBrush = new SolidColorBrush(Color.FromRgb(79, 70, 229));     // Indigo
        protected static readonly SolidColorBrush VisibilityBrush = new SolidColorBrush(Color.FromRgb(100, 116, 139)); // Slate
        protected static readonly SolidColorBrush AutoFitBrush = new SolidColorBrush(Color.FromRgb(2, 132, 199));   // Sky Blue
        protected static readonly SolidColorBrush AddSheetBrush = new SolidColorBrush(Color.FromRgb(16, 124, 65));  // Green
        protected static readonly SolidColorBrush DeleteSheetBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
        protected static readonly SolidColorBrush DuplicateSheetBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // Blue
        protected static readonly SolidColorBrush FilterBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // Azure Blue

        static SpreadContextMenu()
        {
            CutBrush.Freeze();
            CopyBrush.Freeze();
            PasteBrush.Freeze();
            ClearBrush.Freeze();
            MergeBrush.Freeze();
            UnmergeBrush.Freeze();
            SortBrush.Freeze();
            VisibilityBrush.Freeze();
            AutoFitBrush.Freeze();
            AddSheetBrush.Freeze();
            DeleteSheetBrush.Freeze();
            DuplicateSheetBrush.Freeze();
            FilterBrush.Freeze();
        }

        private static ResourceDictionary _themeDictionary;
        protected static ResourceDictionary ThemeDictionary
        {
            get
            {
                if (_themeDictionary == null)
                {
                    _themeDictionary = new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DevBrewLabs.WPF.Spreadsheet;component/Themes/ContextMenuStyle.xaml")
                    };
                }
                return _themeDictionary;
            }
        }

        public SpreadContextMenu(Spread spread)
        {
            Spread = spread;
            var style = ThemeDictionary["SpreadContextMenuStyle"] as Style;
            if (style != null)
            {
                Style = style;
            }
        }

        protected MenuItem CreateMenuItem(string header, string gestureText, string iconResourceKey, Brush iconBrush, ICommand command, object commandParameter)
        {
            var item = new MenuItem
            {
                Header = header,
                InputGestureText = gestureText,
                Command = command,
                CommandParameter = commandParameter
            };

            var itemStyle = ThemeDictionary["SpreadMenuItemStyle"] as Style;
            if (itemStyle != null)
            {
                item.Style = itemStyle;
            }

            if (!string.IsNullOrEmpty(iconResourceKey))
            {
                var geometry = ThemeDictionary[iconResourceKey] as Geometry;
                if (geometry != null)
                {
                    var path = new Path
                    {
                        Data = geometry,
                        Fill = iconBrush ?? new SolidColorBrush(Color.FromRgb(55, 65, 81)),
                        Width = 14,
                        Height = 14,
                        Stretch = Stretch.Uniform,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    item.Icon = path;
                }
            }

            return item;
        }

        protected Separator CreateSeparator()
        {
            var sep = new Separator();
            var sepStyle = ThemeDictionary["SpreadMenuSeparatorStyle"] as Style;
            if (sepStyle != null)
            {
                sep.Style = sepStyle;
            }
            return sep;
        }
    }
}
