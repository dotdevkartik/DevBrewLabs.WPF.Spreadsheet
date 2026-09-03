using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// In-cell micro-chart cell type supporting Line, Column, Win/Loss, and Area sparklines with highlight markers.
    /// </summary>
    public class SparklineCellType : BaseCellType
    {
        private static readonly char[] Delimiters = new[] { ',', ';', ' ', '\t', '\r', '\n' };

        #region Properties

        /// <summary>
        /// Gets or sets the sparkline visual chart representation.
        /// </summary>
        public SparklineType Type { get; set; } = SparklineType.Line;

        /// <summary>
        /// Gets or sets the primary series brush for the sparkline (line or bars).
        /// </summary>
        public Brush SeriesBrush { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the line in line and area sparklines.
        /// </summary>
        public double LineThickness { get; set; } = 1.5;

        /// <summary>
        /// Gets or sets the fill brush for area sparklines.
        /// </summary>
        public Brush AreaBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush for negative values/bars.
        /// </summary>
        public Brush NegativeBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush for the highest data point.
        /// </summary>
        public Brush HighPointBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush for the lowest data point.
        /// </summary>
        public Brush LowPointBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush for the first data point.
        /// </summary>
        public Brush FirstPointBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush for the last data point.
        /// </summary>
        public Brush LastPointBrush { get; set; }

        /// <summary>
        /// Gets or sets the brush for intermediate markers when <see cref="ShowMarkers"/> is enabled.
        /// </summary>
        public Brush MarkerBrush { get; set; }

        /// <summary>
        /// Gets or sets the diameter of marker circles.
        /// </summary>
        public double MarkerSize { get; set; } = 3.5;

        /// <summary>
        /// Gets or sets whether all data point markers are displayed on line sparklines.
        /// </summary>
        public bool ShowMarkers { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the highest data point is highlighted.
        /// </summary>
        public bool ShowHighPoint { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the lowest data point is highlighted.
        /// </summary>
        public bool ShowLowPoint { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the first data point is highlighted.
        /// </summary>
        public bool ShowFirstPoint { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the last data point is highlighted.
        /// </summary>
        public bool ShowLastPoint { get; set; } = false;

        /// <summary>
        /// Gets or sets whether negative data points are highlighted with <see cref="NegativeBrush"/>.
        /// </summary>
        public bool ShowNegativePoints { get; set; } = false;

        /// <summary>
        /// Gets or sets whether a horizontal zero axis line is rendered.
        /// </summary>
        public bool ShowZeroAxis { get; set; } = false;

        /// <summary>
        /// Gets or sets the pen for the horizontal zero axis.
        /// </summary>
        public Pen AxisPen { get; set; }

        /// <summary>
        /// Gets or sets the internal margin around the sparkline within the cell rectangle.
        /// </summary>
        public double Margin { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets an optional manual minimum value for chart scaling.
        /// </summary>
        public double? ManualMin { get; set; }

        /// <summary>
        /// Gets or sets an optional manual maximum value for chart scaling.
        /// </summary>
        public double? ManualMax { get; set; }

        /// <summary>
        /// Gets or sets an optional selector function to extract numeric data from cell values.
        /// </summary>
        public Func<object, IReadOnlyList<double>> DataSelector { get; set; }

        /// <summary>
        /// Gets or sets optional static data points to use when cell value is null.
        /// </summary>
        public IReadOnlyList<double> StaticData { get; set; }

        #endregion

        #region Data Parsing

        /// <summary>
        /// Parses a cell value into a list of numeric data points.
        /// </summary>
        /// <param name="value">The raw cell value.</param>
        /// <returns>A read-only list of double values.</returns>
        public IReadOnlyList<double> ParseDataPoints(object value)
        {
            if (value == null)
            {
                return StaticData ?? Array.Empty<double>();
            }

            if (DataSelector != null)
            {
                var result = DataSelector(value);
                if (result != null) return result;
            }

            if (value is IReadOnlyList<double> listD)
                return listD;

            if (value is double[] arrD)
                return arrD;

            if (value is IEnumerable<double> enumD)
            {
                var list = new List<double>();
                foreach (var item in enumD) list.Add(item);
                return list;
            }

            if (value is IEnumerable<float> enumF)
            {
                var list = new List<double>();
                foreach (var item in enumF) list.Add(item);
                return list;
            }

            if (value is IEnumerable<int> enumI)
            {
                var list = new List<double>();
                foreach (var item in enumI) list.Add(item);
                return list;
            }

            if (value is IEnumerable<decimal> enumDec)
            {
                var list = new List<double>();
                foreach (var item in enumDec) list.Add((double)item);
                return list;
            }

            if (value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                    return StaticData ?? Array.Empty<double>();

                var tokens = str.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<double>(tokens.Length);
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (double.TryParse(tokens[i], NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed) ||
                        double.TryParse(tokens[i], NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                    {
                        list.Add(parsed);
                    }
                }
                return list;
            }

            if (value is IEnumerable enumObj)
            {
                var list = new List<double>();
                foreach (var item in enumObj)
                {
                    if (item == null) continue;
                    try
                    {
                        list.Add(Convert.ToDouble(item, CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                        // Ignore unconvertible items
                    }
                }
                return list;
            }

            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double singleVal))
            {
                return new[] { singleVal };
            }

            return StaticData ?? Array.Empty<double>();
        }

        #endregion

        #region Rendering

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            var data = ParseDataPoints(value);
            if (data == null || data.Count == 0)
                return;

            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            double pad = Margin * zoom;
            var plotRect = new Rect(cellRect.X + pad, cellRect.Y + pad, Math.Max(0, cellRect.Width - pad * 2), Math.Max(0, cellRect.Height - pad * 2));
            if (plotRect.Width <= 1 || plotRect.Height <= 1)
                return;

            // Compute range and extreme indices
            double min = double.MaxValue;
            double max = double.MinValue;
            int highIndex = 0;
            int lowIndex = 0;

            for (int i = 0; i < data.Count; i++)
            {
                double v = data[i];
                if (v < min)
                {
                    min = v;
                    lowIndex = i;
                }
                if (v > max)
                {
                    max = v;
                    highIndex = i;
                }
            }

            if (ManualMin.HasValue) min = ManualMin.Value;
            if (ManualMax.HasValue) max = ManualMax.Value;

            if (ShowZeroAxis || Type == SparklineType.Column)
            {
                min = Math.Min(min, 0.0);
                max = Math.Max(max, 0.0);
            }

            if (Math.Abs(max - min) < 1e-9)
            {
                min -= 1.0;
                max += 1.0;
            }

            // Draw zero baseline if enabled
            if (ShowZeroAxis)
            {
                double y0 = plotRect.Bottom - ((0.0 - min) / (max - min)) * plotRect.Height;
                var axisPen = AxisPen ?? SheetUtils.SparklineAxisPen;
                if (axisPen != null)
                {
                    renderContext.DrawLine(axisPen, new Point(plotRect.Left, y0), new Point(plotRect.Right, y0));
                }
            }

            switch (Type)
            {
                case SparklineType.Line:
                    DrawLineSparkline(renderContext, plotRect, data, min, max, highIndex, lowIndex, zoom, isArea: false);
                    break;
                case SparklineType.Area:
                    DrawLineSparkline(renderContext, plotRect, data, min, max, highIndex, lowIndex, zoom, isArea: true);
                    break;
                case SparklineType.Column:
                    DrawColumnSparkline(renderContext, plotRect, data, min, max, highIndex, lowIndex, zoom);
                    break;
                case SparklineType.WinLoss:
                    DrawWinLossSparkline(renderContext, plotRect, data, zoom);
                    break;
            }
        }

        private void DrawLineSparkline(
            IRenderContext context,
            Rect plotRect,
            IReadOnlyList<double> data,
            double min,
            double max,
            int highIndex,
            int lowIndex,
            double zoom,
            bool isArea)
        {
            int n = data.Count;
            if (n == 0) return;

            var points = new Point[n];
            double range = max - min;

            if (n == 1)
            {
                points[0] = new Point(plotRect.Left + plotRect.Width / 2.0, plotRect.Bottom - ((data[0] - min) / range) * plotRect.Height);
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    double x = plotRect.Left + (i / (double)(n - 1)) * plotRect.Width;
                    double y = plotRect.Bottom - ((data[i] - min) / range) * plotRect.Height;
                    points[i] = new Point(x, y);
                }
            }

            // Draw Area Fill if requested
            if (isArea && n > 1)
            {
                double baselineY = Math.Max(plotRect.Top, Math.Min(plotRect.Bottom, plotRect.Bottom - ((0.0 - min) / range) * plotRect.Height));
                var areaGeometry = new StreamGeometry();
                using (var ctx = areaGeometry.Open())
                {
                    ctx.BeginFigure(new Point(points[0].X, baselineY), true, true);
                    for (int i = 0; i < n; i++)
                    {
                        ctx.LineTo(points[i], true, false);
                    }
                    ctx.LineTo(new Point(points[n - 1].X, baselineY), true, false);
                }
                areaGeometry.Freeze();

                var areaBrush = AreaBrush ?? SheetUtils.SparklineAreaBrush;
                context.DrawGeometry(areaBrush, null, areaGeometry);
            }

            // Draw Polyline
            if (n > 1)
            {
                var lineGeometry = new StreamGeometry();
                using (var ctx = lineGeometry.Open())
                {
                    ctx.BeginFigure(points[0], false, false);
                    for (int i = 1; i < n; i++)
                    {
                        ctx.LineTo(points[i], true, false);
                    }
                }
                lineGeometry.Freeze();

                var lineBrush = SeriesBrush ?? SheetUtils.SparklineSeriesBrush;
                var linePen = WpfResourceCache.GetPen(lineBrush, Math.Max(1.0, LineThickness * zoom), PenLineCap.Round, PenLineJoin.Round);
                context.DrawGeometry(null, linePen, lineGeometry);
            }

            // Draw Markers
            double markerRadius = Math.Max(1.5, (MarkerSize * zoom) / 2.0);

            for (int i = 0; i < n; i++)
            {
                bool isFirst = (i == 0);
                bool isLast = (i == n - 1);
                bool isHigh = (i == highIndex);
                bool isLow = (i == lowIndex);
                bool isNegative = (data[i] < 0);

                bool drawMarker = ShowMarkers;
                Brush markerBrush = MarkerBrush ?? SheetUtils.SparklineMarkerBrush;

                if (isNegative && ShowNegativePoints)
                {
                    drawMarker = true;
                    markerBrush = NegativeBrush ?? SheetUtils.SparklineNegativeBrush;
                }
                else if (isHigh && ShowHighPoint)
                {
                    drawMarker = true;
                    markerBrush = HighPointBrush ?? SheetUtils.SparklineHighPointBrush;
                }
                else if (isLow && ShowLowPoint)
                {
                    drawMarker = true;
                    markerBrush = LowPointBrush ?? SheetUtils.SparklineLowPointBrush;
                }
                else if (isFirst && ShowFirstPoint)
                {
                    drawMarker = true;
                    markerBrush = FirstPointBrush ?? SheetUtils.SparklineFirstPointBrush;
                }
                else if (isLast && ShowLastPoint)
                {
                    drawMarker = true;
                    markerBrush = LastPointBrush ?? SheetUtils.SparklineLastPointBrush;
                }

                if (drawMarker && markerBrush != null)
                {
                    context.DrawEllipse(markerBrush, null, points[i], markerRadius, markerRadius);
                }
            }
        }

        private void DrawColumnSparkline(
            IRenderContext context,
            Rect plotRect,
            IReadOnlyList<double> data,
            double min,
            double max,
            int highIndex,
            int lowIndex,
            double zoom)
        {
            int n = data.Count;
            if (n == 0) return;

            double range = max - min;
            double y0 = plotRect.Bottom - ((0.0 - min) / range) * plotRect.Height;
            double step = plotRect.Width / n;
            double barWidth = Math.Max(1.0, step * 0.72);
            double gap = (step - barWidth) / 2.0;

            for (int i = 0; i < n; i++)
            {
                double v = data[i];
                double x = plotRect.Left + i * step + gap;
                double yVal = plotRect.Bottom - ((v - min) / range) * plotRect.Height;

                double top;
                double height;

                if (v >= 0)
                {
                    top = yVal;
                    height = Math.Max(1.0, y0 - yVal);
                }
                else
                {
                    top = y0;
                    height = Math.Max(1.0, yVal - y0);
                }

                Brush barBrush;
                if (v < 0)
                {
                    barBrush = NegativeBrush ?? SheetUtils.SparklineNegativeBrush;
                }
                else if (i == highIndex && ShowHighPoint)
                {
                    barBrush = HighPointBrush ?? SheetUtils.SparklineHighPointBrush;
                }
                else if (i == lowIndex && ShowLowPoint)
                {
                    barBrush = LowPointBrush ?? SheetUtils.SparklineLowPointBrush;
                }
                else if (i == 0 && ShowFirstPoint)
                {
                    barBrush = FirstPointBrush ?? SheetUtils.SparklineFirstPointBrush;
                }
                else if (i == n - 1 && ShowLastPoint)
                {
                    barBrush = LastPointBrush ?? SheetUtils.SparklineLastPointBrush;
                }
                else
                {
                    barBrush = SeriesBrush ?? SheetUtils.SparklineSeriesBrush;
                }

                if (barBrush != null && barWidth > 0 && height > 0)
                {
                    context.DrawRectangle(barBrush, null, new Rect(x, top, barWidth, height));
                }
            }
        }

        private void DrawWinLossSparkline(
            IRenderContext context,
            Rect plotRect,
            IReadOnlyList<double> data,
            double zoom)
        {
            int n = data.Count;
            if (n == 0) return;

            double step = plotRect.Width / n;
            double barWidth = Math.Max(1.0, step * 0.72);
            double gap = (step - barWidth) / 2.0;
            double centerY = plotRect.Y + plotRect.Height / 2.0;
            double blockHeight = Math.Max(2.0, (plotRect.Height / 2.0) - 2.0 * zoom);

            for (int i = 0; i < n; i++)
            {
                double v = data[i];
                double x = plotRect.Left + i * step + gap;

                if (v > 0)
                {
                    // Win (upper block)
                    double top = centerY - blockHeight;
                    var brush = SeriesBrush ?? SheetUtils.SparklineSeriesBrush;
                    context.DrawRectangle(brush, null, new Rect(x, top, barWidth, blockHeight));
                }
                else if (v < 0)
                {
                    // Loss (lower block)
                    double top = centerY;
                    var brush = NegativeBrush ?? SheetUtils.SparklineNegativeBrush;
                    context.DrawRectangle(brush, null, new Rect(x, top, barWidth, blockHeight));
                }
                else
                {
                    // Tie / Zero (thin centered bar)
                    double top = centerY - 1.0;
                    var brush = MarkerBrush ?? SheetUtils.SparklineMarkerBrush;
                    context.DrawRectangle(brush, null, new Rect(x, top, barWidth, 2.0));
                }
            }
        }

        #endregion

        #region Editing

        /// <summary>
        /// Gets or sets whether in-place editing is disabled for this sparkline cell. Default is false.
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        public override bool SupportsEditing => !IsReadOnly;

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new SparklineCellEditor(this);
        }

        #endregion
    }
}
