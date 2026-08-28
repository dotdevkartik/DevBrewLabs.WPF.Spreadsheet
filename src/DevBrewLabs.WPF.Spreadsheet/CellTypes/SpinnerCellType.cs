using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Abstract base class for cell types that support interactive spinner controls (Up and Down buttons).
    /// </summary>
    public abstract class SpinnerCellType : BaseCellType
    {
        private Brush _separatorBrush;
        private Pen _separatorPen;
        private SpinnerButton _upButton;
        private SpinnerButton _downButton;
        private CellElement[] _spinnerElements;

        /// <summary>
        /// Gets or sets whether interactive spin up and spin down buttons are displayed on the cell.
        /// </summary>
        public bool ShowSpinners { get; set; } = false;

        /// <summary>
        /// Gets the width of the spinner buttons in device-independent units (before zoom scaling).
        /// </summary>
        public virtual double SpinnerWidth => 16.0;

        /// <summary>
        /// Gets or sets the brush used for drawing the spinner arrows in normal state.
        /// </summary>
        public Brush ArrowBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush used for drawing the spinner arrows in hover or pressed state.
        /// </summary>
        public Brush HoverArrowBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush used for drawing the spinner arrows in disabled state.
        /// </summary>
        public Brush DisabledArrowBrush { get; set; }

        /// <summary>
        /// Gets or sets the background brush when hovered.
        /// </summary>
        public Brush HoverBackground { get; set; }

        /// <summary>
        /// Gets or sets the background brush when pressed.
        /// </summary>
        public Brush PressedBackground { get; set; }

        /// <summary>
        /// Gets or sets the separator line brush between buttons and cell content.
        /// </summary>
        public Brush SeparatorBrush
        {
            get => _separatorBrush;
            set
            {
                _separatorBrush = value;
                _separatorPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen SeparatorPen => _separatorPen ?? SheetUtils.SpinnerSeparatorPen;

        /// <inheritdoc/>
        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (ShowSpinners)
            {
                return _spinnerElements ?? (_spinnerElements = new CellElement[]
                {
                    _upButton ?? (_upButton = new SpinnerButton(this, SpinDirection.Up)),
                    _downButton ?? (_downButton = new SpinnerButton(this, SpinDirection.Down))
                });
            }

            return base.GetElements(view, row, col);
        }

        /// <inheritdoc/>
        public override Rect GetContentRect(ISheetView view, int row, int col, Rect cellRect, double zoom)
        {
            var rect = base.GetContentRect(view, row, col, cellRect, zoom);
            if (ShowSpinners)
            {
                double width = SpinnerWidth * zoom;
                return new Rect(rect.X, rect.Y, System.Math.Max(0, rect.Width - width), rect.Height);
            }

            return rect;
        }

        /// <inheritdoc/>
        public override void OnElementClick(ISheetView view, int row, int col, CellElement element)
        {
            base.OnElementClick(view, row, col, element);

            if (element is SpinnerButton spinner)
            {
                if (view?.Spread?.EditingManager?.IsEditing == true)
                {
                    view.Spread.EditingManager.EndEdit(true);
                }

                OnSpin(view, row, col, spinner.Direction);
            }
        }

        /// <summary>
        /// Handles the spin action when a spinner button is clicked.
        /// </summary>
        /// <param name="view">The active sheet view.</param>
        /// <param name="row">The cell row index.</param>
        /// <param name="col">The cell column index.</param>
        /// <param name="direction">The spin direction (Up or Down).</param>
        public abstract void OnSpin(ISheetView view, int row, int col, SpinDirection direction);
    }

    #region Elements

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
        private static readonly Geometry _upArrowGeometry = CreateUpArrowGeometry();
        private static readonly Geometry _downArrowGeometry = CreateDownArrowGeometry();
        private readonly SpinnerCellType _cellType;

        /// <summary>
        /// Gets the spin direction (Up or Down).
        /// </summary>
        public SpinDirection Direction { get; }

        public SpinnerButton(SpinnerCellType cellType, SpinDirection direction)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
            Direction = direction;
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
                var bg = _cellType?.PressedBackground ?? SheetUtils.SpinnerPressedBackground;
                if (bg != null)
                {
                    context.DrawRectangle(bg, null, bounds);
                }
            }
            else if (state == CellElementState.Hover)
            {
                var bg = _cellType?.HoverBackground ?? SheetUtils.SpinnerHoverBackground;
                if (bg != null)
                {
                    context.DrawRectangle(bg, null, bounds);
                }
            }

            // 2. Draw vertical separator on left edge
            var separatorPen = _cellType?.SeparatorPen ?? SheetUtils.SpinnerSeparatorPen;
            if (separatorPen != null)
            {
                context.DrawLine(separatorPen, new Point(bounds.Left, bounds.Top), new Point(bounds.Left, bounds.Bottom));

                // 3. Draw horizontal separator below the Up button
                if (Direction == SpinDirection.Up)
                {
                    context.DrawLine(separatorPen, new Point(bounds.Left, bounds.Bottom), new Point(bounds.Right, bounds.Bottom));
                }
            }

            // 4. Draw arrow icon
            double arrowWidth = 8 * context.ZoomFactor;
            double arrowHeight = 4 * context.ZoomFactor;
            double x = bounds.X + (bounds.Width - arrowWidth) / 2.0;
            double y = bounds.Y + (bounds.Height - arrowHeight) / 2.0;

            Brush arrowBrush = _cellType?.ArrowBrush ?? SheetUtils.SpinnerArrowBrush;
            if (state == CellElementState.Hover || state == CellElementState.Pressed)
            {
                arrowBrush = _cellType?.HoverArrowBrush ?? SheetUtils.SpinnerHoverArrowBrush;
            }
            else if (state == CellElementState.Disabled)
            {
                arrowBrush = _cellType?.DisabledArrowBrush ?? SheetUtils.SpinnerDisabledArrowBrush;
            }

            if (arrowBrush != null)
            {
                context.PushTransform(new MatrixTransform(context.ZoomFactor, 0, 0, context.ZoomFactor, x, y));
                context.DrawGeometry(arrowBrush, null, Direction == SpinDirection.Up ? _upArrowGeometry : _downArrowGeometry);
                context.Pop();
            }
        }
    }

    #endregion
}
