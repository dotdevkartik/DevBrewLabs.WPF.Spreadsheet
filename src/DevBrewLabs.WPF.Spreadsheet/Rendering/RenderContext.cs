using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI;
using System;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RenderContext : IDisposable
    {
        private bool _disposed;

        private DrawingContext _drawingContext;
        private SheetView _sheetView;
        private Rows _rows;
        private Columns _columns;
        private ColumnHeaders _columnHeaders;
        private ColumnHeaderRows _columnHeaderRows;
        private RowHeaders _rowHeaders;
        private RowHeaderColumns _rowHeaderColumns;
        private Worksheet _worksheet;
        private ViewPort _viewPort;
        private Pen _gridLinePen;
        private AutoFilter _filter;

        public double Zoom { get; }
        public double PixelPerDip { get; }
        public double TextPadding { get; }
        public bool SnapToPixels { get; }
        public double HalfPenWidth { get; }
        public int TopRow { get; }
        public int BottomRow { get; }
        public int LeftColumn { get; }
        public int RightColumn { get; }
        public Pen GridLinePen => _gridLinePen;
        public Rows Rows => _rows;
        public Columns Columns => _columns;
        public RowHeaders RowHeaders => _rowHeaders;
        public RowHeaderColumns RowHeaderColumns => _rowHeaderColumns;
        public ColumnHeaders ColumnHeaders => _columnHeaders;
        public ColumnHeaderRows ColumnHeaderRows => _columnHeaderRows;
        public Worksheet Worksheet => _worksheet;
        public SheetView SheetView => _sheetView;
        public ViewPort ViewPort => _viewPort;
        public AutoFilter AutoFilter => _filter;

        public RenderContext(DrawingContext context, SheetView view, double textPadding = 5, bool snapsToDevicePixels = true)
        {
            _drawingContext = context;
            _sheetView = view;
            _viewPort = _sheetView.ViewPort;
            TopRow = _viewPort.ViewRange.TopRow;
            BottomRow = _viewPort.ViewRange.BottomRow;
            LeftColumn = _viewPort.ViewRange.LeftColumn;
            RightColumn = _viewPort.ViewRange.RightColumn;
            _gridLinePen = _sheetView.Spread.GridLinePen;
            _worksheet = (Worksheet)_sheetView.WorkSheet;
            _rows = (Rows)_worksheet.Rows;
            _filter = _worksheet.AutoFilter;
            _columns = (Columns)_worksheet.Columns;
            _columnHeaders = (ColumnHeaders)_worksheet.ColumnHeaders;
            _columnHeaderRows = (ColumnHeaderRows)_columnHeaders.Rows;
            _rowHeaders = (RowHeaders)_worksheet.RowHeaders;
            _rowHeaderColumns = (RowHeaderColumns)_rowHeaders.Columns;
            Zoom = _sheetView.ZoomFactor > 0 ? _sheetView.ZoomFactor : 1.0;
            PixelPerDip = _sheetView.Spread.PixelPerDip;
            HalfPenWidth = _gridLinePen.Thickness * PixelPerDip / 2;
            TextPadding = textPadding;
            SnapToPixels = snapsToDevicePixels; 
        }

        public void DrawRectangle(DrawingColor? color, DrawingPen? pen, Rect rect)
        {
            if (_disposed) return;

            _drawingContext.DrawRectangle(
                color.HasValue ? WpfResourceCache.GetBrush(color.Value) : null, 
                pen.HasValue ? WpfResourceCache.GetPen(pen.Value) : null,
                rect);
        }

        public void DrawRectangle(Brush color, Pen pen, Rect rect)
        {
            if (_disposed) return;

            _drawingContext.DrawRectangle(color, pen, rect);
        }

        public void DrawGlyphRun(DrawingColor color, GlyphRun glyphRun)
        {
            if (_disposed) return;

            _drawingContext.DrawGlyphRun(WpfResourceCache.GetBrush(color), glyphRun);
        }

        public void DrawLine(DrawingPen pen, Point point0, Point point1)
        {
            if (_disposed) return;

            _drawingContext.DrawLine(WpfResourceCache.GetPen(pen), point0, point1); 
        }

        public void DrawLine(Pen gridLinePen, Point point1, Point point2)
        {
            if(_disposed) return;

            _drawingContext.DrawLine(gridLinePen, point1, point2);
        }

        public void DrawGeometry(DrawingColor? color, DrawingPen? pen, Geometry geometry)
        {
            if (_disposed) return;

            _drawingContext.DrawGeometry(
                 color.HasValue ? WpfResourceCache.GetBrush(color.Value) : null,
                 pen.HasValue ? WpfResourceCache.GetPen(pen.Value) : null,
                 geometry);
        }

        public void Pop()
        {
            if (_disposed) return;

            _drawingContext.Pop();
        }

        public void PushGuidelineSet(GuidelineSet guidelines)
        {
            if (_disposed) return;

            _drawingContext.PushGuidelineSet(guidelines);
        }

        public void Dispose()
        {
            _drawingContext?.Close();
            _drawingContext = null;
            _columns = null;
            _rows = null;
            _columnHeaderRows = null;
            _columnHeaders = null;
            _rowHeaderColumns = null;
            _rowHeaders = null;
            _gridLinePen = null;
            _sheetView = null;
            _worksheet = null;
            _disposed = true;
            _viewPort = null;
            _filter = null;
        }
    }
}
