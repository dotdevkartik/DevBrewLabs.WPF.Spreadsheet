using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    public class CheckBoxCellType : BaseCellType
    {
        internal static Size CheckBoxSize { get; } = new Size(14, 14);

        private Brush _checkedBrush;
        private Pen _checkedPen;
        private Brush _indeterminateBrush;
        private Pen _indeterminatePen;
        private Brush _uncheckedBorderBrush;
        private Pen _uncheckedBorderPen;
        private CheckBoxElement _element;

        public bool IsThreeState { get; set; }

        /// <summary>
        /// Gets or sets the brush used for the background when checked.
        /// </summary>
        public Brush CheckedBrush
        {
            get => _checkedBrush;
            set
            {
                _checkedBrush = value;
                _checkedPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen CheckedPen => _checkedPen ?? SheetUtils.CheckBoxCheckedPen;

        /// <summary>
        /// Gets or sets the brush used for the background when indeterminate.
        /// </summary>
        public Brush IndeterminateBrush
        {
            get => _indeterminateBrush;
            set
            {
                _indeterminateBrush = value;
                _indeterminatePen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen IndeterminatePen => _indeterminatePen ?? SheetUtils.CheckBoxIndeterminatePen;

        /// <summary>
        /// Gets or sets the brush used for the border when unchecked.
        /// </summary>
        public Brush UncheckedBorderBrush
        {
            get => _uncheckedBorderBrush;
            set
            {
                _uncheckedBorderBrush = value;
                _uncheckedBorderPen = SheetUtils.CreateFrozenPen(value, 1.2);
            }
        }

        internal Pen UncheckedBorderPen => _uncheckedBorderPen ?? SheetUtils.CheckBoxUncheckedBorderPen;

        /// <summary>
        /// Gets or sets the background brush when unchecked.
        /// </summary>
        public Brush UncheckedBackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush used for drawing the checkmark or indeterminate dash icon.
        /// </summary>
        public Brush CheckMarkBrush { get; set; }

        /// <summary>
        /// Gets or sets the hover glow brush used by the checkbox element.
        /// </summary>
        public Brush HoverGlowBrush { get; set; }

        /// <summary>
        /// Gets or sets the pressed glow brush used by the checkbox element.
        /// </summary>
        public Brush PressedGlowBrush { get; set; }

        public CheckBoxCellType()
        {
            IsThreeState = false;
        }

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            yield return _element ?? (_element = new CheckBoxElement(this));
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            var scaledCheckBoxSize = new Size(CheckBoxSize.Width * zoom, CheckBoxSize.Height * zoom);
            var rawCheckBoxRect = cellRect.ToCellCheckBoxRect(scaledCheckBoxSize);
            
            double dpi = renderContext.PixelPerDip > 0 ? renderContext.PixelPerDip : 1.0;
            double penThickness = 1.0;
            double x1 = PixelSnapper.SnapLine(rawCheckBoxRect.Left, dpi, penThickness);
            double y1 = PixelSnapper.SnapLine(rawCheckBoxRect.Top, dpi, penThickness);
            double x2 = PixelSnapper.SnapLine(rawCheckBoxRect.Right, dpi, penThickness);
            double y2 = PixelSnapper.SnapLine(rawCheckBoxRect.Bottom, dpi, penThickness);
            var checkBoxRect = new Rect(x1, y1, x2 - x1, y2 - y1);

            int radius = (int)Math.Max(1, Math.Round(2.5 * zoom));

            bool isChecked = false;
            bool isIndeterminate = false;

            if (IsThreeState && value == null)
            {
                isIndeterminate = true;
            }
            else if (value != null)
            {
                try
                {
                    isChecked = Convert.ToBoolean(value);
                }
                catch
                {
                    isChecked = false;
                }
            }

            if (isChecked)
            {
                var bgBrush = CheckedBrush ?? SheetUtils.CheckBoxCheckedBrush;
                var pen = CheckedPen;
                renderContext.DrawRoundedRectangle(bgBrush, pen, checkBoxRect, radius, radius);

                DrawCheckMark(renderContext, checkBoxRect, zoom);
            }
            else if (isIndeterminate)
            {
                var bgBrush = IndeterminateBrush ?? CheckedBrush ?? SheetUtils.CheckBoxIndeterminateBrush;
                var pen = _indeterminateBrush != null ? IndeterminatePen : (_checkedBrush != null ? CheckedPen : SheetUtils.CheckBoxIndeterminatePen);
                renderContext.DrawRoundedRectangle(bgBrush, pen, checkBoxRect, radius, radius);

                DrawIndeterminateDash(renderContext, checkBoxRect, zoom);
            }
            else
            {
                var bgBrush = UncheckedBackgroundBrush ?? SheetUtils.CheckBoxUncheckedBgBrush;
                var borderPen = UncheckedBorderPen;
                renderContext.DrawRoundedRectangle(bgBrush, borderPen, checkBoxRect, radius, radius);
            }
        }

        private void DrawCheckMark(IRenderContext renderContext, Rect checkBoxRect, double zoom)
        {
            var checkGeometry = new StreamGeometry();
            using (var ctx = checkGeometry.Open())
            {
                var p1 = new Point(checkBoxRect.Left + checkBoxRect.Width * 0.25, checkBoxRect.Top + checkBoxRect.Height * 0.52);
                var p2 = new Point(checkBoxRect.Left + checkBoxRect.Width * 0.44, checkBoxRect.Top + checkBoxRect.Height * 0.72);
                var p3 = new Point(checkBoxRect.Left + checkBoxRect.Width * 0.76, checkBoxRect.Top + checkBoxRect.Height * 0.28);

                ctx.BeginFigure(p1, false, false);
                ctx.LineTo(p2, true, true);
                ctx.LineTo(p3, true, true);
            }
            checkGeometry.Freeze();

            var markBrush = CheckMarkBrush ?? SheetUtils.CheckBoxCheckMarkBrush;
            var markPen = WpfResourceCache.GetPen(markBrush, Math.Max(1.4, 1.8 * zoom), PenLineCap.Round, PenLineJoin.Round);

            renderContext.DrawGeometry(null, markPen, checkGeometry);
        }

        private void DrawIndeterminateDash(IRenderContext renderContext, Rect checkBoxRect, double zoom)
        {
            var dashGeometry = new StreamGeometry();
            using (var ctx = dashGeometry.Open())
            {
                var p1 = new Point(checkBoxRect.Left + checkBoxRect.Width * 0.28, checkBoxRect.Top + checkBoxRect.Height * 0.50);
                var p2 = new Point(checkBoxRect.Right - checkBoxRect.Width * 0.28, checkBoxRect.Top + checkBoxRect.Height * 0.50);

                ctx.BeginFigure(p1, false, false);
                ctx.LineTo(p2, true, true);
            }
            dashGeometry.Freeze();

            var markBrush = CheckMarkBrush ?? SheetUtils.CheckBoxCheckMarkBrush;
            var dashPen = WpfResourceCache.GetPen(markBrush, Math.Max(1.5, 2.0 * zoom), PenLineCap.Round, PenLineJoin.Round);

            renderContext.DrawGeometry(null, dashPen, dashGeometry);
        }

        /// <summary>
        /// Computes the next value in the state cycle.
        /// </summary>
        /// <param name="currentValue">The current cell value.</param>
        /// <returns>The next boolean or null value.</returns>
        public object GetNextValue(object currentValue)
        {
            if (IsThreeState)
            {
                if (currentValue == null)
                    return false;

                bool boolVal;
                try
                {
                    boolVal = Convert.ToBoolean(currentValue);
                }
                catch
                {
                    boolVal = false;
                }

                if (!boolVal)
                    return true;
                else
                    return null;
            }
            else
            {
                if (currentValue == null)
                    return true;

                try
                {
                    return !Convert.ToBoolean(currentValue);
                }
                catch
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Toggles the cell value and records an undo action.
        /// </summary>
        public bool ToggleValue(ISheetView view, int row, int col, EditTrigger trigger = EditTrigger.Programmatic)
        {
            var sheetView = view as SheetView;
            if (sheetView == null) return false;

            var worksheet = sheetView.WorkSheet as Worksheet;
            if (worksheet == null) return false;

            var columns = worksheet.Columns as Columns;
            var rows = worksheet.Rows as Rows;
            var sheetCol = columns?.GetItem(col);
            var sheetRow = rows?.GetItem(row);

            bool locked = worksheet.GetLocked(row, col) ||
                (sheetRow != null && sheetRow.Locked) ||
                (sheetCol != null && sheetCol.Locked);

            if (locked) return false;

            var startingArgs = new CellEditStartingEventArgs(sheetView, row, col, trigger);
            if (sheetView.Spread != null && !sheetView.Spread.RaiseCellEditStarting(startingArgs))
                return false;

            object currentValue = worksheet.GetValue(row, col);
            object nextValue = GetNextValue(currentValue);

            var endingArgs = new CellEditEndingEventArgs(sheetView, row, col, nextValue);
            if (sheetView.Spread != null && !sheetView.Spread.RaiseCellEditEnding(endingArgs))
            {
                sheetView.Spread?.RaiseCellEditEnded(new CellEditEndedEventArgs(sheetView, row, col, false));
                return false;
            }

            var cellChangedAction = new CellChangedAction { SheetView = sheetView };
            cellChangedAction.OldState.Value = currentValue;
            cellChangedAction.OldState.Row = row;
            cellChangedAction.OldState.Column = col;
            cellChangedAction.OldState.Selection = sheetView.Selection.Clone();

            worksheet.SetValue(row, col, nextValue);

            cellChangedAction.NewState.Value = worksheet.GetValue(row, col);
            cellChangedAction.NewState.Row = row;
            cellChangedAction.NewState.Column = col;
            cellChangedAction.NewState.Selection = sheetView.Selection.Clone();

            sheetView.Spread?.UndoRedoManager?.AddAction(cellChangedAction);
            sheetView.Spread?.RaiseCellEditEnded(new CellEditEndedEventArgs(sheetView, row, col, true));
            return true;
        }

        public override bool SupportsEditing => false;
    }

    #region Elements

    /// <summary>
    /// Interactive sub-element for CheckBox cells providing hit-testing, hover/pressed visual states, and click toggling.
    /// </summary>
    public class CheckBoxElement : CellElement
    {
        private readonly CheckBoxCellType _cellType;

        public CheckBoxElement(CheckBoxCellType cellType)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
        }

        public override Cursor Cursor => Cursors.Hand;

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            var scaledSize = new Size(CheckBoxCellType.CheckBoxSize.Width * zoom, CheckBoxCellType.CheckBoxSize.Height * zoom);
            var boxRect = cellRect.ToCellCheckBoxRect(scaledSize);
            // Include comfortable 3px padding for hit-testing
            boxRect.Inflate(3.0 * zoom, 3.0 * zoom);
            return boxRect;
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            int radius = (int)Math.Max(2, Math.Round(4.0 * context.ZoomFactor));
            if (state == CellElementState.Hover)
            {
                var brush = _cellType?.HoverGlowBrush ?? SheetUtils.CheckBoxHoverGlowBrush;
                if (brush != null)
                {
                    context.DrawRoundedRectangle(brush, null, bounds, radius, radius);
                }
            }
            else if (state == CellElementState.Pressed)
            {
                var brush = _cellType?.PressedGlowBrush ?? SheetUtils.CheckBoxPressedGlowBrush;
                if (brush != null)
                {
                    context.DrawRoundedRectangle(brush, null, bounds, radius, radius);
                }
            }
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            if (_cellType != null)
            {
                _cellType.ToggleValue(view, row, col);
            }
            else
            {
                base.OnClick(view, row, col);
            }
        }
    }

    #endregion
}

