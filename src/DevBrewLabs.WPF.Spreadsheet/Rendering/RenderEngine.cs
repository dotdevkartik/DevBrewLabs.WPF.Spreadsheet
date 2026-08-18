using DevBrewLabs.Spreadsheet;
using System;
using System.Windows.Threading;
using System.Windows.Media;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Renderers;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class RenderEngine : IRenderEngine, IDisposable
    {
        private SheetView _sheetView;
        private Worksheet _workSheet;
        private DispatcherProcessingDisabled _dispatcherDisabled;

        #region Renderers
        internal Renderer GridLinesRenderer { get; }
        internal Renderer CellsRenderer { get; }
        internal Renderer RowHeadersRenderer { get; }
        internal Renderer ColumnHeadersRenderer { get; }
        internal Renderer RowHeaderGridLinesRenderer { get; }
        internal Renderer ColumnHeaderGridLinesRenderer { get; }
        internal Renderer TopLeftRenderer { get; }
        #endregion

        public RenderEngine()
        {
            CellsRenderer = new CellsRenderer();
            GridLinesRenderer = new GridLinesRenderer();
            RowHeadersRenderer = new RowHeadersRenderer();
            ColumnHeadersRenderer = new ColumnHeadersRenderer();
            RowHeaderGridLinesRenderer = new RowHeaderGridLinesRenderer();
            ColumnHeaderGridLinesRenderer = new ColumnHeaderGridLinesRenderer();
            TopLeftRenderer = new TopLeftRenderer();
        }

        public void SetRenderSheet(SheetView sheetView)
        {
            _sheetView = sheetView;
            _workSheet = (Worksheet)sheetView.WorkSheet;
            CellsRenderer.SetRenderSheet(sheetView);
            GridLinesRenderer.SetRenderSheet(sheetView);
            RowHeadersRenderer.SetRenderSheet(sheetView);
            ColumnHeadersRenderer.SetRenderSheet(sheetView);
            RowHeaderGridLinesRenderer.SetRenderSheet(sheetView);
            ColumnHeaderGridLinesRenderer.SetRenderSheet(sheetView);
            TopLeftRenderer.SetRenderSheet(sheetView);
        }

        #region Render Begin/End
        public void BeginRender()
        {
            InitRender();
        }

        private void InitRender()
        {
            _dispatcherDisabled = Dispatcher.CurrentDispatcher.DisableProcessing();
        }

        public void EndRender()
        {
            CellsRenderer.EndRender();
            GridLinesRenderer.EndRender();
            RowHeadersRenderer.EndRender();
            ColumnHeadersRenderer.EndRender();
            RowHeaderGridLinesRenderer.EndRender();
            ColumnHeaderGridLinesRenderer.EndRender();
            TopLeftRenderer.EndRender();
            _dispatcherDisabled.Dispose();
        }
        #endregion

        public void DrawGridLines(int topRow, int leftCol, int bottomRow, int rightCol)
        {
            GridLinesRenderer.Render(topRow, leftCol, bottomRow, rightCol);
        }

        public void DrawCellRange(int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            CellsRenderer.Render(topRow, leftColumn, bottomRow, rightColumn);
        }

        public void DrawRowHeaderCells(int topRow, int bottomRow)
        {
            if (_sheetView.HeadersVisibility == HeadersVisibility.Row || _sheetView.HeadersVisibility == HeadersVisibility.Both)
            {
                RowHeadersRenderer.Render(topRow, 0, bottomRow, _workSheet.RowHeaders.ColumnCount - 1);
            }
        }

        public void DrawRowHeaderGridLines(int topRow, int bottomRow)
        {
            if (_sheetView.HeadersVisibility == HeadersVisibility.Row || _sheetView.HeadersVisibility == HeadersVisibility.Both)
            {
                RowHeaderGridLinesRenderer.Render(topRow, 0, bottomRow, _workSheet.RowHeaders.ColumnCount - 1);
            }
        }

        public void DrawColumnHeaderCells(int leftCol, int rightCol)
        {
            if (_sheetView.HeadersVisibility == HeadersVisibility.Column || _sheetView.HeadersVisibility == HeadersVisibility.Both)
            {
                ColumnHeadersRenderer.Render(0, leftCol, _workSheet.ColumnHeaders.RowCount - 1, rightCol);
            }
        }

        public void DrawColumnHeaderGridLines(int leftCol, int rightCol)
        {
            if (_sheetView.HeadersVisibility == HeadersVisibility.Column || _sheetView.HeadersVisibility == HeadersVisibility.Both)
            {
                ColumnHeaderGridLinesRenderer.Render(0, leftCol, _workSheet.ColumnHeaders.RowCount - 1, rightCol);
            }
        }

        public void DrawTopLeft()
        {
            if (_sheetView.HeadersVisibility == HeadersVisibility.Both)
            {
                TopLeftRenderer.Render(-1, -1, -1, -1);
            }
        }

        public void Dispose()
        {
            _workSheet = null;
            _sheetView = null;
        }
    }
}
