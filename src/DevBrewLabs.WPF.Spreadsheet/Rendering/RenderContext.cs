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
    public interface IRenderContext
    {
        double ZoomFactor { get; }
        ISheetView SheetView { get; }

        void DrawGeometry(DrawingColor? color, DrawingPen? pen, Geometry geometry);
        void DrawGeometry(Brush brush, Pen pen, Geometry geometry);
        void DrawGlyphRun(DrawingColor color, GlyphRun glyphRun);
        void DrawGlyphRun(Brush brush, GlyphRun glyphRun);
        void DrawLine(DrawingPen pen, Point point0, Point point1);
        void DrawLine(Pen gridLinePen, Point point1, Point point2);
        void DrawRectangle(DrawingColor? color, DrawingPen? pen, Rect rect);
        void DrawRectangle(Brush color, Pen pen, Rect rect);
        void DrawRoundedRectangle(Brush brush, Pen pen, Rect rect, int radiusX, int radiusY);
        void DrawText(string text, Rect bounds, DrawingFontFamily fontFamily, double fontSize, DrawingFontWeight fontWeight, DrawingFontStyle fontStyle, DrawingColor foreColor, CellHorizontalAlignment horizontalAlignment = CellHorizontalAlignment.Left, CellVerticalAlignment verticalAlignment = CellVerticalAlignment.Bottom, CellTextTrimming textTrimming = CellTextTrimming.None, bool allowMultiLineText = false);
        Rect GetCellRect(int row, int col);
        void Pop();
        void PushClip(Geometry clipGeometry);
        void PushOpacity(double opacity);
        void PushTransform(Transform transform);
        double Snap(double value);
        Point Snap(Point point);
        Rect Snap(Rect rect);
    }

    internal class RenderContext : IRenderContext, IDisposable
    {
        private bool _disposed;
        private DrawingContext _drawingContext;
        private bool _isOwnDrawingContext;

        public ISheetView SheetView => View;
        public ViewPort ViewPort { get; }
        public IWorksheet Worksheet { get; }
        public IRows Rows { get; }
        public IColumns Columns { get; }
        public IColumnHeaders ColumnHeaders { get; }
        public IRows ColumnHeaderRows { get; }
        public IRowHeaders RowHeaders { get; }
        public IColumns RowHeaderColumns { get; }
        public AutoFilter AutoFilter { get; }
        internal SheetView View { get; }
        internal HeaderHoverManager HeaderHoverManager { get; }
        internal Pen GridLinePen { get; }
        internal Brush SelectedHeaderBackground { get; }
        internal Brush SelectedHeaderForeground { get; }
        internal Brush RangeSelectedHeaderBackground { get; }
        public CellRange Selection => View != null ? View.Selection : default;
        public GridLineVisibility GridLineVisibility => View != null ? View.GridLineVisibility : GridLineVisibility.Both;
        public double RowHeaderWidth => View != null ? View.GetRowHeaderWidth() * ZoomFactor : 0;
        public double ColumnHeaderHeight => View != null ? View.GetColumnHeaderHeight() * ZoomFactor : 0;

        public double ZoomFactor { get; }
        public double PixelPerDip { get; }
        public double TextPadding { get; }

        public RenderContext(DrawingContext context, SheetView view, double textPadding = 5)
        {
            _drawingContext = context;
            View = view;
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

            ZoomFactor = view.ZoomFactor > 0 ? view.ZoomFactor : 1.0;
            TextPadding = textPadding;
        }

        public RenderContext(DrawingGroup drawing, SheetView view, double textPadding = 5) : this(drawing.Open(), view, textPadding)
        {
            if (drawing == null)
                throw new ArgumentNullException(nameof(drawing));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _isOwnDrawingContext = true;
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
            return ViewPort != null ? (modelX - ViewPort.LeftColumnLocation) * ZoomFactor : modelX * ZoomFactor;
        }

        public double GetScreenY(double modelY)
        {
            return ViewPort != null ? (modelY - ViewPort.TopRowLocation) * ZoomFactor : modelY * ZoomFactor;
        }

        public double GetColumnScreenLocation(int col)
        {
            return ViewPort != null ? (ViewPort.GetColumnLocation(col) - ViewPort.LeftColumnLocation) * ZoomFactor : 0;
        }

        public double GetRowScreenLocation(int row)
        {
            return ViewPort != null ? (ViewPort.GetRowLocation(row) - ViewPort.TopRowLocation) * ZoomFactor : 0;
        }

        public double GetHeaderColumnScreenLocation(int col)
        {
            return ViewPort != null ? ViewPort.GetHeaderColumnLocation(col) * ZoomFactor : 0;
        }

        public double GetHeaderRowScreenLocation(int row)
        {
            return ViewPort != null ? ViewPort.GetHeaderRowLocation(row) * ZoomFactor : 0;
        }

        public Rect GetCellRect(int row, int col)
        {
            if (ViewPort == null) return Rect.Empty;
            var unzoomed = ViewPort.GetCellRect(row, col);
            double x = (unzoomed.X - ViewPort.LeftColumnLocation) * ZoomFactor;
            double y = (unzoomed.Y - ViewPort.TopRowLocation) * ZoomFactor;
            double width = unzoomed.Width * ZoomFactor;
            double height = unzoomed.Height * ZoomFactor;
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

        public void Pop()
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.Pop();
        }

        public void DrawRoundedRectangle(Brush brush, Pen pen, Rect rect, int radiusX, int radiusY)
        {
            if (_disposed || _drawingContext == null) return;

            _drawingContext.DrawRoundedRectangle(brush, pen, rect, radiusX, radiusY);
        }


        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_isOwnDrawingContext)
                {
                    _drawingContext?.Close();
                }
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
