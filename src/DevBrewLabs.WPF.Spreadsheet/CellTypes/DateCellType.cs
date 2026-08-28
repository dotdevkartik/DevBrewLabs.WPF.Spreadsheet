using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Cell type for date values supporting formatted display, inline text editing, and calendar dropdown selection.
    /// </summary>
    public class DateCellType : BaseCellType
    {
        private Brush _iconBrush;
        private Pen _iconPen;
        private Brush _hoverIconBrush;
        private Pen _hoverIconPen;
        private DatePickerButton _dropDownButton;

        public string Format { get; set; } = "d";

        /// <summary>
        /// Gets or sets whether the calendar dropdown button is displayed on the cell.
        /// </summary>
        public bool ShowDropDownButton { get; set; } = true;

        /// <summary>
        /// Gets the width of the dropdown button in device-independent units (before zoom scaling).
        /// </summary>
        public virtual double DropDownButtonWidth => 18.0;

        /// <summary>
        /// Gets or sets the hover background brush for the date picker button. If null, the spread's default hover background is used.
        /// </summary>
        public Brush ButtonHoverBackground { get; set; }

        /// <summary>
        /// Gets or sets the icon stroke brush in normal state.
        /// </summary>
        public Brush IconBrush
        {
            get => _iconBrush;
            set
            {
                _iconBrush = value;
                _iconPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen IconPen => _iconPen ?? SheetUtils.DatePickerIconPen;

        /// <summary>
        /// Gets or sets the icon stroke brush when hovered or pressed.
        /// </summary>
        public Brush HoverIconBrush
        {
            get => _hoverIconBrush;
            set
            {
                _hoverIconBrush = value;
                _hoverIconPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen HoverIconPen => _hoverIconPen ?? SheetUtils.DatePickerHoverIconPen;

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (ShowDropDownButton)
            {
                yield return _dropDownButton ?? (_dropDownButton = new DatePickerButton(this));
            }
        }

        public override Rect GetContentRect(ISheetView view, int row, int col, Rect cellRect, double zoom)
        {
            var rect = base.GetContentRect(view, row, col, cellRect, zoom);
            if (ShowDropDownButton)
            {
                double width = DropDownButtonWidth * zoom;
                return new Rect(rect.X, rect.Y, Math.Max(0, rect.Width - width), rect.Height);
            }

            return rect;
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (value == null)
                return;

            var contentRect = GetContentRect(renderContext.SheetView, -1, -1, cellRect, renderContext.ZoomFactor);

            var align = style.HorizontalAlignment;
            if (align == CellHorizontalAlignment.Auto)
                align = CellHorizontalAlignment.Right;

            DateTime? date = null;
            if (value is DateTime dt)
                date = dt;
            else if (value is string s && DateTime.TryParse(s, out var parsed))
                date = parsed;
            else if (value is double d)
                date = DateTime.FromOADate(d);

            string textToDraw;
            if (date.HasValue)
            {
                textToDraw = date.Value.ToString(Format);
            }
            else
            {
                textToDraw = value.ToString();
            }

            renderContext.DrawText(
                textToDraw,
                contentRect,
                style.FontFamily,
                style.FontSize,
                style.FontWeight,
                style.FontStyle,
                style.ForeColor,
                align,
                style.VerticalAlignment,
                style.TextTrimming,
                style.AllowMultiLineText);
        }

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new DateCellEditor { Format = Format };
        }
    }

    #region Elements

    /// <summary>
    /// Interactive calendar dropdown button displayed on Date cells.
    /// </summary>
    public class DatePickerButton : CellElement
    {
        private static readonly Geometry _calendarGeometry = CreateCalendarGeometry();
        private readonly DateCellType _cellType;

        public DatePickerButton(DateCellType cellType)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
        }

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
            var hoverBg = _cellType?.ButtonHoverBackground ?? spread.HoverFilterButtonBackground;

            if (isHovered && hoverBg != null)
            {
                var hoverBgRect = new Rect(bounds.X + 1, bounds.Y + 1, Math.Max(0, bounds.Width - 2), Math.Max(0, bounds.Height - 2));
                context.DrawRoundedRectangle(hoverBg, null, hoverBgRect, 2, 2);
            }

            double iconSize = 10 * context.ZoomFactor;
            double x = bounds.X + (bounds.Width - iconSize) / 2;
            double y = bounds.Y + (bounds.Height - iconSize) / 2;

            var pen = isHovered ? (_cellType?.HoverIconPen ?? SheetUtils.DatePickerHoverIconPen) : (_cellType?.IconPen ?? SheetUtils.DatePickerIconPen);

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

    #endregion
}
