using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Event arguments providing cell coordinates and new value when a slider is adjusted.
    /// </summary>
    public class SliderValueChangedEventArgs : EventArgs
    {
        public int Row { get; }
        public int Column { get; }
        public double Value { get; }

        public SliderValueChangedEventArgs(int row, int col, double val)
        {
            Row = row;
            Column = col;
            Value = val;
        }
    }

    /// <summary>
    /// Interactive in-cell draggable slider cell type for real-time scenario modeling, parameter tuning,
    /// and visual numeric adjustments.
    /// </summary>
    public class SliderCellType : BaseCellType
    {
        private readonly SliderElement _sliderElement;
        private Brush _trackBrush;
        private Pen _trackPen;
        private Brush _fillBrush;
        private Brush _thumbBrush;
        private Brush _thumbBorderBrush;
        private Pen _thumbBorderPen;

        public SliderCellType()
        {
            _sliderElement = new SliderElement(this);
        }

        #region Range and Stepping Properties

        /// <summary>
        /// Gets or sets the minimum slider value. Default is 0.0.
        /// </summary>
        public double Minimum { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the maximum slider value. Default is 100.0.
        /// </summary>
        public double Maximum { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets the snapping step increment. If 0 or less, value adjustment is continuous. Default is 1.0.
        /// </summary>
        public double Step { get; set; } = 1.0;

        #endregion

        #region Visual & Geometry Properties

        /// <summary>
        /// Gets or sets the background brush for the inactive slider track.
        /// </summary>
        public Brush TrackBrush
        {
            get => _trackBrush;
            set
            {
                _trackBrush = value;
                _trackPen = value != null ? SheetUtils.CreateFrozenPen(value, 1.0) : null;
            }
        }

        /// <summary>
        /// Gets or sets the fill brush representing the active slider range.
        /// </summary>
        public Brush FillBrush
        {
            get => _fillBrush;
            set => _fillBrush = value;
        }

        /// <summary>
        /// Gets or sets the fill brush for the draggable thumb knob.
        /// </summary>
        public Brush ThumbBrush
        {
            get => _thumbBrush;
            set => _thumbBrush = value;
        }

        /// <summary>
        /// Gets or sets the border brush for the draggable thumb knob.
        /// </summary>
        public Brush ThumbBorderBrush
        {
            get => _thumbBorderBrush;
            set
            {
                _thumbBorderBrush = value;
                _thumbBorderPen = value != null ? SheetUtils.CreateFrozenPen(value, 1.8) : null;
            }
        }

        /// <summary>
        /// Gets or sets the height of the slider groove track in device-independent units. Default is 5.0.
        /// </summary>
        public double TrackHeight { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the diameter of the circular thumb knob in device-independent units. Default is 14.0.
        /// </summary>
        public double ThumbSize { get; set; } = 14.0;

        /// <summary>
        /// Gets or sets the outer cell margin. Default is 6.0.
        /// </summary>
        public double BarMargin { get; set; } = 6.0;

        #endregion

        #region Value Label & Ticks

        /// <summary>
        /// Gets or sets whether to display the numeric value label. Default is true.
        /// </summary>
        public bool ShowValue { get; set; } = true;

        /// <summary>
        /// Gets or sets the string format used for the value label. Default is "{0:0}".
        /// </summary>
        public string ValueFormat { get; set; } = "{0:0}";

        /// <summary>
        /// Gets or sets where the numeric value label is positioned. Default is <see cref="SliderValuePlacement.Right"/>.
        /// </summary>
        public SliderValuePlacement ValuePlacement { get; set; } = SliderValuePlacement.Right;

        /// <summary>
        /// Gets or sets the foreground brush for the value label.
        /// </summary>
        public Brush ValueForeground { get; set; }

        /// <summary>
        /// Gets or sets whether to draw tick marks along the slider track. Default is false.
        /// </summary>
        public bool ShowTicks { get; set; } = false;

        /// <summary>
        /// Gets or sets the interval between tick marks when <see cref="ShowTicks"/> is true. Default is 10.0.
        /// </summary>
        public double TickFrequency { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets whether user dragging and editing is disabled. Default is false.
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the slider value is changed by user interaction.
        /// </summary>
        public event EventHandler<SliderValueChangedEventArgs> ValueChanged;

        #endregion

        #region Value Helper Methods

        /// <summary>
        /// Parses an arbitrary cell value into a valid numeric double.
        /// </summary>
        public double ParseValue(object value)
        {
            if (value == null) return Minimum;

            if (value is double d) return ClampAndStep(d);
            if (value is float f) return ClampAndStep(f);
            if (value is int i) return ClampAndStep(i);
            if (value is long l) return ClampAndStep(l);
            if (value is decimal dec) return ClampAndStep((double)dec);

            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed) ||
                double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return ClampAndStep(parsed);
            }

            return Minimum;
        }

        /// <summary>
        /// Clamps a numeric value between <see cref="Minimum"/> and <see cref="Maximum"/> and snaps to <see cref="Step"/>.
        /// </summary>
        public double ClampAndStep(double rawValue)
        {
            if (Maximum <= Minimum) return Minimum;

            double clamped = Math.Max(Minimum, Math.Min(Maximum, rawValue));
            if (Step > 0.0)
            {
                double stepped = Math.Round((clamped - Minimum) / Step) * Step + Minimum;
                clamped = Math.Max(Minimum, Math.Min(Maximum, stepped));
            }
            return clamped;
        }

        /// <summary>
        /// Calculates the normalized 0.0–1.0 ratio along the slider track for a given value.
        /// </summary>
        public double ComputeRatio(double value)
        {
            if (Maximum <= Minimum) return 0.0;
            double progress = (value - Minimum) / (Maximum - Minimum);
            return Math.Max(0.0, Math.Min(1.0, progress));
        }

        #endregion

        #region Geometry Layout

        /// <summary>
        /// Computes the track bounding rectangle and thumb center coordinates.
        /// </summary>
        internal (Rect TrackRect, Point ThumbCenter, Rect TextRect) CalculateLayout(Rect cellRect, double zoom, double value)
        {
            double margin = BarMargin * zoom;
            double thumbRadius = (ThumbSize / 2.0) * zoom;
            double trackH = TrackHeight * zoom;

            double reservedTextWidth = (ShowValue && ValuePlacement != SliderValuePlacement.None) ? 46.0 * zoom : 0.0;
            double availableWidth = Math.Max(thumbRadius * 2, cellRect.Width - margin * 2 - reservedTextWidth);

            double trackStartX = cellRect.X + margin + thumbRadius;
            if (ShowValue && ValuePlacement == SliderValuePlacement.Left)
            {
                trackStartX += reservedTextWidth;
            }

            double trackWidth = Math.Max(0, availableWidth - thumbRadius * 2);
            double centerY = cellRect.Y + cellRect.Height / 2.0;
            double trackY = centerY - trackH / 2.0;

            var trackRect = new Rect(trackStartX, trackY, trackWidth, trackH);

            double ratio = ComputeRatio(value);
            double thumbX = trackStartX + trackWidth * ratio;
            var thumbCenter = new Point(thumbX, centerY);

            Rect textRect = Rect.Empty;
            if (ShowValue && ValuePlacement != SliderValuePlacement.None)
            {
                double textW = 40.0 * zoom;
                if (ValuePlacement == SliderValuePlacement.Right)
                {
                    textRect = new Rect(cellRect.Right - margin - textW, cellRect.Y, textW, cellRect.Height);
                }
                else
                {
                    textRect = new Rect(cellRect.X + margin, cellRect.Y, textW, cellRect.Height);
                }
            }

            return (trackRect, thumbCenter, textRect);
        }

        #endregion

        #region Rendering

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            double margin = BarMargin * zoom;

            if (cellRect.Width <= margin * 2 || cellRect.Height <= margin * 2)
                return;

            double numVal = ParseValue(value);
            var (trackRect, thumbCenter, textRect) = CalculateLayout(cellRect, zoom, numVal);

            if (trackRect.Width <= 0) return;

            int trackRadius = (int)Math.Max(0, Math.Round(trackRect.Height / 2.0));

            // 1. Draw Inactive Track
            var trackBrush = TrackBrush ?? SheetUtils.SliderTrackBrush;
            var trackPen = _trackPen ?? SheetUtils.SliderTrackPen;
            renderContext.DrawRoundedRectangle(trackBrush, trackPen, trackRect, trackRadius, trackRadius);

            // 2. Draw Active Fill Bar
            double fillWidth = Math.Max(0, thumbCenter.X - trackRect.X);
            if (fillWidth > 0)
            {
                var fillRect = new Rect(trackRect.X, trackRect.Y, fillWidth, trackRect.Height);
                var fillBrush = FillBrush ?? SheetUtils.SliderFillBrush;
                int fillRadius = Math.Min(trackRadius, (int)Math.Round(fillWidth / 2.0));
                renderContext.DrawRoundedRectangle(fillBrush, null, fillRect, fillRadius, fillRadius);
            }

            // 3. Draw Optional Tick Marks
            if (ShowTicks && TickFrequency > 0 && Maximum > Minimum)
            {
                double tickPenY1 = trackRect.Bottom + 2.0 * zoom;
                double tickPenY2 = tickPenY1 + 4.0 * zoom;
                var tickPen = SheetUtils.SliderTickPen;

                for (double tVal = Minimum; tVal <= Maximum; tVal += TickFrequency)
                {
                    double tRatio = (tVal - Minimum) / (Maximum - Minimum);
                    double tX = trackRect.X + trackRect.Width * tRatio;
                    renderContext.DrawLine(tickPen, new Point(tX, tickPenY1), new Point(tX, tickPenY2));
                }
            }

            // 4. Draw Thumb Knob
            double thumbRadius = (ThumbSize / 2.0) * zoom;
            var thumbBrush = ThumbBrush ?? SheetUtils.SliderThumbBrush;
            var thumbBorderPen = _thumbBorderPen ?? SheetUtils.SliderThumbBorderPen;
            renderContext.DrawEllipse(thumbBrush, thumbBorderPen, thumbCenter, thumbRadius, thumbRadius);

            // 5. Draw Value Text
            if (ShowValue && ValuePlacement != SliderValuePlacement.None && !textRect.IsEmpty)
            {
                string displayText = string.Format(CultureInfo.CurrentCulture, ValueFormat, numVal);
                var textBrush = ValueForeground ?? (style != null ? WpfResourceCache.GetBrush(style.ForeColor) : SheetUtils.SliderTextBrush);
                var hAlign = ValuePlacement == SliderValuePlacement.Right ? CellHorizontalAlignment.Right : CellHorizontalAlignment.Left;

                renderContext.DrawText(
                    displayText,
                    textRect,
                    style?.FontFamily,
                    style != null ? Math.Max(9.0, style.FontSize - 1.0) : 10.0,
                    DrawingFontWeight.Normal,
                    style != null ? style.FontStyle : DrawingFontStyle.Normal,
                    textBrush,
                    hAlign,
                    CellVerticalAlignment.Center,
                    CellTextTrimming.Character,
                    false);
            }
        }

        #endregion

        #region Interactive Elements & Mouse Routing

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (!IsReadOnly)
            {
                yield return _sliderElement;
            }
        }

        public override void OnElementMouseDown(ISheetView view, int row, int col, CellElement element)
        {
            if (IsReadOnly) return;
            var sheetView = view as SheetView;
            if (sheetView == null) return;

            var interactionLayer = sheetView.CellsSurface?.GetInteractionLayer();
            if (interactionLayer != null)
            {
                var mousePos = Mouse.GetPosition(interactionLayer);
                UpdateValueFromMouse(sheetView, row, col, mousePos);
            }
        }

        public override void OnElementMouseMove(ISheetView view, int row, int col, CellElement element, Point currentPoint)
        {
            if (IsReadOnly) return;
            var sheetView = view as SheetView;
            if (sheetView == null) return;
            UpdateValueFromMouse(sheetView, row, col, currentPoint);
        }

        private void UpdateValueFromMouse(SheetView sheetView, int row, int col, Point mousePoint)
        {
            var worksheet = sheetView.WorkSheet as Worksheet;
            var viewPort = sheetView.ViewPort as ViewPort;
            if (worksheet == null || viewPort == null) return;

            var cellRect = viewPort.GetCellRect(row, col);
            double zoom = sheetView.ZoomFactor > 0 ? sheetView.ZoomFactor : 1.0;

            var unscaled = new Rect(
                cellRect.X - viewPort.LeftColumnLocation,
                cellRect.Y - viewPort.TopRowLocation,
                cellRect.Width,
                cellRect.Height);
            var scaledCellRect = new Rect(
                unscaled.X * zoom,
                unscaled.Y * zoom,
                unscaled.Width * zoom,
                unscaled.Height * zoom);

            object currentValObj = worksheet.GetValue(row, col);
            double currentVal = ParseValue(currentValObj);

            var (trackRect, _, _) = CalculateLayout(scaledCellRect, zoom, currentVal);
            if (trackRect.Width <= 0) return;

            double ratio = (mousePoint.X - trackRect.X) / trackRect.Width;
            ratio = Math.Max(0.0, Math.Min(1.0, ratio));

            double rawValue = Minimum + ratio * (Maximum - Minimum);
            double steppedValue = ClampAndStep(rawValue);

            if (Math.Abs(steppedValue - currentVal) > 0.000001)
            {
                worksheet.SetValue(row, col, steppedValue);
                sheetView.Spread?.InvalidateVisual();
                ValueChanged?.Invoke(this, new SliderValueChangedEventArgs(row, col, steppedValue));
            }
        }

        #endregion

        #region Editing

        public override bool SupportsEditing => !IsReadOnly;

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new NumericCellEditor();
        }

        #endregion

        #region Nested SliderElement

        /// <summary>
        /// Sub-element representing the slider thumb and track for hit-testing and interaction layer rendering.
        /// </summary>
        private class SliderElement : CellElement
        {
            private readonly SliderCellType _cellType;

            public SliderElement(SliderCellType cellType)
            {
                _cellType = cellType;
            }

            public override Cursor Cursor => Cursors.Hand;

            public override Rect GetBounds(Rect cellRect, double zoom)
            {
                double thumbRadius = (_cellType.ThumbSize / 2.0) * zoom;
                var (trackRect, _, _) = _cellType.CalculateLayout(cellRect, zoom, _cellType.Minimum);
                // Expand track bounds by thumb radius to comfortably hit the thumb at ends
                return new Rect(
                    trackRect.X - thumbRadius,
                    cellRect.Y,
                    trackRect.Width + thumbRadius * 2,
                    cellRect.Height);
            }

            public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
            {
                if (state == CellElementState.Normal) return;

                // On Hover or Pressed: Draw soft glowing halo around the thumb knob
                double zoom = context.ZoomFactor > 0 ? context.ZoomFactor : 1.0;
                double haloRadius = ((_cellType.ThumbSize / 2.0) + 3.5) * zoom;

                var scaledCellRect = context.GetCellRect(row, col);
                object cellVal = context.SheetView?.WorkSheet?.GetValue(row, col);
                double numVal = _cellType.ParseValue(cellVal);

                var (_, thumbCenter, _) = _cellType.CalculateLayout(scaledCellRect, zoom, numVal);

                var haloBrush = (state == CellElementState.Pressed)
                    ? SheetUtils.SliderThumbPressedHaloBrush
                    : SheetUtils.SliderThumbHoverHaloBrush;

                context.DrawEllipse(haloBrush, null, thumbCenter, haloRadius, haloRadius);
            }
        }

        #endregion
    }
}
