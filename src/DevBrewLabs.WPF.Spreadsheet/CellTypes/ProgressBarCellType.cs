using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Cell type that renders an immediate-mode progress bar inside spreadsheet cells,
    /// supporting customizable ranges, track/fill colors, percentage labels, and in-place numeric editing.
    /// </summary>
    public class ProgressBarCellType : BaseCellType
    {
        private Brush _trackBorderBrush;
        private Pen _trackBorderPen;
        private Brush _progressBorderBrush;
        private Pen _progressBorderPen;

        /// <summary>
        /// Gets or sets the minimum possible value representing 0% progress. Default is 0.0.
        /// </summary>
        public double Minimum { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the maximum possible value representing 100% progress. Default is 100.0.
        /// </summary>
        public double Maximum { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets the background brush of the progress bar track.
        /// </summary>
        public Brush TrackBrush { get; set; }

        /// <summary>
        /// Gets or sets the fill brush representing progress.
        /// </summary>
        public Brush ProgressBrush { get; set; }

        /// <summary>
        /// Gets or sets the border brush for the track.
        /// </summary>
        public Brush TrackBorderBrush
        {
            get => _trackBorderBrush;
            set
            {
                _trackBorderBrush = value;
                _trackBorderPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen TrackBorderPen => _trackBorderPen;

        /// <summary>
        /// Gets or sets the border brush for the filled portion.
        /// </summary>
        public Brush ProgressBorderBrush
        {
            get => _progressBorderBrush;
            set
            {
                _progressBorderBrush = value;
                _progressBorderPen = SheetUtils.CreateFrozenPen(value, 1.0);
            }
        }

        internal Pen ProgressBorderPen => _progressBorderPen;

        /// <summary>
        /// Gets or sets the height of the progress bar in device-independent units.
        /// When null, the bar fills the available cell height minus <see cref="BarMargin"/>.
        /// </summary>
        public double? BarHeight { get; set; } = 14.0;

        /// <summary>
        /// Gets or sets the outer margin surrounding the progress bar inside the cell. Default is 4.0.
        /// </summary>
        public double BarMargin { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the corner radius of the progress bar. Default is 4.0.
        /// </summary>
        public double CornerRadius { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets whether to display the progress text or percentage label.
        /// </summary>
        public bool ShowText { get; set; } = true;

        /// <summary>
        /// Gets or sets the format string used to render the value or percentage. Default is "{0:0}%".
        /// </summary>
        public string Format { get; set; } = "{0:0}%";

        /// <summary>
        /// Gets or sets where the text is placed relative to the progress bar.
        /// </summary>
        public ProgressBarTextPlacement TextPlacement { get; set; } = ProgressBarTextPlacement.Overlay;

        /// <summary>
        /// Gets or sets the foreground brush for the progress text.
        /// When null, a high-contrast foreground or the cell style ForeColor is used.
        /// </summary>
        public Brush TextForeground { get; set; }

        /// <summary>
        /// Gets or sets whether the fill color dynamically transitions between Danger, Warning, and Success
        /// colors based on progress ratio (&lt; 35% Red, 35-70% Amber, &gt;= 70% Green).
        /// </summary>
        public bool AutoColor { get; set; } = false;

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            double margin = BarMargin * zoom;

            if (cellRect.Width <= margin * 2 || cellRect.Height <= margin * 2)
                return;

            double progress = ComputeProgress(value);
            double dpi = renderContext.PixelPerDip > 0 ? renderContext.PixelPerDip : 1.0;

            double reservedTextWidth = (ShowText && TextPlacement == ProgressBarTextPlacement.Right) ? 42.0 * zoom : 0.0;
            double availableWidth = Math.Max(0, cellRect.Width - margin * 2 - reservedTextWidth);

            double rawHeight = BarHeight.HasValue
                ? BarHeight.Value * zoom
                : Math.Max(2.0, cellRect.Height - margin * 2);

            double barHeight = Math.Min(rawHeight, Math.Max(2.0, cellRect.Height - margin * 2));
            double trackY = cellRect.Y + (cellRect.Height - barHeight) / 2.0;

            double x1 = PixelSnapper.SnapLine(cellRect.X + margin, dpi, 1.0);
            double y1 = PixelSnapper.SnapLine(trackY, dpi, 1.0);
            double x2 = PixelSnapper.SnapLine(cellRect.X + margin + availableWidth, dpi, 1.0);
            double y2 = PixelSnapper.SnapLine(trackY + barHeight, dpi, 1.0);

            var trackRect = new Rect(x1, y1, Math.Max(0, x2 - x1), Math.Max(0, y2 - y1));
            if (trackRect.Width <= 0 || trackRect.Height <= 0) return;

            int radius = (int)Math.Max(0, Math.Round(CornerRadius * zoom));

            // 1. Draw Track Background
            var trackBg = TrackBrush ?? SheetUtils.ProgressBarTrackBrush;
            renderContext.DrawRoundedRectangle(trackBg, TrackBorderPen, trackRect, radius, radius);

            // 2. Draw Progress Fill
            double fillWidth = trackRect.Width * progress;
            if (fillWidth > 0)
            {
                var fillRect = new Rect(trackRect.X, trackRect.Y, fillWidth, trackRect.Height);
                var fillBrush = ResolveFillBrush(progress);
                renderContext.DrawRoundedRectangle(fillBrush, ProgressBorderPen, fillRect, radius, radius);
            }

            // 3. Draw Text / Percentage
            if (ShowText && TextPlacement != ProgressBarTextPlacement.None)
            {
                string displayText = FormatProgressText(value, formatter);

                if (!string.IsNullOrEmpty(displayText))
                {
                    Rect textRect;
                    CellHorizontalAlignment hAlign;
                    Brush textBrush;

                    if (TextPlacement == ProgressBarTextPlacement.Right)
                    {
                        textRect = new Rect(x2 + 4 * zoom, cellRect.Y, Math.Max(0, cellRect.Right - (x2 + 4 * zoom) - margin), cellRect.Height);
                        hAlign = CellHorizontalAlignment.Left;
                        textBrush = TextForeground ?? (style != null ? WpfResourceCache.GetBrush(style.ForeColor) : SheetUtils.ProgressBarDarkTextBrush);
                    }
                    else // Overlay
                    {
                        textRect = trackRect;
                        hAlign = CellHorizontalAlignment.Center;

                        if (TextForeground != null)
                        {
                            textBrush = TextForeground;
                        }
                        else
                        {
                            // If progress covers center of track, use white text; else dark text
                            textBrush = progress >= 0.55
                                ? Brushes.White
                                : SheetUtils.ProgressBarOverlayDarkTextBrush;
                        }
                    }

                    renderContext.DrawText(
                        displayText,
                        textRect,
                        style?.FontFamily,
                        style != null ? Math.Max(9.0, style.FontSize - 1.0) : 10.0,
                        DrawingFontWeight.Bold,
                        style != null ? style.FontStyle : DrawingFontStyle.Normal,
                        textBrush,
                        hAlign,
                        CellVerticalAlignment.Center,
                        CellTextTrimming.Character,
                        false);
                }
            }
        }

        public double ComputeProgress(object value)
        {
            if (value == null) return 0.0;

            if (Maximum <= Minimum) return 0.0;

            double numVal;
            if (value is double d) numVal = d;
            else if (value is float f) numVal = f;
            else if (value is int i) numVal = i;
            else if (value is long l) numVal = l;
            else if (value is decimal dec) numVal = (double)dec;
            else if (!double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out numVal) &&
                     !double.TryParse(value.ToString(), out numVal))
            {
                return 0.0;
            }

            double progress = (numVal - Minimum) / (Maximum - Minimum);
            return Math.Max(0.0, Math.Min(1.0, progress));
        }

        private Brush ResolveFillBrush(double progressRatio)
        {
            if (AutoColor)
            {
                if (progressRatio < 0.35)
                    return SheetUtils.ProgressBarDangerBrush;
                if (progressRatio < 0.70)
                    return SheetUtils.ProgressBarWarningBrush;
                return SheetUtils.ProgressBarSuccessBrush;
            }

            return ProgressBrush ?? SheetUtils.ProgressBarFillBrush;
        }

        private string FormatProgressText(object value, IFormatter formatter)
        {
            double numVal = 0.0;
            if (value != null)
            {
                if (value is double d) numVal = d;
                else if (value is float f) numVal = f;
                else if (value is int i) numVal = i;
                else if (value is long l) numVal = l;
                else if (value is decimal dec) numVal = (double)dec;
                else
                {
                    double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out numVal);
                }
            }

            string result;
            if (formatter != null)
            {
                result = formatter.Format(value);
            }
            else if (!string.IsNullOrEmpty(Format))
            {
                try
                {
                    result = string.Format(CultureInfo.InvariantCulture, Format, numVal);
                }
                catch
                {
                    result = numVal.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                result = numVal.ToString(CultureInfo.InvariantCulture);
            }

            return result;
        }

        public override bool SupportsEditing => true;

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new NumericCellEditor();
        }
    }
}
