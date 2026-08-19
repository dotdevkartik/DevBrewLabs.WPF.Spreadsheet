using DevBrewLabs.WPF.Spreadsheet.Rendering.Renderers;
using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RenderEngine : IDisposable
    {
        private RendererBase _gridLinesRenderer;
        private RendererBase _cellsRenderer;
        private RendererBase _rowHeadersRenderer;
        private RendererBase _rowHeaderGridLinesRenderer;
        private RendererBase _columnHeadersRenderer;
        private RendererBase _columnHeaderGridLinesRenderer;
        private RendererBase _topLeftRenderer;

        private DispatcherProcessingDisabled _dispatcherDisabled;

        public RenderEngine()
        {
            _cellsRenderer = new CellsRenderer();
            _gridLinesRenderer = new GridLinesRenderer();
            _rowHeadersRenderer = new RowHeadersRenderer();
            _columnHeadersRenderer = new ColumnHeadersRenderer();
            _rowHeaderGridLinesRenderer = new RowHeaderGridLinesRenderer();
            _columnHeaderGridLinesRenderer = new ColumnHeaderGridLinesRenderer();
            _topLeftRenderer = new TopLeftRenderer();
        }

        public void BeginRender()
        {
            InitRender();
        }

        private void InitRender()
        {
            _dispatcherDisabled = Dispatcher.CurrentDispatcher.DisableProcessing();
        }

        public void DrawGridLines(SheetView view, int topRow, int leftCol, int bottomRow, int rightCol)
        {
            DrawingGroup group = view.CellsSurface.GetDrawing();
            DrawingGroup gridLinesDrawing = group.Children[1] as DrawingGroup;
            gridLinesDrawing.ClipGeometry = null;
            using (RenderContext context = new RenderContext(gridLinesDrawing.Open(), view))
            {
                _gridLinesRenderer.OnRender(context, topRow, leftCol, bottomRow, rightCol);
            }
        }

        public void DrawCellRange(SheetView view, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            DrawingGroup group = view.CellsSurface.GetDrawing();
            DrawingGroup cellsDrawing = group.Children[0] as DrawingGroup;
            cellsDrawing.ClipGeometry = null;
            using (RenderContext context = new RenderContext(cellsDrawing.Open(), view))
            {
                _cellsRenderer.OnRender(context, topRow, leftColumn, bottomRow, rightColumn);
            }
        }

        public void DrawRowHeaderCells(SheetView view, int topRow, int bottomRow)
        {
            DrawingGroup group = view.RowHeadersSurface.GetDrawing();
            DrawingGroup rowHeadersDrawing = group.Children[0] as DrawingGroup;
            rowHeadersDrawing.ClipGeometry = null;
            using (RenderContext context = new RenderContext(rowHeadersDrawing.Open(), view))
            {
                _rowHeadersRenderer.OnRender(context, topRow, 0, bottomRow, view.WorkSheet.RowHeaders.ColumnCount - 1);
            }
        }

        public void DrawRowHeaderGridLines(SheetView view, int topRow, int bottomRow)
        {
            DrawingGroup group = view.RowHeadersSurface.GetDrawing();
            DrawingGroup gridLinesDrawing = group.Children[1] as DrawingGroup;
            gridLinesDrawing.ClipGeometry = null;
            using (RenderContext context = new RenderContext(gridLinesDrawing.Open(), view))
            {
                _rowHeaderGridLinesRenderer.OnRender(context, topRow, 0, bottomRow, view.WorkSheet.RowHeaders.ColumnCount - 1);
            }
        }

        public void DrawColumnHeaderCells(SheetView view, int leftCol, int rightCol)
        {
            DrawingGroup group = view.ColumnHeadersSurface.GetDrawing();
            DrawingGroup columnHeaderDrawing = group.Children[0] as DrawingGroup;
            columnHeaderDrawing.ClipGeometry = null;
            using (RenderContext context = new RenderContext(columnHeaderDrawing.Open(), view))
            {
                _columnHeadersRenderer.OnRender(context, 0, leftCol, view.WorkSheet.ColumnHeaders.RowCount - 1, rightCol);
            }
        }

        public void DrawColumnHeaderGridLines(SheetView view, int leftCol, int rightCol)
        {
            DrawingGroup group = view.ColumnHeadersSurface.GetDrawing();
            DrawingGroup gridLineDrawing = group.Children[1] as DrawingGroup;
            gridLineDrawing.ClipGeometry = null;
            using (RenderContext context = new RenderContext(gridLineDrawing.Open(), view))
            {
                _columnHeaderGridLinesRenderer.OnRender(context, 0, leftCol, view.WorkSheet.ColumnHeaders.RowCount - 1, rightCol);
            }
        }

        public void DrawTopLeft(SheetView view)
        {
            DrawingGroup drawing = view.TopLeftSurface.GetDrawing();
            using (RenderContext context = new RenderContext(drawing.Open(), view))
            {
                _topLeftRenderer.OnRender(context, -1, -1, -1, -1);
            }
        }

        public void EndRender()
        {
            _dispatcherDisabled.Dispose();
        }

        public void Dispose()
        {
            _cellsRenderer = null;
            _gridLinesRenderer = null;
            _columnHeaderGridLinesRenderer = null;
            _columnHeadersRenderer = null;
            _rowHeaderGridLinesRenderer = null;
            _rowHeadersRenderer = null;
            _topLeftRenderer = null;
        }
    }
}
