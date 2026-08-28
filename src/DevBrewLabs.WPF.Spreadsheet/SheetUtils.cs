using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet
{
    public static class SheetUtils
    {
        public static double GridLineThickness { get; } = 0.35;
        public static double SelectionBorderThickness { get; } = 1.5;
        public static double PixelPerDip { get; internal set; }
        public static string Tab { get; } = "\t";
        public static string NextLine { get; } = "\n";

        #region Cursors
        public static Cursor SheetCursor { get; }
        public static Cursor ColumnHeaderCursor { get; }
        public static Cursor RowHeaderCursor { get; }
        public static Cursor ColumnResizeCursor { get; }
        public static Cursor DragFillCursor { get; }
        public static Cursor RowResizeCursor { get; }
        #endregion

        #region CheckBox Defaults
        public static Brush CheckBoxCheckedBrush { get; }
        public static Pen CheckBoxCheckedPen { get; }
        public static Brush CheckBoxIndeterminateBrush { get; }
        public static Pen CheckBoxIndeterminatePen { get; }
        public static Brush CheckBoxUncheckedBgBrush { get; }
        public static Brush CheckBoxUncheckedBorderBrush { get; }
        public static Pen CheckBoxUncheckedBorderPen { get; }
        public static Brush CheckBoxCheckMarkBrush { get; }
        public static Brush CheckBoxHoverGlowBrush { get; }
        public static Brush CheckBoxPressedGlowBrush { get; }
        #endregion

        #region DatePicker Defaults
        public static Brush DatePickerIconBrush { get; }
        public static Pen DatePickerIconPen { get; }
        public static Brush DatePickerHoverIconBrush { get; }
        public static Pen DatePickerHoverIconPen { get; }
        #endregion

        #region Spinner Defaults
        public static Brush SpinnerArrowBrush { get; }
        public static Brush SpinnerHoverArrowBrush { get; }
        public static Brush SpinnerDisabledArrowBrush { get; }
        public static Brush SpinnerHoverBackground { get; }
        public static Brush SpinnerPressedBackground { get; }
        public static Brush SpinnerSeparatorBrush { get; }
        public static Pen SpinnerSeparatorPen { get; }
        #endregion

        #region Button Defaults
        public static Brush ButtonBackgroundBrush { get; }
        public static Brush ButtonHoverBackgroundBrush { get; }
        public static Brush ButtonPressedBackgroundBrush { get; }
        public static Brush ButtonDisabledBackgroundBrush { get; }
        public static Brush ButtonBorderBrush { get; }
        public static Pen ButtonBorderPen { get; }
        public static Brush ButtonHoverBorderBrush { get; }
        public static Pen ButtonHoverBorderPen { get; }
        public static Brush ButtonPressedBorderBrush { get; }
        public static Pen ButtonPressedBorderPen { get; }
        public static Brush ButtonDisabledBorderBrush { get; }
        public static Pen ButtonDisabledBorderPen { get; }
        public static Brush ButtonForegroundBrush { get; }
        public static Brush ButtonDisabledForegroundBrush { get; }
        #endregion

        static SheetUtils()
        {
            var assembly = Assembly.GetExecutingAssembly();
            SheetCursor = new Cursor(assembly.GetManifestResourceStream("DevBrewLabs.WPF.Spreadsheet.Resources.SheetCursor.cur"), true);
            DragFillCursor = new Cursor(assembly.GetManifestResourceStream("DevBrewLabs.WPF.Spreadsheet.Resources.DragFillCursor.cur"), true);
            ColumnHeaderCursor = new Cursor(assembly.GetManifestResourceStream("DevBrewLabs.WPF.Spreadsheet.Resources.ColumnHeaderCursor.cur"), true);
            RowHeaderCursor = new Cursor(assembly.GetManifestResourceStream("DevBrewLabs.WPF.Spreadsheet.Resources.RowHeaderCursor.cur"), true);
            ColumnResizeCursor = new Cursor(assembly.GetManifestResourceStream("DevBrewLabs.WPF.Spreadsheet.Resources.ColumnResizeCursor.cur"), true);
            RowResizeCursor = new Cursor(assembly.GetManifestResourceStream("DevBrewLabs.WPF.Spreadsheet.Resources.RowResizeCursor.cur"), true);

            // CheckBox Defaults
            CheckBoxCheckedBrush = CreateFrozenBrush("#107C41");
            CheckBoxCheckedPen = CreateFrozenPen(CheckBoxCheckedBrush, 1.0);
            CheckBoxIndeterminateBrush = CreateFrozenBrush("#64748B");
            CheckBoxIndeterminatePen = CreateFrozenPen(CheckBoxIndeterminateBrush, 1.0);
            CheckBoxUncheckedBgBrush = Brushes.White;
            CheckBoxUncheckedBorderBrush = CreateFrozenBrush("#94A3B8");
            CheckBoxUncheckedBorderPen = CreateFrozenPen(CheckBoxUncheckedBorderBrush, 1.2);
            CheckBoxCheckMarkBrush = Brushes.White;
            CheckBoxHoverGlowBrush = CreateFrozenBrush(Color.FromArgb(24, 16, 124, 65));
            CheckBoxPressedGlowBrush = CreateFrozenBrush(Color.FromArgb(50, 16, 124, 65));

            // DatePicker Defaults
            DatePickerIconBrush = CreateFrozenBrush(Color.FromRgb(100, 105, 115));
            DatePickerIconPen = CreateFrozenPen(DatePickerIconBrush, 1.0);
            DatePickerHoverIconBrush = Brushes.Black;
            DatePickerHoverIconPen = CreateFrozenPen(DatePickerHoverIconBrush, 1.0);

            // Spinner Defaults
            SpinnerArrowBrush = CreateFrozenBrush(Color.FromArgb(255, 75, 85, 99));
            SpinnerHoverArrowBrush = CreateFrozenBrush(Color.FromArgb(255, 17, 24, 39));
            SpinnerDisabledArrowBrush = CreateFrozenBrush(Color.FromArgb(255, 156, 163, 175));
            SpinnerHoverBackground = CreateFrozenBrush(Color.FromArgb(255, 229, 231, 235));
            SpinnerPressedBackground = CreateFrozenBrush(Color.FromArgb(255, 209, 213, 219));
            SpinnerSeparatorBrush = CreateFrozenBrush(Color.FromArgb(255, 229, 231, 235));
            SpinnerSeparatorPen = CreateFrozenPen(SpinnerSeparatorBrush, 1.0);

            // Button Defaults
            ButtonBackgroundBrush = CreateFrozenBrush("#F3F4F6");
            ButtonHoverBackgroundBrush = CreateFrozenBrush("#E5E7EB");
            ButtonPressedBackgroundBrush = CreateFrozenBrush("#D1D5DB");
            ButtonDisabledBackgroundBrush = CreateFrozenBrush("#F9FAFB");
            ButtonBorderBrush = CreateFrozenBrush("#D1D5DB");
            ButtonBorderPen = CreateFrozenPen(ButtonBorderBrush, 1.0);
            ButtonHoverBorderBrush = CreateFrozenBrush("#9CA3AF");
            ButtonHoverBorderPen = CreateFrozenPen(ButtonHoverBorderBrush, 1.0);
            ButtonPressedBorderBrush = CreateFrozenBrush("#6B7280");
            ButtonPressedBorderPen = CreateFrozenPen(ButtonPressedBorderBrush, 1.0);
            ButtonDisabledBorderBrush = CreateFrozenBrush("#E5E7EB");
            ButtonDisabledBorderPen = CreateFrozenPen(ButtonDisabledBorderBrush, 1.0);
            ButtonForegroundBrush = CreateFrozenBrush("#111827");
            ButtonDisabledForegroundBrush = CreateFrozenBrush("#9CA3AF");
        }

        #region Frozen Resource Helpers

        public static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }

        public static SolidColorBrush CreateFrozenBrush(string hexColor)
        {
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            return CreateFrozenBrush(color);
        }

        public static Pen CreateFrozenPen(Brush brush, double thickness)
        {
            if (brush == null) return null;
            var pen = new Pen(brush, thickness);
            if (pen.CanFreeze) pen.Freeze();
            return pen;
        }

        public static Pen CreateFrozenPen(Color color, double thickness)
        {
            var brush = CreateFrozenBrush(color);
            return CreateFrozenPen(brush, thickness);
        }

        #endregion
    }
}
