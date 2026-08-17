using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Components;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet
{
    /// <summary>
    /// Represents view for a workbook.
    /// </summary>
    public partial class Spread : Control, IDisposable
    {
        private ZoomManager _zoomManager;
        private ClipboardManager _clipboardManager;
        private SelectionManager _selectionManager;
        private EditingManager _editingManager;
        private RenderEngine _renderEngine;
        private SheetViewPane _sheetViewPane;
        private SheetTabControl _sheetTabControl;
        private UndoRedoManager _undoRedoManager;
        private WorkBook _workBook;
        private WorksheetChangeListener _changeListener;

        #region Dependency Properties
        public static readonly DependencyProperty ScrollBarStyleProperty;
        public static readonly DependencyProperty ScrollModeProperty;
        public static readonly DependencyProperty SelectionBackgroundProperty;
        public static readonly DependencyProperty GridLineBrushProperty;
        public static readonly DependencyProperty SelectionBorderBrushProperty;
        public static readonly DependencyProperty AllowRowResizeProperty;
        public static readonly DependencyProperty AllowColumnResizeProperty;
        public static readonly DependencyProperty ShowTabStripProperty;
        public static readonly DependencyProperty ShowAddNewSheetProperty;
        public static readonly DependencyProperty IsSelectionAnimationEnabledProperty;
        public static readonly DependencyProperty ShowFormulaSuggestionsProperty;
        public static readonly DependencyProperty ZoomFactorProperty;
        public static readonly DependencyProperty AllowZoomingProperty;

        static Spread()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spread), new FrameworkPropertyMetadata(typeof(Spread)));

            ZoomFactorProperty = DependencyProperty.Register(
                nameof(ZoomFactor),
                typeof(double),
                typeof(Spread),
                new FrameworkPropertyMetadata(
                    1.0,
                    OnZoomFactorChanged,
                    CoerceZoomFactor));

            AllowZoomingProperty = DependencyProperty.Register(
                nameof(AllowZooming),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            ScrollModeProperty = DependencyProperty.Register(
                nameof(ScrollMode),
                typeof(SheetScrollMode),
                typeof(Spread),
                new PropertyMetadata(SheetScrollMode.Item));

            SelectionBackgroundProperty = DependencyProperty.Register(
                nameof(SelectionBackground),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(
                    new SolidColorBrush(Color.FromArgb(50, 25, 25, 25))));

            GridLineBrushProperty = DependencyProperty.Register(
                nameof(GridLineBrush),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(OnGridLineBrushChanged));

            SelectionBorderBrushProperty = DependencyProperty.Register(
                nameof(SelectionBorderBrush),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(OnSelectionBorderBrushChanged));

            AllowRowResizeProperty = DependencyProperty.Register(
                nameof(AllowRowResize),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            AllowColumnResizeProperty = DependencyProperty.Register(
                nameof(AllowColumnResize),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            ShowFormulaSuggestionsProperty = DependencyProperty.Register(
                nameof(ShowFormulaSuggestions),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            IsSelectionAnimationEnabledProperty = DependencyProperty.Register(
                nameof(IsSelectionAnimationEnabled),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(false));

            ShowTabStripProperty = DependencyProperty.Register(
                nameof(ShowTabStrip),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            ShowAddNewSheetProperty = DependencyProperty.Register(
                nameof(ShowAddNewSheet),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            ScrollBarStyleProperty = DependencyProperty.Register(
                nameof(ScrollBarStyle),
                typeof(Style),
                typeof(Spread),
                new PropertyMetadata(null));
        }

        /// <summary>
        /// Gets or sets scrollbar style.
        /// </summary>
        public Style ScrollBarStyle
        {
            get { return (Style)GetValue(ScrollBarStyleProperty); }
            set { SetValue(ScrollBarStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the tab strip is visible.
        /// </summary>
        public bool ShowTabStrip
        {
            get { return (bool)GetValue(ShowTabStripProperty); }
            set { SetValue(ShowTabStripProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the add new sheet button is visible.
        /// </summary>
        public bool ShowAddNewSheet
        {
            get { return (bool)GetValue(ShowAddNewSheetProperty); }
            set { SetValue(ShowAddNewSheetProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the formula suggestion is enabled.
        /// </summary>
        public bool ShowFormulaSuggestions
        {
            get { return (bool)GetValue(ShowFormulaSuggestionsProperty); }
            set { SetValue(ShowFormulaSuggestionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the columns can be resized.
        /// </summary>
        public bool AllowColumnResize
        {
            get { return (bool)GetValue(AllowColumnResizeProperty); }
            set { SetValue(AllowColumnResizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the rows can be resized.
        /// </summary>
        public bool AllowRowResize
        {
            get { return (bool)GetValue(AllowRowResizeProperty); }
            set { SetValue(AllowRowResizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the selection travel animation is enabled.
        /// </summary>
        public bool IsSelectionAnimationEnabled
        {
            get { return (bool)GetValue(IsSelectionAnimationEnabledProperty); }
            set { SetValue(IsSelectionAnimationEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the scroll mode.
        /// </summary>
        public SheetScrollMode ScrollMode
        {
            get { return (SheetScrollMode)GetValue(ScrollModeProperty); }
            set { SetValue(ScrollModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selection background.
        /// </summary>
        public Brush SelectionBackground
        {
            get { return (Brush)GetValue(SelectionBackgroundProperty); }
            set { SetValue(SelectionBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the grid line brush.
        /// </summary>
        public Brush GridLineBrush
        {
            get { return (Brush)GetValue(GridLineBrushProperty); }
            set { SetValue(GridLineBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selection border brush.
        /// </summary>
        public Brush SelectionBorderBrush
        {
            get { return (Brush)GetValue(SelectionBorderBrushProperty); }
            set { SetValue(SelectionBorderBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the zoom factor for the active worksheet view. (1.0 = 100%).
        /// </summary>
        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether zooming via UI interactions (like mouse wheel) is allowed.
        /// </summary>
        public bool AllowZooming
        {
            get { return (bool)GetValue(AllowZoomingProperty); }
            set { SetValue(AllowZoomingProperty, value); }
        }
        #endregion

        /// <summary>
        /// Fires when cell selection changes.
        /// </summary>
        public event EventHandler<CellsSelectionEventArgs> CellsSelectionChanged;
        /// <summary>
        /// Fires on calculation error.
        /// </summary>
        public event EventHandler<CalcErrorEventArgs> CalculationError;
        /// <summary>
        /// Fires when sheet zoom factor changes.
        /// </summary>
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;
        /// <summary>
        ///  Gets the workbook.
        /// </summary>
        public IWorkBook WorkBook => _workBook;
        /// <summary>
        /// Gets the sheetview collection.
        /// </summary>
        public SheetViewCollection SheetViews { get; }

        /// <summary>
        /// Suspend UI updates
        /// </summary>
        public bool SuspendUpdates
        {
            get
            {
                return _changeListener.SuspendUpdates;
            }
            set
            {
                _changeListener.SuspendUpdates = value;
            }
        }

        public Spread()
        {
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            _changeListener = new WorksheetChangeListener(this);
            _workBook = new WorkBook("Book1", _changeListener);
            _undoRedoManager = new UndoRedoManager(this);
            SheetViews = new SheetViewCollection(this);
            _renderEngine = new RenderEngine();
            _sheetViewPane = new SheetViewPane(this);
            ScrollMode = SheetScrollMode.Item;
            SelectionBorderBrush = new SolidColorBrush(Color.FromRgb(16, 124, 65));
            BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
            Background = Brushes.Transparent;
            SnapsToDevicePixels = true;
            GridLineBrush = new SolidColorBrush(Color.FromRgb(160, 165, 175));
            PixelPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var workSheet = WorkBook.WorkSheets.AddSheet("Sheet1");
            WorkBook.WorkSheets.ActiveSheet = workSheet;
            _editingManager = new EditingManager(this);
            _selectionManager = new SelectionManager(this);
            _clipboardManager = new ClipboardManager(this);
            _zoomManager = new ZoomManager(this);
            SelectCell(0, 0);
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Hittest the spread at specific point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public SpreadHitTestResult HitTest(Point point)
        {
            if (SheetViews.ActiveSheetView != null)
            {
                var activeSheetView = SheetViews.ActiveSheetView.As<SheetView>();
                double zoom = activeSheetView.ZoomFactor > 0 ? activeSheetView.ZoomFactor : 1.0;
                var columnHeaderHeight = activeSheetView.GetColumnHeaderHeight() * zoom;
                var rowHeaderWidth = activeSheetView.GetRowHeaderWidth() * zoom;

                var panePoint = TranslatePoint(point, SheetViewPane);

                // Row headers hit test
                if (panePoint.X >= 0 && panePoint.X < rowHeaderWidth && panePoint.Y >= columnHeaderHeight && panePoint.Y < SheetViewPane.ActualHeight)
                    return SheetViewPane.RowHeadersRegion.HitTest(TranslatePoint(point, SheetViewPane.RowHeadersRegion));

                // Cells hit test
                if (panePoint.X >= rowHeaderWidth && panePoint.Y >= columnHeaderHeight && panePoint.X < SheetViewPane.ActualWidth && panePoint.Y < SheetViewPane.ActualHeight)
                    return SheetViewPane.CellsRegion.HitTest(TranslatePoint(point, SheetViewPane.CellsRegion));

                // Column headers hit test
                if (panePoint.X >= rowHeaderWidth && panePoint.Y >= 0 && panePoint.Y < columnHeaderHeight && panePoint.X < SheetViewPane.ActualWidth)
                    return SheetViewPane.ColumnHeadersRegion.HitTest(TranslatePoint(point, SheetViewPane.ColumnHeadersRegion));

                if (panePoint.X < rowHeaderWidth && panePoint.Y < columnHeaderHeight)
                    return SheetViewPane.TopLeftRegion.HitTest(TranslatePoint(point, SheetViewPane.TopLeftRegion));

                return null;
            }

            return null;
        }

        /// <summary>
        /// Scrolls to specific row.
        /// </summary>
        /// <param name="sheetView"></param>
        /// <param name="row"></param>
        public void ScrollToRow(ISheetView sheetView, int row)
        {
            var workSheet = sheetView.WorkSheet;
            SheetTabControl.VScrollBar.Value = ((SheetView)sheetView).ViewPort.GetRowLocation(row);
        }

        /// <summary>
        /// Scrolls to specific column.
        /// </summary>
        /// <param name="sheetView"></param>
        /// <param name="column"></param>
        public void ScrollToColumn(ISheetView sheetView, int column)
        {
            var workSheet = sheetView.WorkSheet;
            SheetTabControl.HScrollBar.Value = ((SheetView)sheetView).ViewPort.GetColumnLocation(column);
        }

        /// <summary>
        /// Undo last operation.
        /// </summary>
        public void Undo()
        {
            _undoRedoManager.Undo();
        }

        /// <summary>
        /// Redo last operation.
        /// </summary>
        public void Redo()
        {
            _undoRedoManager.Redo();
        }

        /// <summary>
        /// Starts editing the cell at provided index.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void BeginEdit(int row, int column)
        {
            _editingManager.BeginEdit((SheetView)SheetViews.ActiveSheetView, row, column);
        }

        /// <summary>
        /// Ends editing.
        /// </summary>
        /// <param name="commitChanges">Save changes to cell</param>
        public void EndEdit(bool commitChanges)
        {
            _editingManager.EndEdit(commitChanges);
        }

        public void SelectCell(int row, int col)
        {
            SheetViews.ActiveSheetView.SelectCell(row, col);
        }

        public void SelectColumn(int column)
        {
            SheetViews.ActiveSheetView.SelectColumn(column);
        }

        public void SelectColumns(int column, int count)
        {
            SheetViews.ActiveSheetView.SelectColumns(column, count);
        }

        public void SelectRow(int row)
        {
            SheetViews.ActiveSheetView.SelectRow(row);
        }

        public void SelectRows(int row, int count)
        {
            SheetViews.ActiveSheetView.SelectRows(row, count);
        }

        public void SelectRange(CellRange range)
        {
            SheetViews.ActiveSheetView.SelectRange(range);
        }

        public void SelectRange(int row, int column, int rowCount, int columnCount)
        {
            SheetViews.ActiveSheetView.SelectRange(row, column, rowCount, columnCount);
        }

        public void Copy()
        {
            SheetViews.ActiveSheetView.Copy();
        }

        public void Paste()
        {
            SheetViews.ActiveSheetView.Paste();
        }

        public void CopyRange(CellRange range)
        {
            SheetViews.ActiveSheetView.CopyRange(range);
        }

        public void MergeRange(CellRange range)
        {
            SheetViews.ActiveSheetView.MergeRange(range);
        }

        public void UnmergeRange(CellRange range)
        {
            SheetViews.ActiveSheetView.UnmergeRange(range);
        }

        public void ZoomIn()
        {
            _zoomManager.ZoomIn();
        }

        public void ZoomOut()
        {
            _zoomManager.ZoomOut();
        }

        /// <summary>
        /// Invalidates the provided sheet region.
        /// </summary>
        /// <param name="rowHeaders"></param>
        /// <param name="columnHeaders"></param>
        /// <param name="cells"></param>
        /// <param name="topLeft"></param>
        public void Invalidate(bool rowHeaders = true, bool columnHeaders = true, bool cells = true, bool topLeft = true)
        {
            var pane = SheetViewPane;

            pane.Draw(rowHeaders, columnHeaders, cells, cells, topLeft);

            if (cells)
            {
                var interactionLayer = pane.CellsRegion.GetInteractionLayer();
                if (interactionLayer != null)
                    interactionLayer.InvalidateVisual();
            }

            if (rowHeaders)
            {
                var interactionLayer = pane.RowHeadersRegion.GetInteractionLayer();
                if (interactionLayer != null)
                    interactionLayer.InvalidateVisual();
            }

            if (columnHeaders)
            {
                var interactionLayer = pane.ColumnHeadersRegion.GetInteractionLayer();
                if (interactionLayer != null)
                    interactionLayer.InvalidateVisual();
            }

            if (topLeft)
                pane.TopLeftRegion.InvalidateVisual();
        }

        /// <summary>
        /// Updates the grid line pen.
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="thickness"></param>
        private void UpdateGridlinePen(Brush brush, double thickness)
        {
            GridLinePen = new Pen(brush, thickness);
            GridLinePen.Freeze();
        }

        /// <summary>
        /// Updates Selection border pen.
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="thickness"></param>
        private void UpdateSelectionBorderPen(Brush brush, double thickness)
        {
            SelectionBorderPen = new Pen(brush, thickness);
            SelectionBorderPen.Freeze();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Disposes the resources.
        /// </summary>
        public void Dispose()
        {
            Loaded -= OnLoaded;
            WorkBook.Dispose();
            SheetTabControl.Dispose();
            SheetViewPane.Dispose();
            RenderEngine.Dispose();
        }
    }

    #region Internals
    public partial class Spread
    {
        internal const double GridLineThickness = 0.25;
        internal double PixelPerDip { get; set; }

        internal EditingManager EditingManager => _editingManager;
        internal SelectionManager SelectionManager => _selectionManager;
        internal ClipboardManager ClipboardManager => _clipboardManager;
        internal ZoomManager ZoomManager => _zoomManager;
        internal RenderEngine RenderEngine => _renderEngine;
        internal SheetViewPane SheetViewPane => _sheetViewPane;
        internal SheetTabControl SheetTabControl => _sheetTabControl;
        internal UndoRedoManager UndoRedoManager => _undoRedoManager;
        internal FormulaTextBox FormulaTextBox { get; set; }
        internal Pen GridLinePen { get; private set; }
        internal Pen SelectionBorderPen { get; private set; }

        internal void RaiseCellsSelectionChanged(CellsSelectionEventArgs args)
        {
            CellsSelectionChanged?.Invoke(this, args);
        }

        internal void RaiseCalculationError(CalcErrorEventArgs args)
        {
            CalculationError?.Invoke(this, args);
        }
    }
    #endregion

    #region Overrides
    public partial class Spread
    {
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (EditingManager != null && EditingManager.IsEditing)
                return;

            var activeSheetView = SheetViews.ActiveSheetView.As<SheetView>();
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.C:
                        ClipboardManager.Copy(activeSheetView);
                        break;

                    case Key.A:
                        SelectionManager.SelectRange(activeSheetView,((Cells)activeSheetView.WorkSheet.Cells).AsCellRange());
                        break;

                    case Key.V:
                        ClipboardManager.Paste(activeSheetView);
                        break;

                    case Key.Y:
                        UndoRedoManager.Redo();
                        break;

                    case Key.Z:
                        UndoRedoManager.Undo();
                        break;
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            var activeSheetView = SheetViews.ActiveSheetView.As<SheetView>();

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _zoomManager.HandleMouseWheel(e);
                e.Handled = true;
                return;
            }

            switch (activeSheetView.MouseWheelScrollDirection)
            {
                case MouseWheelScrollDirection.Vertical:
                    if (_sheetTabControl.VScrollBar == null)
                        return;
                    _sheetTabControl.VScrollBar.Value += -e.Delta / 2;
                    Invalidate(true, false, true, false);
                    break;

                case MouseWheelScrollDirection.Horizontal:
                    if (_sheetTabControl.HScrollBar == null)
                        return;
                    _sheetTabControl.HScrollBar.Value += -e.Delta / 2;
                    Invalidate(false, true, true, false);
                    break;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.PreviousSize == sizeInfo.NewSize)
                return;

            SheetTabControl.UpdateScrollbars();
            var activeSheetView = SheetViews.ActiveSheetView;
            activeSheetView.ScrollToHorizontalOffset(activeSheetView.ScrollPosition.X);
            activeSheetView.ScrollToVerticalOffset(activeSheetView.ScrollPosition.Y);
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            PixelPerDip = newDpi.PixelsPerDip;
            TextLayoutCache.Clear();
            Invalidate();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _sheetTabControl = GetTemplateChild("_sheetTabControl") as SheetTabControl;
        }
    }
    #endregion

    #region PropertyChanged Callbacks
    public partial class Spread
    {

        private static object CoerceZoomFactor(DependencyObject d, object baseValue)
        {
            if (baseValue is double val)
            {
                return Math.Max(0.1, Math.Min(4.0, Math.Round(val, 2)));
            }
            return 1.0;
        }

        private static void OnZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spread = d as Spread;
            spread?._zoomManager?.OnSpreadZoomFactorChanged((double)e.OldValue, (double)e.NewValue);
        }

        internal void RaiseZoomChanged(double oldZoom, double newZoom)
        {
            ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(oldZoom, newZoom));
        }

        private static void OnSelectionBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spread = d as Spread;
            if (e.NewValue != null && !e.NewValue.Equals(e.OldValue))
                spread.UpdateSelectionBorderPen(spread.SelectionBorderBrush, 1.5);
        }

        private static void OnGridLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spread = d as Spread;
            if (e.NewValue != null && !e.NewValue.Equals(e.OldValue))
                spread.UpdateGridlinePen(spread.GridLineBrush, GridLineThickness);
        }
    }
    #endregion
}