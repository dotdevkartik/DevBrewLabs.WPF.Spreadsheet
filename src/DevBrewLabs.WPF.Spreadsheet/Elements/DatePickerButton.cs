using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Elements
{
    /// <summary>
    /// Interactive calendar dropdown button displayed on Date cells.
    /// </summary>
    public class DatePickerButton : CellElement
    {
        public static DatePickerButton Instance { get; } = new DatePickerButton();

        private static readonly Geometry _calendarGeometry = CreateCalendarGeometry();

        private static Geometry CreateCalendarGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                // Outer calendar frame: 10x10
                ctx.BeginFigure(new Point(1, 2), true, true);
                ctx.LineTo(new Point(9, 2), true, true);
                ctx.LineTo(new Point(9, 9), true, true);
                ctx.LineTo(new Point(1, 9), true, true);

                // Top binding rings/hooks
                ctx.BeginFigure(new Point(3, 0.5), false, false);
                ctx.LineTo(new Point(3, 2.5), true, false);

                ctx.BeginFigure(new Point(7, 0.5), false, false);
                ctx.LineTo(new Point(7, 2.5), true, false);

                // Horizontal dividing bar
                ctx.BeginFigure(new Point(1, 4.5), false, false);
                ctx.LineTo(new Point(9, 4.5), true, false);

                // Date dots/grid marks
                ctx.BeginFigure(new Point(3, 6.5), false, false);
                ctx.LineTo(new Point(4, 6.5), true, false);

                ctx.BeginFigure(new Point(6, 6.5), false, false);
                ctx.LineTo(new Point(7, 6.5), true, false);
            }
            geometry.Freeze();
            return geometry;
        }

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double buttonWidth = 18 * zoom;
            double buttonHeight = Math.Min(cellRect.Height, 20 * zoom);
            double y = cellRect.Y + (cellRect.Height - buttonHeight) / 2;
            return new Rect(cellRect.Right - buttonWidth, y, buttonWidth, buttonHeight);
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            var spread = context.SheetView?.Spread;
            if (spread == null) return;

            bool isHovered = (state == CellElementState.Hover || state == CellElementState.Pressed);

            if (isHovered && spread.HoverFilterButtonBackground != null)
            {
                var hoverBgRect = new Rect(bounds.X + 1, bounds.Y + 1, Math.Max(0, bounds.Width - 2), Math.Max(0, bounds.Height - 2));
                context.DrawRoundedRectangle(spread.HoverFilterButtonBackground, null, hoverBgRect, 2, 2);
            }

            double iconSize = 10 * context.ZoomFactor;
            double x = bounds.X + (bounds.Width - iconSize) / 2;
            double y = bounds.Y + (bounds.Height - iconSize) / 2;

            Brush strokeBrush = isHovered ? Brushes.Black : new SolidColorBrush(Color.FromRgb(100, 105, 115));
            var pen = new Pen(strokeBrush, 1.0);
            pen.Freeze();

            double scale = iconSize / 10.0;
            context.PushTransform(new MatrixTransform(scale, 0, 0, scale, x, y));
            context.DrawGeometry(null, pen, _calendarGeometry);
            context.Pop();
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            var sheetView = view as SheetView;
            if (sheetView == null) return;

            var editingManager = sheetView.Spread?.EditingManager;
            if (editingManager == null) return;

            if (editingManager.IsEditing)
            {
                if (!editingManager.EndEdit(true))
                    return;
            }

            editingManager.BeginEdit(sheetView, row, col, EditTrigger.DropdownClick);
        }
    }
}
