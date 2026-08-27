using DevBrewLabs.WPF.Spreadsheet.Rendering;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Elements
{
    /// <summary>
    /// Specifies the spin direction of a spinner button.
    /// </summary>
    public enum SpinDirection
    {
        Up,
        Down
    }

    /// <summary>
    /// Interactive spinner button element for incrementing or decrementing numeric cell values.
    /// </summary>
    public class SpinnerButton : CellElement
    {
        public static SpinnerButton Up { get; } = new SpinnerButton(SpinDirection.Up);
        public static SpinnerButton Down { get; } = new SpinnerButton(SpinDirection.Down);

        private static readonly Geometry _upArrowGeometry = CreateUpArrowGeometry();
        private static readonly Geometry _downArrowGeometry = CreateDownArrowGeometry();
        private static readonly Brush _normalArrowBrush = CreateFrozenBrush(Color.FromArgb(255, 75, 85, 99));     // #4B5563
        private static readonly Brush _hoverArrowBrush = CreateFrozenBrush(Color.FromArgb(255, 17, 24, 39));      // #111827
        private static readonly Brush _disabledArrowBrush = CreateFrozenBrush(Color.FromArgb(255, 156, 163, 175)); // #9CA3AF
        private static readonly Brush _hoverBackground = CreateFrozenBrush(Color.FromArgb(255, 229, 231, 235));   // #E5E7EB
        private static readonly Brush _pressedBackground = CreateFrozenBrush(Color.FromArgb(255, 209, 213, 219)); // #D1D5DB
        private static readonly Pen _separatorPen = CreateFrozenPen(Color.FromArgb(255, 229, 231, 235), 1.0);

        /// <summary>
        /// Gets the spin direction (Up or Down).
        /// </summary>
        public SpinDirection Direction { get; }

        public SpinnerButton(SpinDirection direction)
        {
            Direction = direction;
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static Pen CreateFrozenPen(Color color, double thickness)
        {
            var pen = new Pen(CreateFrozenBrush(color), thickness);
            pen.Freeze();
            return pen;
        }

        private static Geometry CreateUpArrowGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(0, 4), true, true);
                ctx.LineTo(new Point(8, 4), true, true);
                ctx.LineTo(new Point(4, 0), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateDownArrowGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(0, 0), true, true);
                ctx.LineTo(new Point(8, 0), true, true);
                ctx.LineTo(new Point(4, 4), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double spinnerWidth = 16 * zoom;
            double halfHeight = cellRect.Height / 2.0;

            if (Direction == SpinDirection.Up)
            {
                return new Rect(cellRect.Right - spinnerWidth, cellRect.Y, spinnerWidth, halfHeight);
            }
            else
            {
                return new Rect(cellRect.Right - spinnerWidth, cellRect.Y + halfHeight, spinnerWidth, cellRect.Height - halfHeight);
            }
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // 1. Draw background
            if (state == CellElementState.Pressed)
            {
                context.DrawRectangle(_pressedBackground, null, bounds);
            }
            else if (state == CellElementState.Hover)
            {
                context.DrawRectangle(_hoverBackground, null, bounds);
            }

            // 2. Draw vertical separator on left edge
            context.DrawLine(_separatorPen, new Point(bounds.Left, bounds.Top), new Point(bounds.Left, bounds.Bottom));

            // 3. Draw horizontal separator below the Up button
            if (Direction == SpinDirection.Up)
            {
                context.DrawLine(_separatorPen, new Point(bounds.Left, bounds.Bottom), new Point(bounds.Right, bounds.Bottom));
            }

            // 4. Draw arrow icon
            double arrowWidth = 8 * context.ZoomFactor;
            double arrowHeight = 4 * context.ZoomFactor;
            double x = bounds.X + (bounds.Width - arrowWidth) / 2.0;
            double y = bounds.Y + (bounds.Height - arrowHeight) / 2.0;

            Brush arrowBrush = _normalArrowBrush;
            if (state == CellElementState.Hover || state == CellElementState.Pressed)
            {
                arrowBrush = _hoverArrowBrush;
            }
            else if (state == CellElementState.Disabled)
            {
                arrowBrush = _disabledArrowBrush;
            }

            context.PushTransform(new MatrixTransform(context.ZoomFactor, 0, 0, context.ZoomFactor, x, y));
            context.DrawGeometry(arrowBrush, null, Direction == SpinDirection.Up ? _upArrowGeometry : _downArrowGeometry);
            context.Pop();
        }
    }
}
