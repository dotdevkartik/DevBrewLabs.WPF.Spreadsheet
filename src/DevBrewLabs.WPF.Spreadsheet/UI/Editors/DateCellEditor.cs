using DevBrewLabs.WPF.Spreadsheet.Enums;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// In-place date editor for spreadsheet cells supporting inline text editing and a calendar dropdown popup.
    /// </summary>
    public class DateCellEditor : TextCellEditor
    {
        private Popup _popup;
        private Calendar _calendar;
        private bool _isInitializing;

        public string Format { get; set; } = "d";

        public DateCellEditor()
        {
            InitializePopup();
        }

        private void InitializePopup()
        {
            _calendar = new Calendar
            {
                IsTodayHighlighted = true
            };
            _calendar.SelectedDatesChanged += OnCalendarSelectedDatesChanged;

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.2
                },
                Child = _calendar
            };

            _popup = new Popup
            {
                PlacementTarget = this,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade,
                Child = border
            };
        }

        public void TogglePopup()
        {
            _popup.IsOpen = !_popup.IsOpen;
        }

        private void OnCalendarSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            if (_calendar.SelectedDate.HasValue)
            {
                Text = _calendar.SelectedDate.Value.ToString(Format ?? "d");
                _popup.IsOpen = false;
                Focus();
                CaretIndex = Text.Length;
            }
        }

        public override void StartEdit(IEditorContext context)
        {
            base.StartEdit(context);

            // Sync calendar with initial value without triggering text overwrite
            _isInitializing = true;
            try
            {
                DateTime dateValue;
                if (context.Value is DateTime dt)
                {
                    _calendar.SelectedDate = dt;
                    _calendar.DisplayDate = dt;
                }
                else if (DateTime.TryParse(Text, out dateValue))
                {
                    _calendar.SelectedDate = dateValue;
                    _calendar.DisplayDate = dateValue;
                }
                else
                {
                    _calendar.SelectedDate = null;
                    _calendar.DisplayDate = DateTime.Today;
                }
            }
            finally
            {
                _isInitializing = false;
            }

            if (context.Trigger == EditTrigger.DropdownClick)
            {
                _popup.IsOpen = true;
            }
        }

        public override void UpdateLayout(Rect contentRect, double zoomFactor)
        {
            base.UpdateLayout(contentRect, zoomFactor);
            _popup.PlacementTarget = this;
        }

        public override bool HandlesKeyDown(KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Alt + Down Arrow or F4 toggles calendar dropdown
            if ((key == Key.Down && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) || key == Key.F4)
            {
                _popup.IsOpen = !_popup.IsOpen;
                return true;
            }

            if (_popup.IsOpen)
            {
                if (key == Key.Escape)
                {
                    _popup.IsOpen = false;
                    return true;
                }

                if (key == Key.Enter)
                {
                    if (_calendar.SelectedDate.HasValue)
                    {
                        Text = _calendar.SelectedDate.Value.ToString(Format ?? "d");
                    }
                    _popup.IsOpen = false;
                    return false; // let EditingManager commit
                }
            }

            return base.HandlesKeyDown(e);
        }

        public override void EndEdit()
        {
            _popup.IsOpen = false;
            base.EndEdit();
        }
    }
}
