using DevBrewLabs.WPF.Spreadsheet.Components;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Editors
{
    /// <summary>
    /// In-place date editor for spreadsheet cells supporting inline text editing and a custom lightweight calendar dropdown popup.
    /// </summary>
    public class DateCellEditor : TextCellEditor
    {
        private SpreadCalendar _calendar;
        private bool _isInitializing;
        private SheetView _sheetView;
        private int _row;
        private int _column;

        public string Format { get; set; } = "d";

        public SpreadCalendar Calendar => _calendar;

        public bool IsPopupOpen => _sheetView?.Spread?.PopupManager != null &&
                                   _sheetView.Spread.PopupManager.IsPopupOpen &&
                                   _sheetView.Spread.PopupManager.CurrentContent == _calendar;

        public DateCellEditor()
        {
            InitializeCalendar();
        }

        private void InitializeCalendar()
        {
            _calendar = new SpreadCalendar();
            _calendar.SelectedDateChanged += OnCalendarSelectedDateChanged;
            _calendar.DateCommitted += OnCalendarDateCommitted;
        }

        public void TogglePopup()
        {
            if (IsPopupOpen)
            {
                ClosePopup();
            }
            else
            {
                OpenPopup();
            }
        }

        public void OpenPopup()
        {
            if (_sheetView?.Spread?.PopupManager == null)
                return;

            _sheetView.Spread.PopupManager.ShowForCell(
                _sheetView,
                _row,
                _column,
                _calendar,
                new PopupPlacementOptions
                {
                    Alignment = PopupAlignment.Right,
                    AutoFlip = true,
                    UseStandardContainer = true,
                    RestoreFocusTarget = this
                });
        }

        public void ClosePopup()
        {
            if (IsPopupOpen)
            {
                _sheetView?.Spread?.PopupManager?.ClosePopup();
            }
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
            ClosePopup();
            Focus();
            CaretIndex = Text?.Length ?? 0;
        }

        public override void StartEdit(IEditorContext context)
        {
            base.StartEdit(context);

            _sheetView = context.SheetView as SheetView;
            _row = context.Row;
            _column = context.Column;

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
                OpenPopup();
            }
        }

        public override bool HandlesKeyDown(KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Alt + Down Arrow or F4 toggles calendar dropdown
            if ((key == Key.Down && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) || key == Key.F4)
            {
                TogglePopup();
                return true;
            }

            if (IsPopupOpen)
            {
                if (key == Key.Escape)
                {
                    ClosePopup();
                    return true;
                }

                if (key == Key.Enter)
                {
                    if (_calendar.SelectedDate.HasValue)
                    {
                        Text = _calendar.SelectedDate.Value.ToString(Format ?? "d");
                    }
                    ClosePopup();
                    return false; // let EditingManager commit
                }
            }

            return base.HandlesKeyDown(e);
        }

        public override void EndEdit()
        {
            ClosePopup();
            _sheetView = null;
            base.EndEdit();
        }
    }
}
