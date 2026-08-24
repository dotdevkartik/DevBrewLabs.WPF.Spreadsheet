using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Components;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Data.Common;
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
        private RowResizeManager _rowResizeManager;
        private ColumnResizeManager _columnResizeManager;
        private RenderEngine _renderEngine;
        private FilterManager _filterManager;
        private FormulaSuggestionManager _formulaSuggestionManager;
        private HeaderHoverManager _headerHoverManager;
        private ContextMenuManager _contextMenuManager;
        private SheetViewHost _sheetViewHost;
        private SheetTabControl _sheetTabControl;
        private UndoRedoManager _undoRedoManager;
        private Workbook _workBook;
        private WorksheetChangeListener _changeListener;

        #region Dependency Properties
        public static readonly DependencyProperty ScrollBarStyleProperty;
        public static readonly DependencyProperty ResizeMarkerStyleProperty;
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
        public static readonly DependencyProperty AllowFilteringProperty;
        public static readonly DependencyProperty AllowContextMenuProperty;
        public static readonly DependencyProperty CellContextMenuProperty;
        public static readonly DependencyProperty RowHeaderContextMenuProperty;
        public static readonly DependencyProperty ColumnHeaderContextMenuProperty;
        public static readonly DependencyProperty SheetTabContextMenuProperty;
        public static readonly DependencyProperty MouseHoverHeaderBackgroundProperty;
        public static readonly DependencyProperty SelectedHeaderBackgroundProperty;
        public static readonly DependencyProperty RangeSelectedHeaderBackgroundProperty;
        public static readonly DependencyProperty SelectedHeaderForegroundProperty;

        static Spread()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spread), new FrameworkPropertyMetadata(typeof(Spread)));

            MouseHoverHeaderBackgroundProperty = DependencyProperty.Register(
                nameof(MouseHoverHeaderBackground),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(
                    new SolidColorBrush(Color.FromRgb(134, 196, 162)),
                    OnHeaderAppearanceChanged));

            SelectedHeaderBackgroundProperty = DependencyProperty.Register(
                nameof(SelectedHeaderBackground),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(
                    new SolidColorBrush(Color.FromRgb(16, 124, 65)),
                    OnHeaderAppearanceChanged));

            RangeSelectedHeaderBackgroundProperty = DependencyProperty.Register(
                nameof(RangeSelectedHeaderBackground),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(
                    new SolidColorBrush(Color.FromArgb(35, 25, 25, 25)),
                    OnHeaderAppearanceChanged));

            SelectedHeaderForegroundProperty = DependencyProperty.Register(
                nameof(SelectedHeaderForeground),
                typeof(Brush),
                typeof(Spread),
                new PropertyMetadata(
                    Brushes.White,
                    OnHeaderAppearanceChanged));

            AllowFilteringProperty = DependencyProperty.Register(
                nameof(AllowFiltering),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(false));

            AllowContextMenuProperty = DependencyProperty.Register(
                nameof(AllowContextMenu),
                typeof(bool),
                typeof(Spread),
                new PropertyMetadata(true));

            CellContextMenuProperty = DependencyProperty.Register(
                nameof(CellContextMenu),
                typeof(ContextMenu),
                typeof(Spread),
                new PropertyMetadata(null));

            RowHeaderContextMenuProperty = DependencyProperty.Register(
                nameof(RowHeaderContextMenu),
                typeof(ContextMenu),
                typeof(Spread),
                new PropertyMetadata(null));

            ColumnHeaderContextMenuProperty = DependencyProperty.Register(
                nameof(ColumnHeaderContextMenu),
                typeof(ContextMenu),
                typeof(Spread),
                new PropertyMetadata(null));

            SheetTabContextMenuProperty = DependencyProperty.Register(
                nameof(SheetTabContextMenu),
                typeof(ContextMenu),
                typeof(Spread),
                new PropertyMetadata(null));

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

            ResizeMarkerStyleProperty = DependencyProperty.Register(
                nameof(ResizeMarkerStyle),
                typeof(Style),
                typeof(Spread),
                new PropertyMetadata(null, OnResizeMarkerStyleChanged));
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
        /// Gets or sets the resize marker line style.
        /// </summary>
        public Style ResizeMarkerStyle
        {
            get { return (Style)GetValue(ResizeMarkerStyleProperty); }
            set { SetValue(ResizeMarkerStyleProperty, value); }
        }

        private static void OnResizeMarkerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spread = (Spread)d;
            var style = (Style)e.NewValue;
            spread._columnResizeManager?.UpdateResizeMarkerStyle(style);
            spread._rowResizeManager?.UpdateResizeMarkerStyle(style);
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
        /// Gets or sets the header hover brush.
        /// </summary>
        public Brush MouseHoverHeaderBackground
        {
            get { return (Brush)GetValue(MouseHoverHeaderBackgroundProperty); }
            set { SetValue(MouseHoverHeaderBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected header background brush.
        /// </summary>
        public Brush SelectedHeaderBackground
        {
            get { return (Brush)GetValue(SelectedHeaderBackgroundProperty); }
            set { SetValue(SelectedHeaderBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the range selected header background brush.
        /// </summary>
        public Brush RangeSelectedHeaderBackground
        {
            get { return (Brush)GetValue(RangeSelectedHeaderBackgroundProperty); }
            set { SetValue(RangeSelectedHeaderBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected header foreground brush.
        /// </summary>
        public Brush SelectedHeaderForeground
        {
            get { return (Brush)GetValue(SelectedHeaderForegroundProperty); }
            set { SetValue(SelectedHeaderForegroundProperty, value); }
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

        public bool AllowFiltering
        {
            get { return (bool)GetValue(AllowFilteringProperty); }
            set { SetValue(AllowFilteringProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether context menus are allowed across Spread.
        /// </summary>
        public bool AllowContextMenu
        {
            get { return (bool)GetValue(AllowContextMenuProperty); }
            set { SetValue(AllowContextMenuProperty, value); }
        }

        /// <summary>
        /// Gets or sets custom ContextMenu for cell area.
        /// </summary>
        public ContextMenu CellContextMenu
        {
            get { return (ContextMenu)GetValue(CellContextMenuProperty); }
            set { SetValue(CellContextMenuProperty, value); }
        }

        /// <summary>
        /// Gets or sets custom ContextMenu for row headers.
        /// </summary>
        public ContextMenu RowHeaderContextMenu
        {
            get { return (ContextMenu)GetValue(RowHeaderContextMenuProperty); }
            set { SetValue(RowHeaderContextMenuProperty, value); }
        }

        /// <summary>
        /// Gets or sets custom ContextMenu for column headers.
        /// </summary>
        public ContextMenu ColumnHeaderContextMenu
        {
            get { return (ContextMenu)GetValue(ColumnHeaderContextMenuProperty); }
            set { SetValue(ColumnHeaderContextMenuProperty, value); }
        }

        /// <summary>
        /// Gets or sets custom ContextMenu for sheet tabs.
        /// </summary>
        public ContextMenu SheetTabContextMenu
        {
            get { return (ContextMenu)GetValue(SheetTabContextMenuProperty); }
            set { SetValue(SheetTabContextMenuProperty, value); }
        }
        #endregion

        /// <summary>
        /// Fires before a context menu is displayed on any spreadsheet region.
        /// </summary>
        public new event EventHandler<SpreadContextMenuOpeningEventArgs> ContextMenuOpening;
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
        public IWorkbook WorkBook => _workBook;
        /// <summary>
        /// Gets the sheetview collection.
        /// </summary>
        public SheetViewCollection Sheets { get; }

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
            _workBook = new Workbook("Book1", _changeListener);
            _undoRedoManager = new UndoRedoManager(this);
            Sheets = new SheetViewCollection(this);
            _renderEngine = new RenderEngine();
            _sheetViewHost = new SheetViewHost(this);
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
            _filterManager = new FilterManager(this);
            _formulaSuggestionManager = new FormulaSuggestionManager(this);
            _clipboardManager = new ClipboardManager(this);
            _contextMenuManager = new ContextMenuManager(this);
            _rowResizeManager = new RowResizeManager(this);
            _columnResizeManager = new ColumnResizeManager(this);
            _headerHoverManager = new HeaderHoverManager(this);
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
            if (Sheets.ActiveSheet != null)
            {
                var activeSheetView = Sheets.ActiveSheet.As<SheetView>();
                double zoom = activeSheetView.ZoomFactor > 0 ? activeSheetView.ZoomFactor : 1.0;
                var columnHeaderHeight = activeSheetView.GetColumnHeaderHeight() * zoom;
                var rowHeaderWidth = activeSheetView.GetRowHeaderWidth() * zoom;

                var panePoint = TranslatePoint(point, _sheetViewHost);

                // Row headers hit test
                if (panePoint.X >= 0 && panePoint.X < rowHeaderWidth && panePoint.Y >= columnHeaderHeight && panePoint.Y < _sheetViewHost.ActualHeight)
                    return activeSheetView.RowHeadersSurface.HitTest(TranslatePoint(point, activeSheetView.RowHeadersSurface));

                // Cells hit test
                if (panePoint.X >= rowHeaderWidth && panePoint.Y >= columnHeaderHeight && panePoint.X < _sheetViewHost.ActualWidth && panePoint.Y < _sheetViewHost.ActualHeight)
                    return activeSheetView.CellsSurface.HitTest(TranslatePoint(point, activeSheetView.CellsSurface));

                // Column headers hit test
                if (panePoint.X >= rowHeaderWidth && panePoint.Y >= 0 && panePoint.Y < columnHeaderHeight && panePoint.X < _sheetViewHost.ActualWidth)
                    return activeSheetView.ColumnHeadersSurface.HitTest(TranslatePoint(point, activeSheetView.ColumnHeadersSurface));

                if (panePoint.X < rowHeaderWidth && panePoint.Y < columnHeaderHeight)
                    return activeSheetView.TopLeftSurface.HitTest(TranslatePoint(point, activeSheetView.TopLeftSurface));

                return null;
            }

            return null;
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
            _editingManager.BeginEdit((SheetView)Sheets.ActiveSheet, row, column);
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
            Sheets.ActiveSheet.SelectCell(row, col);
        }

        public void SelectColumn(int column)
        {
            Sheets.ActiveSheet.SelectColumn(column);
        }

        public void SelectColumns(int column, int count)
        {
            Sheets.ActiveSheet.SelectColumns(column, count);
        }

        public void SelectRow(int row)
        {
            Sheets.ActiveSheet.SelectRow(row);
        }

        public void SelectRows(int row, int count)
        {
            Sheets.ActiveSheet.SelectRows(row, count);
        }

        public void SelectRange(CellRange range)
        {
            Sheets.ActiveSheet.SelectRange(range);
        }

        public void SelectRange(int row, int column, int rowCount, int columnCount)
        {
            Sheets.ActiveSheet.SelectRange(row, column, rowCount, columnCount);
        }

        public void Cut()
        {
            Sheets.ActiveSheet.Cut();
        }

        public void Copy()
        {
            Sheets.ActiveSheet.Copy();
        }

        public void Paste()
        {
            Sheets.ActiveSheet.Paste();
        }

        public void ClearContents()
        {
            Sheets.ActiveSheet.ClearContents();
        }

        public void ClearContents(CellRange range)
        {
            Sheets.ActiveSheet.ClearContents(range);
        }

        public void CopyRange(CellRange range)
        {
            Sheets.ActiveSheet.CopyRange(range);
        }

        public void MergeRange(CellRange range)
        {
            Sheets.ActiveSheet.MergeRange(range);
        }

        public void UnmergeRange(CellRange range)
        {
            Sheets.ActiveSheet.UnmergeRange(range);
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
        /// Performs a full refresh of the active sheet: updates header sizes, visible range,
        /// scrollbars, active editor layout, and redraws all surfaces.
        /// </summary>
        public void Refresh()
        {
            var activeSheetView = Sheets.ActiveSheet as SheetView;
            if (activeSheetView != null)
            {
                UpdateHeadersSize();
                activeSheetView.ViewPort.CalculateVisibleRange();
                UpdateScrollbars();
                if (_editingManager != null && _editingManager.IsEditing)
                {
                    _editingManager.UpdateEditorLayout();
                }
            }
            Invalidate();
        }

        public void ScrollToRow(int row)
        {
            SheetView sheetView = (SheetView)Sheets.ActiveSheet;
            _sheetTabControl?.SetVerticalScrollPosition(sheetView.ViewPort.GetRowLocation(row));
        }

        public void ScrollToColumn(int column)
        {
            SheetView sheetView = (SheetView)Sheets.ActiveSheet;
            _sheetTabControl?.SetHorizontalScrollPosition(sheetView.ViewPort.GetColumnLocation(column));
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
            _sheetViewHost?.Draw(rowHeaders, columnHeaders, cells, cells, topLeft);
            _sheetViewHost?.RefreshInteractionLayers(rowHeaders, columnHeaders, cells);
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
            Refresh();
        }

        /// <summary>
        /// Disposes the resources.
        /// </summary>
        public void Dispose()
        {
            Loaded -= OnLoaded;
            WorkBook.Dispose();
            _sheetTabControl?.Dispose();
            _sheetViewHost?.Dispose();
            RenderEngine.Dispose();
            _filterManager?.Dispose();
            _formulaSuggestionManager?.Dispose();
            _headerHoverManager?.Dispose();
            _contextMenuManager?.Dispose();
        }
    }

    #region Internals
    public partial class Spread
    {
        internal const double GridLineThickness = 0.35;
        internal const double SelectionBorderThickness = 1.5;
        internal double PixelPerDip { get; set; }

        internal EditingManager EditingManager => _editingManager;
        internal SelectionManager SelectionManager => _selectionManager;
        internal ClipboardManager ClipboardManager => _clipboardManager;
        internal ContextMenuManager ContextMenuManager => _contextMenuManager;
        internal ZoomManager ZoomManager => _zoomManager;
        internal RenderEngine RenderEngine => _renderEngine;
        internal UndoRedoManager UndoRedoManager => _undoRedoManager;
        internal RowResizeManager RowResizeManager => _rowResizeManager;
        internal ColumnResizeManager ColumnResizeManager => _columnResizeManager;
        internal FilterManager FilterManager => _filterManager;
        internal FormulaSuggestionManager FormulaSuggestionManager => _formulaSuggestionManager;
        internal HeaderHoverManager HeaderHoverManager => _headerHoverManager;
        internal FormulaTextBox FormulaTextBox { get; set; }
        internal Pen GridLinePen { get; private set; }
        internal Pen SelectionBorderPen { get; private set; }

        internal UIElement SheetViewHostElement => _sheetViewHost;

        internal void UpdateScrollbars()
        {
            _sheetTabControl?.UpdateScrollbars();
        }

        internal void UpdateHeadersSize()
        {
            _sheetViewHost?.UpdateHeadersSize();
        }

        internal void UpdateZoomTransform()
        {
            _sheetViewHost?.UpdateZoomTransform();
        }

        internal void RefreshInteractionLayers(bool rowHeaders = true, bool columnHeaders = true, bool cells = true)
        {
            _sheetViewHost?.RefreshInteractionLayers(rowHeaders, columnHeaders, cells);
        }

        internal void InvalidateSurfaces(bool rowHeaders = true, bool columnHeaders = true, bool cells = true, bool gridLines = true, bool topLeft = true)
        {
            _sheetViewHost?.Draw(rowHeaders, columnHeaders, cells, gridLines, topLeft);
        }

        internal void HostSheet(SheetView sheetView)
        {
            _sheetViewHost?.HostSheet(sheetView);
        }

        internal void RaiseCellsSelectionChanged(CellsSelectionEventArgs args)
        {
            CellsSelectionChanged?.Invoke(this, args);
        }

        internal void RaiseCalculationError(CalcErrorEventArgs args)
        {
            CalculationError?.Invoke(this, args);
        }

        internal void RaiseContextMenuOpening(SpreadContextMenuOpeningEventArgs args)
        {
            ContextMenuOpening?.Invoke(this, args);
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

            var activeSheetView = Sheets.ActiveSheet.As<SheetView>();
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.X:
                        ClipboardManager.Cut(activeSheetView);
                        break;

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

            _filterManager?.HideFilterDropdown();
            _formulaSuggestionManager?.Hide();

            var activeSheetView = Sheets.ActiveSheet.As<SheetView>();

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _zoomManager.HandleMouseWheel(e);
                e.Handled = true;
                return;
            }

            switch (activeSheetView.MouseWheelScrollDirection)
            {
                case MouseWheelScrollDirection.Vertical:
                    _sheetTabControl?.ScrollVerticalBy(-e.Delta / 5.0);
                    break;

                case MouseWheelScrollDirection.Horizontal:
                    _sheetTabControl?.ScrollHorizontalBy(-e.Delta / 5.0);
                    break;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.PreviousSize == sizeInfo.NewSize)
                return;

            _filterManager?.HideFilterDropdown();
            _formulaSuggestionManager?.Hide();

            Refresh();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            PixelPerDip = newDpi.PixelsPerDip;
            TextLayoutCache.Clear();
            Refresh();
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
                spread.UpdateSelectionBorderPen(spread.SelectionBorderBrush, SelectionBorderThickness);
        }

        private static void OnGridLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spread = d as Spread;
            if (e.NewValue != null && !e.NewValue.Equals(e.OldValue))
                spread.UpdateGridlinePen(spread.GridLineBrush, GridLineThickness);
        }

        private static void OnHeaderAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spread = (Spread)d;
            spread.Invalidate(rowHeaders: true, columnHeaders: true, cells: false, topLeft: false);
        }
    }
    #endregion
}