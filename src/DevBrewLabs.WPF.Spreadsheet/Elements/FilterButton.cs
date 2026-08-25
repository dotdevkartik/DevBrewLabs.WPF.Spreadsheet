using DevBrewLabs.WPF.Spreadsheet.Rendering;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Elements
{
    /// <summary>
    /// Interactive filter button displayed in column header cells when AutoFilter is enabled.
    /// </summary>
    public class FilterButton : CellElement
    {
        public static FilterButton Instance { get; } = new FilterButton();

        private static readonly Geometry _activeFilterGeometry = CreateActiveFilterGeometry();
        private static readonly Geometry _inactiveFilterGeometry = CreateInactiveFilterGeometry();

        private static Geometry CreateActiveFilterGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(0, 0), true, true);
                ctx.LineTo(new Point(10, 0), true, true);
                ctx.LineTo(new Point(6, 5), true, true);
                ctx.LineTo(new Point(6, 10), true, true);
                ctx.LineTo(new Point(4, 8), true, true);
                ctx.LineTo(new Point(4, 5), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateInactiveFilterGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(0, 3), true, true);
                ctx.LineTo(new Point(10, 3), true, true);
                ctx.LineTo(new Point(5, 8), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double filterButtonWidth = 16 * zoom;
            double filterButtonHeight = Math.Min(cellRect.Height, 20 * zoom);
            double y = cellRect.Y + (cellRect.Height - filterButtonHeight) / 2;
            return new Rect(cellRect.Right - filterButtonWidth, y, filterButtonWidth, filterButtonHeight);
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            var spread = context.SheetView?.Spread;
            if (spread == null) return;

            bool isHovered = (state == CellElementState.Hover || state == CellElementState.Pressed);

            if (isHovered && spread.HoverFilterButtonBackground != null)
            {
                var hoverBgRect = new Rect(bounds.X + 1, bounds.Y + 2, Math.Max(0, bounds.Width - 2), Math.Max(0, bounds.Height - 4));
                context.DrawRoundedRectangle(spread.HoverFilterButtonBackground, null, hoverBgRect, 2, 2);
            }

            double iconSize = 8 * context.Zoom;
            double x = bounds.X + (bounds.Width - iconSize) / 2;
            double y = bounds.Y + (bounds.Height - iconSize) / 2;

            bool isActive = context.SheetView.WorkSheet?.AutoFilter?.IsColumnFiltered(col) == true;

            Brush brush;
            if (isHovered)
            {
                brush = isActive ? spread.HoverActiveFilterBrush : spread.HoverInactiveFilterBrush;
            }
            else
            {
                brush = isActive ? spread.ActiveFilterBrush : spread.InactiveFilterBrush;
            }

            if (brush != null)
            {
                double scale = iconSize / 10.0;
                context.PushTransform(new MatrixTransform(scale, 0, 0, scale, x, y));
                context.DrawGeometry(brush, null, isActive ? _activeFilterGeometry : _inactiveFilterGeometry);
                context.Pop();
            }
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            var sheetView = view as SheetView;
            if (sheetView != null)
            {
                sheetView.Spread?.FilterManager?.ShowFilterDropdown(sheetView, col);
            }
        }
    }
}
