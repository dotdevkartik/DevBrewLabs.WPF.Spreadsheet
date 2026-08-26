using DevBrewLabs.WPF.Spreadsheet.Components;
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
    /// In-place date editor for spreadsheet cells supporting inline text editing and a custom lightweight calendar dropdown popup.
    /// </summary>
    public class DateCellEditor : TextCellEditor
    {
        private Popup _popup;
        private SpreadCalendar _calendar;
        private bool _isInitializing;
        private double _extraRightWidth = 18.0;

        public string Format { get; set; } = "d";

        public SpreadCalendar Calendar => _calendar;

        public DateCellEditor()
        {
            InitializePopup();
        }

        private void InitializePopup()
        {
            _calendar = new SpreadCalendar();
            _calendar.SelectedDateChanged += OnCalendarSelectedDateChanged;
            _calendar.DateCommitted += OnCalendarDateCommitted;

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 4, 0, 0),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 16,
                    ShadowDepth = 4,
                    Direction = 270,
                    Color = Color.FromRgb(0, 0, 0),
                    Opacity = 0.12
                },
                Child = _calendar
            };

            _popup = new Popup
            {
                PlacementTarget = this,
                Placement = PlacementMode.Custom,
                CustomPopupPlacementCallback = PlacePopup,
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade,
                Child = border
            };
        }

        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            // Align the right edge of the popup with the right edge of the cell (including the dropdown button width)
            double x = targetSize.Width + _extraRightWidth - popupSize.Width;
            double y = targetSize.Height;

            return new[]
            {
                new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Horizontal),
                new CustomPopupPlacement(new Point(0, y), PopupPrimaryAxis.Horizontal)
            };
        }

        public void TogglePopup()
        {
            _popup.IsOpen = !_popup.IsOpen;
        }

        private void OnCalendarSelectedDateChanged(object sender, DateTime? newDate)
        {
            if (_isInitializing)
                return;

            if (newDate.HasValue)
            {
                Text = newDate.Value.ToString(Format ?? "d");
            }
            else
            {
                Text = string.Empty;
            }
        }

        private void OnCalendarDateCommitted(object sender, EventArgs e)
        {
            _popup.IsOpen = false;
            Focus();
            CaretIndex = Text?.Length ?? 0;
        }

        public override void StartEdit(IEditorContext context)
        {
            base.StartEdit(context);

            double zoom = context.ZoomFactor > 0 ? context.ZoomFactor : 1.0;
            _extraRightWidth = 18.0 * zoom;

            _isInitializing = true;
            _calendar.ViewMode = SpreadCalendarViewMode.Month;
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
            double zoom = zoomFactor > 0 ? zoomFactor : 1.0;
            _extraRightWidth = 18.0 * zoom;
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
