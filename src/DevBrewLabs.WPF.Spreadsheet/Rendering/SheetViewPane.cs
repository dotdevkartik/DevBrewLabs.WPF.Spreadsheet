using DevBrewLabs.Spreadsheet;

using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class SheetViewPane : Grid, IDisposable
    {
        private Spread _spread;
        private SheetView _sheetView;
        private WorkSheet _workSheet;
        private CellsInteractionLayer _cellInteractionLayer;
        private RowHeadersInteractionLayer _rowHeadersInteractionLayer;
        private ColumnHeadersInteractionLayer _columnHeadersInteractionLayer;
        private TopLeftInteractionLayer _topLeftInteractionLayer;

        public CellsSurface CellsRegion { get; private set; }
        public RowHeadersSurface RowHeadersRegion { get; private set; }
        public ColumnHeadersSurface ColumnHeadersRegion { get; private set; }
        public TopLeftSurface TopLeftRegion { get; private set; }

        public SheetViewPane(Spread spread)
        {
            _spread = spread;
            InitPaneLayout();
            InitRegions();
            InitInteractionLayers();
        }
  
        public void AttachSheet(SheetView sheetView)
        {
            _sheetView = sheetView;
            _workSheet = (WorkSheet)sheetView.WorkSheet;

            CellsRegion.AttachSheet(sheetView);
            RowHeadersRegion.AttachSheet(sheetView);
            ColumnHeadersRegion.AttachSheet(sheetView);
            TopLeftRegion.AttachSheet(sheetView);

            _cellInteractionLayer.EnsureFree();
            _columnHeadersInteractionLayer.EnsureFree();
            _rowHeadersInteractionLayer.EnsureFree();
            _topLeftInteractionLayer.EnsureFree();

            _cellInteractionLayer.AttachToRegion(CellsRegion);
            _columnHeadersInteractionLayer.AttachToRegion(ColumnHeadersRegion);
            _rowHeadersInteractionLayer.AttachToRegion(RowHeadersRegion);
            _topLeftInteractionLayer.AttachToRegion(TopLeftRegion);

            var style = _spread.WorkBook.GetNamedStyle(StyleKeys.DefaultSheetStyleKey);
            CellsRegion.Background = style != null ? Styling.WpfResourceCache.GetBrush(style.BackColor) : null;

            _spread.RenderEngine.SetRenderSheet(sheetView);
            UpdateZoomTransform();
        }

        public void UpdateZoomTransform()
        {
            Width = double.NaN;
            Height = double.NaN;

            if (_sheetView != null)
            {
                UpdateHeadersSize();
                _sheetView.ViewPort?.As<UI.ViewPort>()?.RefreshBounds();
                if (_spread?.EditingManager?.IsEditing == true)
                {
                    _spread.EditingManager.UpdateEditorLayout();
                }
                _spread.Invalidate();
            }
        }

        public void DrawRange(int topRow, int leftCol, int bottomRow, int rightCol)
        {
            if (_sheetView == null)
                return;

            var viewRange = _sheetView.ViewPort.ViewRange;

            if (!viewRange.IsValid)
                return;

            _spread.RenderEngine.BeginRender();
            try
            {
                _spread.RenderEngine.DrawCellRange(topRow, leftCol, bottomRow, rightCol);
            }
            finally
            {
                _spread.RenderEngine.EndRender();
            }
        }

        /// <summary>
        /// Draws the sheet using render engine.
        /// </summary>
        /// <param name="redraw"></param>
        /// <param name="rowHeaders"></param>
        /// <param name="columnHeaders"></param>
        /// <param name="cells"></param>
        /// <param name="gridLines"></param>
        /// <param name="topLeft"></param>
        public void Draw(bool rowHeaders = true, bool columnHeaders = true, bool cells = true, bool gridLines = true, bool topLeft = true)
        {
            if (_sheetView == null)
                return;

            var viewRange = _sheetView.ViewPort.ViewRange;

            if (!viewRange.IsValid)
                return;

            _spread.RenderEngine.BeginRender();
            try
            {
                if (columnHeaders)
                {
                    _spread.RenderEngine.DrawColumnHeaderCells(viewRange.LeftColumn, viewRange.RightColumn);
                    _spread.RenderEngine.DrawColumnHeaderGridLines(viewRange.LeftColumn, viewRange.RightColumn);
                }

                if (rowHeaders)
                {
                    _spread.RenderEngine.DrawRowHeaderCells(viewRange.TopRow, viewRange.BottomRow);
                    _spread.RenderEngine.DrawRowHeaderGridLines(viewRange.TopRow, viewRange.BottomRow);
                }

                if (topLeft)
                    _spread.RenderEngine.DrawTopLeft();

                if (cells)
                    _spread.RenderEngine.DrawCellRange(viewRange.TopRow, viewRange.LeftColumn, viewRange.BottomRow, viewRange.RightColumn);

                if (gridLines)
                    _spread.RenderEngine.DrawGridLines(viewRange.TopRow, viewRange.LeftColumn, viewRange.BottomRow, viewRange.RightColumn);
            }
            finally
            {
                _spread.RenderEngine.EndRender();
            }
        }

        private void InitInteractionLayers()
        {
            _cellInteractionLayer = new CellsInteractionLayer();
            _rowHeadersInteractionLayer = new RowHeadersInteractionLayer();
            _columnHeadersInteractionLayer = new ColumnHeadersInteractionLayer();
            _topLeftInteractionLayer = new TopLeftInteractionLayer();
        }

        /// <summary>
        /// Initializes sheet render regions.
        /// </summary>
        private void InitRegions()
        {
            CellsRegion = new CellsSurface();
            Children.Add(CellsRegion);
            SetRow(CellsRegion, 1);
            SetColumn(CellsRegion, 1);

            RowHeadersRegion = new RowHeadersSurface();
            Children.Add(RowHeadersRegion);
            SetRow(RowHeadersRegion, 1);
            SetColumn(RowHeadersRegion, 0);

            ColumnHeadersRegion = new ColumnHeadersSurface();
            Children.Add(ColumnHeadersRegion);
            SetRow(ColumnHeadersRegion, 0);
            SetColumn(ColumnHeadersRegion, 1);

            TopLeftRegion = new TopLeftSurface();
            Children.Add(TopLeftRegion);
            SetRow(TopLeftRegion, 0);
            SetColumn(TopLeftRegion, 0);
        }

        /// <summary>
        /// Initializes render pane layout.
        /// </summary>
        private void InitPaneLayout()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(0, GridUnitType.Auto),
            });
            RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(1, GridUnitType.Star)
            });
            ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(0, GridUnitType.Auto)
            });
            ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
        }

        public void UpdateHeadersSize()
        {
            if (_sheetView == null)
                return;

            double zoom = _sheetView.ZoomFactor > 0 ? _sheetView.ZoomFactor : 1.0;

            switch (_sheetView.HeadersVisibility)
            {
                case HeadersVisibility.Both:
                    ColumnDefinitions[0].Width = new GridLength(_sheetView.GetRowHeaderWidth() * zoom);
                    RowDefinitions[0].Height = new GridLength(_sheetView.GetColumnHeaderHeight() * zoom);
                    break;

                case HeadersVisibility.Column:
                    ColumnDefinitions[0].Width = new GridLength(0);
                    RowDefinitions[0].Height = new GridLength(_sheetView.GetColumnHeaderHeight() * zoom);
                    break;

                case HeadersVisibility.Row:
                    ColumnDefinitions[0].Width = new GridLength(_sheetView.GetRowHeaderWidth() * zoom);
                    RowDefinitions[0].Height = new GridLength(0);
                    break;

                case HeadersVisibility.None:
                    ColumnDefinitions[0].Width = new GridLength(0);
                    RowDefinitions[0].Height = new GridLength(0);
                    break;
            }
        }

        public void RefreshInteractionLayers(bool rowHeaders = true, bool columnHeaders = true, bool cells = true)
        {
            if (rowHeaders)
            {
                var rowHeadersInteractionLayer = RowHeadersRegion.GetInteractionLayer();

                if (rowHeadersInteractionLayer != null && rowHeadersInteractionLayer.IsLoaded)
                    rowHeadersInteractionLayer.InvalidateVisual();
            }

            if (columnHeaders)
            {
                var columnHeadersInteractionLayer = ColumnHeadersRegion.GetInteractionLayer();

                if (columnHeadersInteractionLayer != null && columnHeadersInteractionLayer.IsLoaded)
                    columnHeadersInteractionLayer.InvalidateVisual();
            }

            if (cells)
            {
                var cellsInteractionLayer = CellsRegion.GetInteractionLayer() as CellsInteractionLayer;

                if (cellsInteractionLayer != null && cellsInteractionLayer.IsLoaded)
                    cellsInteractionLayer.UpdateSelectionRects();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateHeadersSize();
            Clip = new RectangleGeometry(new Rect(new Point(-1, -1), sizeInfo.NewSize));

            if (_sheetView != null)
            {
                _sheetView.ViewPort?.As<UI.ViewPort>()?.CalculateVisibleRange();
                _spread.Invalidate();
            }
        }

        public void Dispose()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            _cellInteractionLayer.Children.Clear();
            _rowHeadersInteractionLayer.Children.Clear();
            _topLeftInteractionLayer.Children.Clear();
            _columnHeadersInteractionLayer.Children.Clear();
            CellsRegion.Children.Clear();
            RowHeadersRegion.Children.Clear();
            ColumnHeadersRegion.Children.Clear();
            TopLeftRegion.Children.Clear();
            Children.Clear();
            _cellInteractionLayer = null;
            _columnHeadersInteractionLayer = null;
            _rowHeadersInteractionLayer = null;
            _topLeftInteractionLayer = null;
            CellsRegion = null;
            RowHeadersRegion = null;
            ColumnHeadersRegion = null;
            TopLeftRegion = null;
        }
    }
}


