using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Interactive filter button displayed in column header cells when AutoFilter is enabled.
    /// </summary>
    public class FilterButton : CellElement
    {
        public static FilterButton Instance { get; } = new FilterButton();

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double filterButtonWidth = 16 * zoom;
            return new Rect(cellRect.Right - filterButtonWidth, cellRect.Y, filterButtonWidth, cellRect.Height);
        }

        public override void Draw(DrawingContext dc, Rect bounds, CellElementState state, ISheetView view, int row, int col)
        {
            var spread = view?.Spread;
            if (spread == null) return;

            bool isHovered = (state == CellElementState.Hover || state == CellElementState.Pressed);

            if (isHovered && spread.HoverFilterButtonBackground != null)
            {
                var hoverBgRect = new Rect(bounds.X + 1, bounds.Y + 2, Math.Max(0, bounds.Width - 2), Math.Max(0, bounds.Height - 4));
                dc.DrawRoundedRectangle(spread.HoverFilterButtonBackground, null, hoverBgRect, 2, 2);
            }

            double zoom = view.ZoomFactor > 0 ? view.ZoomFactor : 1.0;
            double iconSize = 8 * zoom;
            double x = bounds.X + (bounds.Width - iconSize) / 2;
            double y = bounds.Y + (bounds.Height - iconSize) / 2;

            bool isActive = view.WorkSheet?.AutoFilter?.IsColumnFiltered(col) == true;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                if (isActive)
                {
                    ctx.BeginFigure(new Point(x, y), true, true);
                    ctx.LineTo(new Point(x + iconSize, y), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.6, y + iconSize * 0.5), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.6, y + iconSize), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.4, y + iconSize * 0.8), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.4, y + iconSize * 0.5), true, true);
                }
                else
                {
                    ctx.BeginFigure(new Point(x, y + iconSize * 0.3), true, true);
                    ctx.LineTo(new Point(x + iconSize, y + iconSize * 0.3), true, true);
                    ctx.LineTo(new Point(x + iconSize * 0.5, y + iconSize * 0.8), true, true);
                }
            }
            geometry.Freeze();

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
                dc.DrawGeometry(brush, null, geometry);
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
