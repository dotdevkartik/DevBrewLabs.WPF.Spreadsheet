using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RenderContext : IDisposable
    {
        private bool _disposed;
        private DrawingContext _drawingContext;

        public SheetView SheetView { get; }
        public ViewPort ViewPort { get; }
        public IWorksheet Worksheet { get; }
        public IRows Rows { get; }
        public IColumns Columns { get; }
        public IColumnHeaders ColumnHeaders { get; }
        public IRows ColumnHeaderRows { get; }
        public IRowHeaders RowHeaders { get; }
        public IColumns RowHeaderColumns { get; }
        public AutoFilter AutoFilter { get; }
        public HeaderHoverManager HeaderHoverManager { get; }
        public Pen GridLinePen { get; }
        public Brush SelectedHeaderBackground { get; }
        public Brush SelectedHeaderForeground { get; }
        public Brush RangeSelectedHeaderBackground { get; }
        public CellRange Selection => SheetView != null ? SheetView.Selection : default;
        public GridLineVisibility GridLineVisibility => SheetView != null ? SheetView.GridLineVisibility : GridLineVisibility.Both;
        public double RowHeaderWidth => SheetView != null ? SheetView.GetRowHeaderWidth() * Zoom : 0;
        public double ColumnHeaderHeight => SheetView != null ? SheetView.GetColumnHeaderHeight() * Zoom : 0;

        public double Zoom { get; }
        public double PixelPerDip { get; }
        public double TextPadding { get; }
        public double HalfPenWidth { get; }

        public RenderContext(DrawingGroup drawing, SheetView view, double textPadding = 5)
        {
            if (drawing == null)
                throw new ArgumentNullException(nameof(drawing));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _drawingContext = drawing.Open();
            SheetView = view;
            ViewPort = view.ViewPort;
            Worksheet = view.WorkSheet;

            if (Worksheet != null)
            {
                Rows = Worksheet.Rows;
                Columns = Worksheet.Columns;
                ColumnHeaders = Worksheet.ColumnHeaders;
                ColumnHeaderRows = ColumnHeaders?.Rows;
                RowHeaders = Worksheet.RowHeaders;
                RowHeaderColumns = RowHeaders?.Columns;
                AutoFilter = Worksheet.AutoFilter;
            }

            var spread = view.Spread;
            if (spread != null)
            {
                GridLinePen = spread.GridLinePen;
                HeaderHoverManager = spread.HeaderHoverManager;
                SelectedHeaderBackground = spread.SelectedHeaderBackground;
                SelectedHeaderForeground = spread.SelectedHeaderForeground;
                RangeSelectedHeaderBackground = spread.RangeSelectedHeaderBackground;
                PixelPerDip = spread.PixelPerDip;
            }
            else
            {
                PixelPerDip = 1.0;
            }

            Zoom = view.ZoomFactor > 0 ? view.ZoomFactor : 1.0;
            HalfPenWidth = GridLinePen != null ? (GridLinePen.Thickness * PixelPerDip / 2.0) : 0.5;
            TextPadding = textPadding;
        }

        #region Coordinate & Snapping Helpers

        public double Snap(double value)
        {
            return PixelSnapper.Snap(value, PixelPerDip);
        }

        public Point Snap(Point point)
        {
            return new Point(PixelSnapper.Snap(point.X, PixelPerDip), PixelSnapper.Snap(point.Y, PixelPerDip));
        }

        public Rect Snap(Rect rect)
        {
            return new Rect(
                PixelSnapper.Snap(rect.X, PixelPerDip),
                PixelSnapper.Snap(rect.Y, PixelPerDip),
                PixelSnapper.Snap(rect.Width, PixelPerDip),
                PixelSnapper.Snap(rect.Height, PixelPerDip));
        }

        public double GetScreenX(double modelX)
        {
            return ViewPort != null ? (modelX - ViewPort.LeftColumnLocation) * Zoom : modelX * Zoom;
        }

        public double GetScreenY(double modelY)
        {
            return ViewPort != null ? (modelY - ViewPort.TopRowLocation) * Zoom : modelY * Zoom;
        }

        public double GetColumnScreenLocation(int col)
        {
            return ViewPort != null ? (ViewPort.GetColumnLocation(col) - ViewPort.LeftColumnLocation) * Zoom : 0;
        }

        public double GetRowScreenLocation(int row)
        {
            return ViewPort != null ? (ViewPort.GetRowLocation(row) - ViewPort.TopRowLocation) * Zoom : 0;
        }

        public double GetHeaderColumnScreenLocation(int col)
        {
            return ViewPort != null ? ViewPort.GetHeaderColumnLocation(col) * Zoom : 0;
        }

        public double GetHeaderRowScreenLocation(int row)
        {
            return ViewPort != null ? ViewPort.GetHeaderRowLocation(row) * Zoom : 0;
        }

        public Rect GetCellRect(int row, int col)
        {
            if (ViewPort == null) return Rect.Empty;
            var unzoomed = ViewPort.GetCellRect(row, col);
            double x = (unzoomed.X - ViewPort.LeftColumnLocation) * Zoom;
            double y = (unzoomed.Y - ViewPort.TopRowLocation) * Zoom;
            double width = unzoomed.Width * Zoom;
            double height = unzoomed.Height * Zoom;
            double penThickness = GridLinePen != null ? GridLinePen.Thickness : 1.0;
            return new Rect(x, y, Math.Max(0, width - penThickness), Math.Max(0, height - penThickness));
        }

        #endregion

        #region Drawing Commands

        public void DrawRectangle(DrawingColor? color, DrawingPen? pen, Rect rect)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawRectangle(
                color.HasValue ? WpfResourceCache.GetBrush(color.Value) : null, 
                pen.HasValue ? WpfResourceCache.GetPen(pen.Value) : null,
                rect);
        }

        public void DrawRectangle(Brush color, Pen pen, Rect rect)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawRectangle(color, pen, rect);
        }

        public void DrawGlyphRun(DrawingColor color, GlyphRun glyphRun)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawGlyphRun(WpfResourceCache.GetBrush(color), glyphRun);
        }

        public void DrawGlyphRun(Brush brush, GlyphRun glyphRun)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawGlyphRun(brush, glyphRun);
        }

        public void DrawText(
            string text,
            Rect bounds,
            DrawingFontFamily fontFamily,
            double fontSize,
            DrawingFontWeight fontWeight,
            DrawingFontStyle fontStyle,
            DrawingColor foreColor,
            CellHorizontalAlignment horizontalAlignment = CellHorizontalAlignment.Left,
            CellVerticalAlignment verticalAlignment = CellVerticalAlignment.Bottom,
            CellTextTrimming textTrimming = CellTextTrimming.None,
            bool allowMultiLineText = false)
        {
            if (_disposed || _drawingContext == null) return;

            TextRenderer.DrawText(
                this,
                text,
                bounds,
                fontFamily,
                fontSize,
                fontWeight,
                fontStyle,
                foreColor,
                horizontalAlignment,
                verticalAlignment,
                textTrimming,
                allowMultiLineText);
        }

        public void DrawLine(DrawingPen pen, Point point0, Point point1)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawLine(WpfResourceCache.GetPen(pen), point0, point1); 
        }

        public void DrawLine(Pen gridLinePen, Point point1, Point point2)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawLine(gridLinePen, point1, point2);
        }

        public void DrawGeometry(DrawingColor? color, DrawingPen? pen, Geometry geometry)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawGeometry(
                 color.HasValue ? WpfResourceCache.GetBrush(color.Value) : null,
                 pen.HasValue ? WpfResourceCache.GetPen(pen.Value) : null,
                 geometry);
        }

        public void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawGeometry(brush, pen, geometry);
        }

        public void PushClip(Geometry clipGeometry)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.PushClip(clipGeometry);
        }

        public void PushTransform(Transform transform)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.PushTransform(transform);
        }

        public void PushOpacity(double opacity)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.PushOpacity(opacity);
        }

        public void PushGuidelineSet(GuidelineSet guidelines)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.PushGuidelineSet(guidelines);
        }

        public void Pop()
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.Pop();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _drawingContext?.Close();
            }
            catch
            {
                // Suppress secondary exception to prevent masking primary exception during stack unwinding
            }
            finally
            {
                _drawingContext = null;
            }
        }

        #endregion
    }
}
