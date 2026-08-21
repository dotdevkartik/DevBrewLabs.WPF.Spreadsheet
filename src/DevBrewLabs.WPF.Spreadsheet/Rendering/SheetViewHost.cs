using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class SheetViewHost : Grid, IDisposable
    {
        private Spread _spread;
        private SheetView _sheetView;

        public SheetViewHost(Spread spread)
        {
            _spread = spread;
            InitPaneLayout();
        }
  
        public void HostSheet(SheetView sheetView)
        {
            _sheetView = sheetView;

            Children.Clear();

            Children.Add(sheetView.CellsSurface);
            SetRow(sheetView.CellsSurface, 1);
            SetColumn(sheetView.CellsSurface, 1);

            Children.Add(sheetView.RowHeadersSurface);
            SetRow(sheetView.RowHeadersSurface, 1);
            SetColumn(sheetView.RowHeadersSurface, 0);

            Children.Add(sheetView.ColumnHeadersSurface);
            SetRow(sheetView.ColumnHeadersSurface, 0);
            SetColumn(sheetView.ColumnHeadersSurface, 1);

            Children.Add(sheetView.TopLeftSurface);
            SetRow(sheetView.TopLeftSurface, 0);
            SetColumn(sheetView.TopLeftSurface, 0);

            var style = _spread.WorkBook.GetNamedStyle(StyleKeys.DefaultSheetStyleKey);
            sheetView.CellsSurface.Background = style != null ? Styling.WpfResourceCache.GetBrush(style.BackColor) : null;
            UpdateLayout();
            UpdateZoomTransform();
        }

        public void UpdateZoomTransform()
        {
            Width = double.NaN;
            Height = double.NaN;

            if (_sheetView != null)
            {
                UpdateHeadersSize();
                _sheetView.ViewPort.RefreshBounds();
                if (_spread?.EditingManager?.IsEditing == true)
                {
                    _spread.EditingManager.UpdateEditorLayout();
                }
                _spread.Invalidate();
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
                    _spread.RenderEngine.DrawColumnHeaderCells(_sheetView, viewRange.LeftColumn, viewRange.RightColumn);
                    _spread.RenderEngine.DrawColumnHeaderGridLines(_sheetView, viewRange.LeftColumn, viewRange.RightColumn);
                }

                if (rowHeaders)
                {
                    _spread.RenderEngine.DrawRowHeaderCells(_sheetView, viewRange.TopRow, viewRange.BottomRow);
                    _spread.RenderEngine.DrawRowHeaderGridLines(_sheetView, viewRange.TopRow, viewRange.BottomRow);
                }

                if (topLeft)
                    _spread.RenderEngine.DrawTopLeft(_sheetView);

                if (cells)
                    _spread.RenderEngine.DrawCellRange(_sheetView, viewRange.TopRow, viewRange.LeftColumn, viewRange.BottomRow, viewRange.RightColumn);

                if (gridLines)
                    _spread.RenderEngine.DrawGridLines(_sheetView, viewRange.TopRow, viewRange.LeftColumn, viewRange.BottomRow, viewRange.RightColumn);
            }
            finally
            {
                _spread.RenderEngine.EndRender();
            }
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
            if(_sheetView == null)
            {
                return;
            }

            if (rowHeaders)
            {
                var rowHeadersInteractionLayer = _sheetView.RowHeadersSurface.GetInteractionLayer();

                if (rowHeadersInteractionLayer != null && rowHeadersInteractionLayer.IsLoaded)
                    rowHeadersInteractionLayer.InvalidateVisual();
            }

            if (columnHeaders)
            {
                var columnHeadersInteractionLayer = _sheetView.ColumnHeadersSurface.GetInteractionLayer();

                if (columnHeadersInteractionLayer != null && columnHeadersInteractionLayer.IsLoaded)
                    columnHeadersInteractionLayer.InvalidateVisual();
            }

            if (cells)
            {
                var cellsInteractionLayer = _sheetView.CellsSurface.GetInteractionLayer() as CellsInteractionLayer;

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
                _sheetView.ViewPort.CalculateVisibleRange();
                _spread.Invalidate();
            }
        }

        public void Dispose()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            Children.Clear();
        }
    }
}


