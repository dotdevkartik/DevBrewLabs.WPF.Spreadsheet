using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    /// <summary>
    /// Specifies the display view modes for the <see cref="SpreadCalendar"/>.
    /// </summary>
    public enum SpreadCalendarViewMode
    {
        Month,
        Year,
        Decade
    }

    /// <summary>
    /// Interactive button representing a single day in the <see cref="SpreadCalendar"/>.
    /// </summary>
    public class SpreadCalendarDayButton : Button
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SpreadCalendarDayButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsTodayProperty =
            DependencyProperty.Register(nameof(IsToday), typeof(bool), typeof(SpreadCalendarDayButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsInactiveProperty =
            DependencyProperty.Register(nameof(IsInactive), typeof(bool), typeof(SpreadCalendarDayButton), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsToday
        {
            get => (bool)GetValue(IsTodayProperty);
            set => SetValue(IsTodayProperty, value);
        }

        public bool IsInactive
        {
            get => (bool)GetValue(IsInactiveProperty);
            set => SetValue(IsInactiveProperty, value);
        }

        static SpreadCalendarDayButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpreadCalendarDayButton), new FrameworkPropertyMetadata(typeof(SpreadCalendarDayButton)));
            FocusableProperty.OverrideMetadata(typeof(SpreadCalendarDayButton), new FrameworkPropertyMetadata(false));
        }
    }

    /// <summary>
    /// Interactive button representing a month or year in the <see cref="SpreadCalendar"/>.
    /// </summary>
    public class SpreadCalendarMonthButton : Button
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SpreadCalendarMonthButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsInactiveProperty =
            DependencyProperty.Register(nameof(IsInactive), typeof(bool), typeof(SpreadCalendarMonthButton), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsInactive
        {
            get => (bool)GetValue(IsInactiveProperty);
            set => SetValue(IsInactiveProperty, value);
        }

        static SpreadCalendarMonthButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpreadCalendarMonthButton), new FrameworkPropertyMetadata(typeof(SpreadCalendarMonthButton)));
            FocusableProperty.OverrideMetadata(typeof(SpreadCalendarMonthButton), new FrameworkPropertyMetadata(false));
        }
    }

    /// <summary>
    /// A modern, lightweight calendar control designed for spreadsheet date editing.
    /// </summary>
    [TemplatePart(Name = "PART_PreviousButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_HeaderButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NextButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MonthView", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_YearView", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_DecadeView", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_TodayButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ClearButton", Type = typeof(Button))]
    public class SpreadCalendar : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(
                nameof(SelectedDate),
                typeof(DateTime?),
                typeof(SpreadCalendar),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDatePropertyChanged));

        public static readonly DependencyProperty DisplayDateProperty =
            DependencyProperty.Register(
                nameof(DisplayDate),
                typeof(DateTime),
                typeof(SpreadCalendar),
                new FrameworkPropertyMetadata(DateTime.Today, OnDisplayDatePropertyChanged));

        public static readonly DependencyProperty ViewModeProperty =
            DependencyProperty.Register(
                nameof(ViewMode),
                typeof(SpreadCalendarViewMode),
                typeof(SpreadCalendar),
                new FrameworkPropertyMetadata(SpreadCalendarViewMode.Month, OnViewModePropertyChanged));

        public static readonly DependencyProperty FirstDayOfWeekProperty =
            DependencyProperty.Register(
                nameof(FirstDayOfWeek),
                typeof(DayOfWeek),
                typeof(SpreadCalendar),
                new FrameworkPropertyMetadata(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, OnFirstDayOfWeekPropertyChanged));

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public DateTime DisplayDate
        {
            get => (DateTime)GetValue(DisplayDateProperty);
            set => SetValue(DisplayDateProperty, value);
        }

        public SpreadCalendarViewMode ViewMode
        {
            get => (SpreadCalendarViewMode)GetValue(ViewModeProperty);
            set => SetValue(ViewModeProperty, value);
        }

        public DayOfWeek FirstDayOfWeek
        {
            get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
            set => SetValue(FirstDayOfWeekProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler<DateTime?> SelectedDateChanged;
        public event EventHandler DateCommitted;

        #endregion

        #region Template Parts

        private Button _previousButton;
        private Button _headerButton;
        private Button _nextButton;
        private Grid _monthView;
        private Grid _yearView;
        private Grid _decadeView;
        private Button _todayButton;
        private Button _clearButton;

        #endregion

        static SpreadCalendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpreadCalendar), new FrameworkPropertyMetadata(typeof(SpreadCalendar)));
            FocusableProperty.OverrideMetadata(typeof(SpreadCalendar), new FrameworkPropertyMetadata(false));
        }

        public SpreadCalendar()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_previousButton != null) _previousButton.Click -= OnPreviousButtonClick;
            if (_headerButton != null) _headerButton.Click -= OnHeaderButtonClick;
            if (_nextButton != null) _nextButton.Click -= OnNextButtonClick;
            if (_todayButton != null) _todayButton.Click -= OnTodayButtonClick;
            if (_clearButton != null) _clearButton.Click -= OnClearButtonClick;

            _previousButton = GetTemplateChild("PART_PreviousButton") as Button;
            _headerButton = GetTemplateChild("PART_HeaderButton") as Button;
            _nextButton = GetTemplateChild("PART_NextButton") as Button;
            _monthView = GetTemplateChild("PART_MonthView") as Grid;
            _yearView = GetTemplateChild("PART_YearView") as Grid;
            _decadeView = GetTemplateChild("PART_DecadeView") as Grid;
            _todayButton = GetTemplateChild("PART_TodayButton") as Button;
            _clearButton = GetTemplateChild("PART_ClearButton") as Button;

            if (_previousButton != null) _previousButton.Click += OnPreviousButtonClick;
            if (_headerButton != null) _headerButton.Click += OnHeaderButtonClick;
            if (_nextButton != null) _nextButton.Click += OnNextButtonClick;
            if (_todayButton != null) _todayButton.Click += OnTodayButtonClick;
            if (_clearButton != null) _clearButton.Click += OnClearButtonClick;

            UpdateUI();
        }

        private static void OnSelectedDatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = (SpreadCalendar)d;
            var newDate = (DateTime?)e.NewValue;
            if (newDate.HasValue)
            {
                calendar.DisplayDate = newDate.Value;
            }
            calendar.UpdateUI();
            calendar.SelectedDateChanged?.Invoke(calendar, newDate);
        }

        private static void OnDisplayDatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = (SpreadCalendar)d;
            calendar.UpdateUI();
        }

        private static void OnViewModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = (SpreadCalendar)d;
            calendar.UpdateUI();
        }

        private static void OnFirstDayOfWeekPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = (SpreadCalendar)d;
            calendar.UpdateUI();
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            switch (ViewMode)
            {
                case SpreadCalendarViewMode.Month:
                    DisplayDate = DisplayDate.AddMonths(-1);
                    break;
                case SpreadCalendarViewMode.Year:
                    DisplayDate = DisplayDate.AddYears(-1);
                    break;
                case SpreadCalendarViewMode.Decade:
                    DisplayDate = DisplayDate.AddYears(-10);
                    break;
            }
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            switch (ViewMode)
            {
                case SpreadCalendarViewMode.Month:
                    DisplayDate = DisplayDate.AddMonths(1);
                    break;
                case SpreadCalendarViewMode.Year:
                    DisplayDate = DisplayDate.AddYears(1);
                    break;
                case SpreadCalendarViewMode.Decade:
                    DisplayDate = DisplayDate.AddYears(10);
                    break;
            }
        }

        private void OnHeaderButtonClick(object sender, RoutedEventArgs e)
        {
            switch (ViewMode)
            {
                case SpreadCalendarViewMode.Month:
                    ViewMode = SpreadCalendarViewMode.Year;
                    break;
                case SpreadCalendarViewMode.Year:
                    ViewMode = SpreadCalendarViewMode.Decade;
                    break;
                case SpreadCalendarViewMode.Decade:
                    ViewMode = SpreadCalendarViewMode.Month;
                    break;
            }
        }

        private void OnTodayButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedDate = DateTime.Today;
            DisplayDate = DateTime.Today;
            ViewMode = SpreadCalendarViewMode.Month;
            DateCommitted?.Invoke(this, EventArgs.Empty);
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedDate = null;
            DateCommitted?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateUI()
        {
            UpdateHeader();
            UpdateViewVisibility();

            switch (ViewMode)
            {
                case SpreadCalendarViewMode.Month:
                    PopulateMonthView();
                    break;
                case SpreadCalendarViewMode.Year:
                    PopulateYearView();
                    break;
                case SpreadCalendarViewMode.Decade:
                    PopulateDecadeView();
                    break;
            }
        }

        private void UpdateHeader()
        {
            if (_headerButton == null) return;

            switch (ViewMode)
            {
                case SpreadCalendarViewMode.Month:
                    _headerButton.Content = DisplayDate.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
                    break;
                case SpreadCalendarViewMode.Year:
                    _headerButton.Content = DisplayDate.ToString("yyyy", CultureInfo.CurrentCulture);
                    break;
                case SpreadCalendarViewMode.Decade:
                    int startYear = (DisplayDate.Year / 10) * 10;
                    int endYear = startYear + 9;
                    _headerButton.Content = $"{startYear} - {endYear}";
                    break;
            }
        }

        private void UpdateViewVisibility()
        {
            if (_monthView != null)
                _monthView.Visibility = (ViewMode == SpreadCalendarViewMode.Month) ? Visibility.Visible : Visibility.Collapsed;
            if (_yearView != null)
                _yearView.Visibility = (ViewMode == SpreadCalendarViewMode.Year) ? Visibility.Visible : Visibility.Collapsed;
            if (_decadeView != null)
                _decadeView.Visibility = (ViewMode == SpreadCalendarViewMode.Decade) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PopulateMonthView()
        {
            if (_monthView == null) return;

            _monthView.Children.Clear();
            _monthView.RowDefinitions.Clear();
            _monthView.ColumnDefinitions.Clear();

            // 7 Columns (Days of week)
            for (int i = 0; i < 7; i++)
            {
                _monthView.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 1 Row for Day Header + 6 Rows for Weeks
            _monthView.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < 6; i++)
            {
                _monthView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
            var dayNames = dtfi.AbbreviatedDayNames;
            int firstDay = (int)FirstDayOfWeek;

            // Row 0: Day of week abbreviations
            for (int i = 0; i < 7; i++)
            {
                int dayIndex = (firstDay + i) % 7;
                string dayName = dayNames[dayIndex];
                if (dayName.Length > 2)
                    dayName = dayName.Substring(0, 2);

                var headerText = new TextBlock
                {
                    Text = dayName,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 6)
                };

                Grid.SetRow(headerText, 0);
                Grid.SetColumn(headerText, i);
                _monthView.Children.Add(headerText);
            }

            // Calculate starting date for the 7x6 grid
            DateTime firstOfMonth = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
            int offset = ((int)firstOfMonth.DayOfWeek - firstDay + 7) % 7;
            DateTime currentDate = firstOfMonth.AddDays(-offset);

            DateTime today = DateTime.Today;
            DateTime? selected = SelectedDate?.Date;

            // Rows 1-6: 42 Day Buttons
            for (int week = 0; week < 6; week++)
            {
                for (int day = 0; day < 7; day++)
                {
                    DateTime cellDate = currentDate;
                    bool isCurrentMonth = cellDate.Month == DisplayDate.Month;
                    bool isSelected = selected.HasValue && cellDate.Date == selected.Value;
                    bool isToday = cellDate.Date == today;

                    var btn = new SpreadCalendarDayButton
                    {
                        Content = cellDate.Day.ToString(CultureInfo.CurrentCulture),
                        Tag = cellDate,
                        IsSelected = isSelected,
                        IsToday = isToday,
                        IsInactive = !isCurrentMonth
                    };

                    btn.Click += (s, e) =>
                    {
                        var clickedDate = (DateTime)((Button)s).Tag;
                        SelectedDate = clickedDate;
                        DisplayDate = clickedDate;
                        DateCommitted?.Invoke(this, EventArgs.Empty);
                    };

                    Grid.SetRow(btn, week + 1);
                    Grid.SetColumn(btn, day);
                    _monthView.Children.Add(btn);

                    currentDate = currentDate.AddDays(1);
                }
            }
        }

        private void PopulateYearView()
        {
            if (_yearView == null) return;

            _yearView.Children.Clear();
            _yearView.RowDefinitions.Clear();
            _yearView.ColumnDefinitions.Clear();

            // 4 Columns x 3 Rows (12 Months)
            for (int i = 0; i < 4; i++)
            {
                _yearView.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < 3; i++)
            {
                _yearView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
            var monthNames = dtfi.AbbreviatedMonthNames;

            int selectedYear = SelectedDate?.Year ?? -1;
            int selectedMonth = SelectedDate?.Month ?? -1;

            for (int i = 0; i < 12; i++)
            {
                int monthNumber = i + 1;
                string monthName = monthNames[i];
                bool isSelected = (DisplayDate.Year == selectedYear && monthNumber == selectedMonth);

                var btn = new SpreadCalendarMonthButton
                {
                    Content = monthName,
                    Tag = monthNumber,
                    IsSelected = isSelected
                };

                btn.Click += (s, e) =>
                {
                    int chosenMonth = (int)((Button)s).Tag;
                    DisplayDate = new DateTime(DisplayDate.Year, chosenMonth, 1);
                    ViewMode = SpreadCalendarViewMode.Month;
                };

                Grid.SetRow(btn, i / 4);
                Grid.SetColumn(btn, i % 4);
                _yearView.Children.Add(btn);
            }
        }

        private void PopulateDecadeView()
        {
            if (_decadeView == null) return;

            _decadeView.Children.Clear();
            _decadeView.RowDefinitions.Clear();
            _decadeView.ColumnDefinitions.Clear();

            // 4 Columns x 3 Rows (12 Years: 1 previous decade year + 10 current decade years + 1 next decade year)
            for (int i = 0; i < 4; i++)
            {
                _decadeView.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < 3; i++)
            {
                _decadeView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            int startYear = (DisplayDate.Year / 10) * 10;
            int firstGridYear = startYear - 1;
            int selectedYear = SelectedDate?.Year ?? -1;

            for (int i = 0; i < 12; i++)
            {
                int currentYear = firstGridYear + i;
                bool isCurrentDecade = (currentYear >= startYear && currentYear <= startYear + 9);
                bool isSelected = (currentYear == selectedYear);

                var btn = new SpreadCalendarMonthButton
                {
                    Content = currentYear.ToString(CultureInfo.CurrentCulture),
                    Tag = currentYear,
                    IsSelected = isSelected,
                    IsInactive = !isCurrentDecade
                };

                btn.Click += (s, e) =>
                {
                    int chosenYear = (int)((Button)s).Tag;
                    DisplayDate = new DateTime(chosenYear, DisplayDate.Month, 1);
                    ViewMode = SpreadCalendarViewMode.Year;
                };

                Grid.SetRow(btn, i / 4);
                Grid.SetColumn(btn, i % 4);
                _decadeView.Children.Add(btn);
            }
        }
    }
}
