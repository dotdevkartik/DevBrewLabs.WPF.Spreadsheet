using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Cell type that displays a push button control with customizable appearance, hover/pressed visual states, and click command handling.
    /// </summary>
    public class ButtonCellType : BaseCellType
    {
        private Brush _borderBrush;
        private Pen _borderPen;
        private Brush _hoverBorderBrush;
        private Pen _hoverBorderPen;
        private Brush _pressedBorderBrush;
        private Pen _pressedBorderPen;
        private Brush _disabledBorderBrush;
        private Pen _disabledBorderPen;
        private ButtonElement _buttonElement;

        /// <summary>
        /// Gets or sets the command executed on button click.
        /// </summary>
        public ICommand Command { get; set; }

        /// <summary>
        /// Gets or sets the parameter passed to <see cref="Command"/>.
        /// </summary>
        public object CommandParameter { get; set; }

        /// <summary>
        /// Gets or sets the default text displayed on the button when cell value is null or empty.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the margin around the button within the cell in device-independent units.
        /// </summary>
        public double ButtonMargin { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the corner radius of the button in device-independent units.
        /// </summary>
        public double CornerRadius { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets the horizontal alignment of the text inside the button.
        /// </summary>
        public CellHorizontalAlignment ButtonHorizontalAlignment { get; set; } = CellHorizontalAlignment.Center;

        /// <summary>
        /// Gets or sets the vertical alignment of the text inside the button.
        /// </summary>
        public CellVerticalAlignment ButtonVerticalAlignment { get; set; } = CellVerticalAlignment.Center;

        /// <summary>
        /// Gets or sets the background brush of the button in normal state.
        /// </summary>
        public Brush BackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the background brush when hovered.
        /// </summary>
        public Brush HoverBackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the background brush when pressed.
        /// </summary>
        public Brush PressedBackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the background brush when disabled.
        /// </summary>
        public Brush DisabledBackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the border brush in normal state.
        /// </summary>
        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                _borderPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen BorderPen => _borderPen ?? SheetUtils.ButtonBorderPen;

        /// <summary>
        /// Gets or sets the border brush when hovered.
        /// </summary>
        public Brush HoverBorderBrush
        {
            get => _hoverBorderBrush;
            set
            {
                _hoverBorderBrush = value;
                _hoverBorderPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen HoverBorderPen => _hoverBorderPen ?? SheetUtils.ButtonHoverBorderPen;

        /// <summary>
        /// Gets or sets the border brush when pressed.
        /// </summary>
        public Brush PressedBorderBrush
        {
            get => _pressedBorderBrush;
            set
            {
                _pressedBorderBrush = value;
                _pressedBorderPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen PressedBorderPen => _pressedBorderPen ?? SheetUtils.ButtonPressedBorderPen;

        /// <summary>
        /// Gets or sets the border brush when disabled.
        /// </summary>
        public Brush DisabledBorderBrush
        {
            get => _disabledBorderBrush;
            set
            {
                _disabledBorderBrush = value;
                _disabledBorderPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen DisabledBorderPen => _disabledBorderPen ?? SheetUtils.ButtonDisabledBorderPen;

        /// <summary>
        /// Gets or sets the text foreground brush in normal state.
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// Gets or sets the text foreground brush when hovered.
        /// </summary>
        public Brush HoverForeground { get; set; }

        /// <summary>
        /// Gets or sets the text foreground brush when pressed.
        /// </summary>
        public Brush PressedForeground { get; set; }

        /// <summary>
        /// Gets or sets the text foreground brush when disabled.
        /// </summary>
        public Brush DisabledForeground { get; set; }

        /// <summary>
        /// Fires when the button cell is clicked.
        /// </summary>
        public event EventHandler<CellButtonClickedEventArgs> Click;

        /// <summary>
        /// Invoked when the button element is clicked.
        /// </summary>
        public virtual void OnClick(ISheetView view, int row, int col)
        {
            var worksheet = view?.WorkSheet as Worksheet;
            var sheetCol = ((Columns)worksheet?.Columns)?.GetItem(col);
            var sheetRow = ((Rows)worksheet?.Rows)?.GetItem(row);

            bool locked = (worksheet?.GetLocked(row, col) == true) ||
                (sheetRow != null && sheetRow.Locked) ||
                (sheetCol != null && sheetCol.Locked);

            if (locked) return;

            var args = new CellButtonClickedEventArgs(view, row, col, this);
            Click?.Invoke(this, args);

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            yield return _buttonElement ?? (_buttonElement = new ButtonElement(this));
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            double margin = ButtonMargin * zoom;
            var rawRect = new Rect(cellRect.X + margin, cellRect.Y + margin, Math.Max(0, cellRect.Width - margin * 2), Math.Max(0, cellRect.Height - margin * 2));
            if (rawRect.Width <= 0 || rawRect.Height <= 0) return;

            double dpi = renderContext.PixelPerDip > 0 ? renderContext.PixelPerDip : 1.0;
            double penThickness = 1.0;
            double x1 = PixelSnapper.SnapLine(rawRect.Left, dpi, penThickness);
            double y1 = PixelSnapper.SnapLine(rawRect.Top, dpi, penThickness);
            double x2 = PixelSnapper.SnapLine(rawRect.Right, dpi, penThickness);
            double y2 = PixelSnapper.SnapLine(rawRect.Bottom, dpi, penThickness);
            var buttonRect = new Rect(x1, y1, x2 - x1, y2 - y1);
            int radius = (int)Math.Max(1, Math.Round(CornerRadius * zoom));

            var bg = BackgroundBrush ?? SheetUtils.ButtonBackgroundBrush;
            var pen = BorderPen;
            renderContext.DrawRoundedRectangle(bg, pen, buttonRect, radius, radius);

            DrawButtonText(renderContext, value, style, buttonRect);
        }

        internal void DrawButtonText(IRenderContext renderContext, object value, IStyle style, Rect buttonRect, Brush overrideForeground = null)
        {
            string displayText = !string.IsNullOrEmpty(Text) ? Text : value?.ToString();
            if (string.IsNullOrEmpty(displayText)) return;

            var hAlign = ButtonHorizontalAlignment;
            if (hAlign == CellHorizontalAlignment.Auto)
                hAlign = CellHorizontalAlignment.Center;

            var vAlign = ButtonVerticalAlignment;
            if (vAlign == CellVerticalAlignment.Auto)
                vAlign = CellVerticalAlignment.Center;

            var foreColor = GetDrawingColor(overrideForeground ?? Foreground) ?? style?.ForeColor ?? DrawingColor.FromArgb(255, 17, 24, 39);

            renderContext.DrawText(
                displayText,
                buttonRect,
                style?.FontFamily,
                style != null ? style.FontSize : 11,
                style != null ? style.FontWeight : DrawingFontWeight.Normal,
                style != null ? style.FontStyle : DrawingFontStyle.Normal,
                foreColor,
                hAlign,
                vAlign,
                style != null ? style.TextTrimming : CellTextTrimming.None,
                style?.AllowMultiLineText == true);
        }

        private static DrawingColor? GetDrawingColor(Brush brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return new DrawingColor(scb.Color.R, scb.Color.G, scb.Color.B, scb.Color.A);
            }
            return null;
        }

        public override bool SupportsEditing => false;
    }

    #region Elements

    /// <summary>
    /// Interactive button element providing hit-testing, hover/pressed visual states, and click event/command execution.
    /// </summary>
    public class ButtonElement : CellElement
    {
        private readonly ButtonCellType _cellType;

        public ButtonElement(ButtonCellType cellType)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
        }

        public override Cursor Cursor => Cursors.Hand;

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double margin = (_cellType?.ButtonMargin ?? 2.0) * zoom;
            return new Rect(cellRect.X + margin, cellRect.Y + margin, Math.Max(0, cellRect.Width - margin * 2), Math.Max(0, cellRect.Height - margin * 2));
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            if (state == CellElementState.Normal) return;

            var bg = state == CellElementState.Pressed
                ? (_cellType?.PressedBackgroundBrush ?? SheetUtils.ButtonPressedBackgroundBrush)
                : (_cellType?.HoverBackgroundBrush ?? SheetUtils.ButtonHoverBackgroundBrush);

            var pen = state == CellElementState.Pressed
                ? (_cellType?.PressedBorderPen ?? SheetUtils.ButtonPressedBorderPen)
                : (_cellType?.HoverBorderPen ?? SheetUtils.ButtonHoverBorderPen);

            var textBrush = state == CellElementState.Pressed
                ? (_cellType?.PressedForeground ?? _cellType?.Foreground)
                : (_cellType?.HoverForeground ?? _cellType?.Foreground);

            int radius = (int)Math.Max(1, Math.Round((_cellType?.CornerRadius ?? 3.0) * context.ZoomFactor));
            context.DrawRoundedRectangle(bg, pen, bounds, radius, radius);

            var ws = context.SheetView?.WorkSheet;
            var value = ws?.GetValue(row, col);
            var style = ws?.GetCellStyle(row, col, null, null);

            if (_cellType != null)
            {
                _cellType.DrawButtonText(context, value, style, bounds, textBrush);
            }
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            if (_cellType != null)
            {
                _cellType.OnClick(view, row, col);
            }
            else
            {
                base.OnClick(view, row, col);
            }
        }
    }

    #endregion
}

