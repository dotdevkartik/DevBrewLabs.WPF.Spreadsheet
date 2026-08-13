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
    public class Spread : Control, IDisposable
    {
        internal const double GridLineThickness = 0.25;

        #region Dependency Properties
        public static readonly DependencyProperty ScrollBarStyleProperty;
        public static readonly DependencyProperty ScrollModeProperty;
        public static readonly DependencyProperty SelectionBackgroundProperty;
        public static readonly DependencyProperty GridLineBrushProperty;
        public static readonly DependencyProperty SelectionBorderBrushProperty;
        public static readonly DependencyProperty AllowRowResizeProperty;
        public static readonly DependencyProperty AllowColumnResizeProperty;
        public static readonly DependencyProperty ShowFormulaSuggestionsProperty;
        public static readonly DependencyProperty SheetTabsVisibilityProperty;
        public static readonly DependencyProperty IsSelectionAnimationEnabledProperty;

        public static readonly DependencyProperty ZoomFactorProperty;

        static Spread()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spread), new FrameworkPropertyMetadata(typeof(Spread)));
            ZoomFactorProperty = DependencyProperty.Register("ZoomFactor", typeof(double), typeof(Spread),
                new FrameworkPropertyMetadata(1.0, OnZoomFactorChanged, CoerceZoomFactor));
            ScrollModeProperty = DependencyProperty.Register("ScrollMode", typeof(SheetScrollMode), typeof(Spread),
                new PropertyMetadata(SheetScrollMode.Item));
            SelectionBackgroundProperty = DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(Spread),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(50, 25, 25, 25))));
            GridLineBrushProperty = DependencyProperty.Register("GridLineBrush", typeof(Brush), typeof(Spread), 
                new PropertyMetadata(OnGridLineBrushChanged));
            SelectionBorderBrushProperty = DependencyProperty.Register("SelectionBorderBrush", typeof(Brush), typeof(Spread),
                new PropertyMetadata(OnSelectionBorderBrushChanged));
            AllowRowResizeProperty = DependencyProperty.Register("AllowRowResize", typeof(bool), typeof(Spread), new PropertyMetadata(true));
            AllowColumnResizeProperty =
            DependencyProperty.Register("AllowColumnResize", typeof(bool), typeof(Spread), new PropertyMetadata(true));
            ShowFormulaSuggestionsProperty =
            DependencyProperty.Register("ShowFormulaSuggestions", typeof(bool), typeof(Spread), new PropertyMetadata(true));
            IsSelectionAnimationEnabledProperty =
            DependencyProperty.Register("IsSelectionAnimationEnabled", typeof(bool), typeof(Spread), new PropertyMetadata(false));
            SheetTabsVisibilityProperty =
            DependencyProperty.Register("SheetTabsVisibility", typeof(Visibility), typeof(Spread), new PropertyMetadata(Visibility.Visible));
            ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("/DevBrewLabs.WPF.Spreadsheet;component/Themes/ScrollBarStyle.xaml", UriKind.Relative));
            ScrollBarStyleProperty =
            DependencyProperty.Register("ScrollBarStyle", typeof(Style), typeof(Spread), new PropertyMetadata((Style)res["ScrollBarStyle"]));
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
        /// Gets or sets whether the sheet tabs are visible.
        /// </summary>
        public Visibility SheetTabsVisibility
        {
            get { return (Visibility)GetValue(SheetTabsVisibilityProperty); }
            set { SetValue(SheetTabsVisibilityProperty, value); }
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
        #endregion

        private SheetTabControl _tabControl;
        private WorkBook _workBook;

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
        internal RenderEngine RenderEngine { get; }
        internal SheetViewPane SheetViewPane { get; }
        internal SheetTabControl SheetTabControl => _tabControl;
        internal FormulaTextBox FormulaTextBox { get; set; }
        internal Pen GridLinePen { get; private set; }
        internal Pen SelectionBorderPen { get; private set; }
        internal double PixelPerDip { get; set; }
        /// <summary>
        /// Gets the undo/redo manager.
        /// </summary>
        public UndoRedoManager UndoRedoManager { get; private set; }
        /// <summary>
        /// Gets the workbook.
        /// </summary>
        public IWorkBook WorkBook => _workBook;
        /// <summary>
        /// Gets the editing manager.
        /// </summary>
        public IEditingManager EditingManager { get; }
        /// <summary>
        /// Gets the selection manager.
        /// </summary>
        public ISelectionManager SelectionManager { get; }
        /// <summary>
        /// Gets the clipboard manager.
        /// </summary>
        public IClipboardManager ClipboardManager { get; }
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
                return _workBook.UpdateProvider.SuspendUpdates;
            }
            set
            {
                _workBook.UpdateProvider.SuspendUpdates = value;
            }
        }

        #region ctor
        public Spread()
        {
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            _workBook = new WorkBook("Book1", new UIUpdateProvider(this));
            UndoRedoManager = new UndoRedoManager(this);
            SheetViews = new SheetViewCollection(this);
            RenderEngine = new RenderEngine();
            SheetViewPane = new SheetViewPane(this);
            ScrollMode = SheetScrollMode.Item;
            SelectionBorderBrush = new SolidColorBrush(Color.FromRgb(16, 124, 65));
            BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
            Background = Brushes.Transparent;
            SnapsToDevicePixels = true;
            GridLineBrush = new SolidColorBrush(Color.FromRgb(160, 165, 175));
            PixelPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var workSheet = WorkBook.WorkSheets.AddSheet("Sheet1");
            WorkBook.WorkSheets.ActiveSheet = workSheet;
            EditingManager = new EditingManager(this);
            SelectionManager = new SelectionManager(this);
            ClipboardManager = new ClipboardManager(this);
            SelectionManager.SelectCell(0, 0);
            Loaded += OnLoaded;
        }
        #endregion

        #region Public Methods
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
            SheetTabControl.VScrollBar.Value = ((Rows)workSheet.Rows).GetLocation(row);
        }

        /// <summary>
        /// Scrolls to specific column.
        /// </summary>
        /// <param name="sheetView"></param>
        /// <param name="column"></param>
        public void ScrollToColumn(ISheetView sheetView, int column)
        {
            var workSheet = sheetView.WorkSheet;
            SheetTabControl.HScrollBar.Value = ((Columns)workSheet.Columns).GetLocation(column);
        }

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


        #endregion

        #region Private Methods
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
        #endregion

        #region Internal Methods
        internal void RaiseCellsSelectionChanged(CellsSelectionEventArgs args)
        {
            CellsSelectionChanged?.Invoke(this, args);
        }

        internal void RaiseCalculationError(CalcErrorEventArgs args)
        {
            CalculationError?.Invoke(this, args);
        }
        #endregion

        #region Overrides
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
                        SelectionManager.SelectRange(((Cells)activeSheetView.WorkSheet.Cells).AsCellRange());
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
                if (e.Delta > 0)
                {
                    ZoomFactor = Math.Min(4.0, Math.Round(ZoomFactor + 0.1, 2));
                }
                else if (e.Delta < 0)
                {
                    ZoomFactor = Math.Max(0.1, Math.Round(ZoomFactor - 0.1, 2));
                }
                e.Handled = true;
                return;
            }

            switch (activeSheetView.MouseWheelScrollDirection)
            {
                case MouseWheelScrollDirection.Vertical:
                    if (_tabControl.VScrollBar == null)
                        return;
                    _tabControl.VScrollBar.Value += -e.Delta / 2;
                    Invalidate(true, false, true, false);
                    break;

                case MouseWheelScrollDirection.Horizontal:
                    if (_tabControl.HScrollBar == null)
                        return;
                    _tabControl.HScrollBar.Value += -e.Delta / 2;
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
            _tabControl = GetTemplateChild("_sheetTabControl") as SheetTabControl;
        }
        #endregion

        #region PropertyChanged Callbacks
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
            if (spread?.SheetViews?.ActiveSheetView != null)
            {
                var newZoom = (double)e.NewValue;
                var oldZoom = (double)e.OldValue;
                if (spread.SheetViews.ActiveSheetView.ZoomFactor != newZoom)
                {
                    spread.SheetViews.ActiveSheetView.ZoomFactor = newZoom;
                }
                spread.RaiseZoomChanged(oldZoom, newZoom);
            }
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
        #endregion

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
}


