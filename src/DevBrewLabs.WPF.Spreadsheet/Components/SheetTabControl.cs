using DevBrewLabs.Spreadsheet;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    public class SheetTabControl : Control, IDisposable
    {
        static SheetTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SheetTabControl), new FrameworkPropertyMetadata(typeof(SheetTabControl)));
        }

        private ScrollBar _hScrollBar;
        private ScrollBar _vScrollBar;
        private ListBox _sheetsListBox;
        private RepeatButton _nextButton;
        private RepeatButton _previousButton;
        private Button _addButton;
        private Border _sheetViewPaneBorder;
        private Grid _root;
        private bool _eventsRegistered;

        internal ScrollBar HScrollBar => _hScrollBar;
        internal ScrollBar VScrollBar => _vScrollBar;
        internal Border SheetViewPaneBorder => _sheetViewPaneBorder;
        internal Spread Spread { get; private set; }

        public SheetTabControl()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Spread = TemplatedParent.As<Spread>();
            _root = GetTemplateChild("_root").As<Grid>();
            _sheetViewPaneBorder = GetTemplateChild("_sheetViewPaneBorder").As<Border>();
            _sheetViewPaneBorder.Child = Spread.SheetViewHost;
            _sheetViewPaneBorder.BorderBrush = Spread.BorderBrush;
            _sheetViewPaneBorder.BorderThickness = new Thickness(0);
            _sheetViewPaneBorder.SizeChanged += (s, e) => Spread?.SheetViewHost?.UpdateZoomTransform();
            _hScrollBar = GetTemplateChild("_hScrollBar").As<ScrollBar>();
            _vScrollBar = GetTemplateChild("_vScrollBar").As<ScrollBar>();
            _sheetsListBox = GetTemplateChild("_sheetsListBox").As<ListBox>();
            _previousButton = GetTemplateChild("_btnPrevious").As<RepeatButton>();
            _nextButton = GetTemplateChild("_btnNext").As<RepeatButton>();
            _addButton = GetTemplateChild("_btnAddSheet").As<Button>();
            RegisterInternalEventHandlers();
            _sheetsListBox.ItemsSource = Spread.Sheets;
            _sheetsListBox.SelectedIndex = 0;
        }

        private void DisplaySheet(SheetView sheetView)
        {
            Spread.SheetViewHost.HostSheet(sheetView);

            double oldZoom = Spread.ZoomFactor;
            if (oldZoom != sheetView.ZoomFactor)
            {
                Spread.ZoomFactor = sheetView.ZoomFactor;
            }
            else
            {
                Spread.SheetViewHost.UpdateZoomTransform();
                Spread.Invalidate();
            }

            UpdateScrollbars();
            _hScrollBar.Value = sheetView.ScrollPosition.X;
            _vScrollBar.Value = sheetView.ScrollPosition.Y;
            sheetView.ScrollToHorizontalOffset(sheetView.ScrollPosition.X);
            sheetView.ScrollToVerticalOffset(sheetView.ScrollPosition.Y);
            Spread.SheetViewHost.RefreshInteractionLayers();
        }

        private void OnAddSheetClick(object sender, RoutedEventArgs e)
        {
            Spread.WorkBook.WorkSheets.AddSheet($"Sheet{Spread.WorkBook.WorkSheets.Count + 1}");
            _sheetsListBox.SelectedIndex = _sheetsListBox.Items.Count - 1;
            ScrollSelectedSheetIntoView();
        }

        private void OnSheetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_sheetsListBox.SelectedItem == null)
                return;

            if (Spread.EditingManager.IsEditing)
                Spread.EditingManager.EndEdit(true);

            var sheetView = _sheetsListBox.SelectedItem.As<SheetView>();
            Spread.WorkBook.WorkSheets.ActiveSheet = sheetView.WorkSheet;
            DisplaySheet(sheetView);
            ScrollSelectedSheetIntoView();
        }

        private void ScrollSelectedSheetIntoView()
        {
            if (_sheetsListBox == null || _sheetsListBox.SelectedItem == null)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_sheetsListBox.SelectedItem == null)
                    return;

                _sheetsListBox.UpdateLayout();
                var scrollViewer = GetListBoxScrollViewer();
                if (scrollViewer != null)
                {
                    var container = _sheetsListBox.ItemContainerGenerator.ContainerFromItem(_sheetsListBox.SelectedItem) as FrameworkElement;
                    if (container != null)
                    {
                        try
                        {
                            var transform = container.TransformToAncestor(scrollViewer);
                            var rect = transform.TransformBounds(new Rect(new Point(0, 0), container.RenderSize));

                            if (rect.Right > scrollViewer.ViewportWidth)
                            {
                                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (rect.Right - scrollViewer.ViewportWidth) + 20);
                            }
                            else if (rect.Left < 0)
                            {
                                scrollViewer.ScrollToHorizontalOffset(Math.Max(0, scrollViewer.HorizontalOffset + rect.Left - 20));
                            }
                        }
                        catch
                        {
                            scrollViewer.ScrollToRightEnd();
                        }
                    }
                    else
                    {
                        scrollViewer.ScrollToRightEnd();
                    }
                }
                else
                {
                    _sheetsListBox.ScrollIntoView(_sheetsListBox.SelectedItem);
                }
            }), DispatcherPriority.Render);
        }

        private ScrollViewer GetListBoxScrollViewer()
        {
            if (_sheetsListBox == null)
                return null;

            return FindVisualChild<ScrollViewer>(_sheetsListBox);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void OnSheetsListBoxPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;
            var item = FindVisualParent<ListBoxItem>(element);
            if (item != null)
            {
                var sheetView = item.DataContext as SheetView;
                if (sheetView != null)
                {
                    _sheetsListBox.SelectedItem = sheetView;
                    int index = _sheetsListBox.Items.IndexOf(sheetView);
                    Spread?.ContextMenuManager?.ShowSheetTabContextMenu(sheetView, index, item);
                    e.Handled = true;
                }
            }
        }

        private void OnNextSheetClick(object sender, RoutedEventArgs e)
        {
            var scrollViewer = GetListBoxScrollViewer();
            if (scrollViewer != null && scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 60);
            }

            if (_sheetsListBox.SelectedIndex < _sheetsListBox.Items.Count - 1)
            {
                _sheetsListBox.SelectedIndex++;
            }
        }

        private void OnPreviousSheetClick(object sender, RoutedEventArgs e)
        {
            var scrollViewer = GetListBoxScrollViewer();
            if (scrollViewer != null && scrollViewer.HorizontalOffset > 0)
            {
                scrollViewer.ScrollToHorizontalOffset(Math.Max(0, scrollViewer.HorizontalOffset - 60));
            }

            if (_sheetsListBox.SelectedIndex > 0)
            {
                _sheetsListBox.SelectedIndex--;
            }
        }

        /// <summary>
        /// Register internal event handlers
        /// </summary>
        private void RegisterInternalEventHandlers()
        {
            if (_eventsRegistered)
                return;

            WeakEventManager<ScrollBar, RoutedPropertyChangedEventArgs<double>>.AddHandler(_hScrollBar, "ValueChanged", OnHorizontalScrollBarValueChanged);
            WeakEventManager<ScrollBar, RoutedPropertyChangedEventArgs<double>>.AddHandler(_vScrollBar, "ValueChanged", OnVerticalScrollBarValueChanged);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(_addButton, "Click", OnAddSheetClick);
            WeakEventManager<RepeatButton, RoutedEventArgs>.AddHandler(_nextButton, "Click", OnNextSheetClick);
            WeakEventManager<RepeatButton, RoutedEventArgs>.AddHandler(_previousButton, "Click", OnPreviousSheetClick);
            WeakEventManager<ListBox, SelectionChangedEventArgs>.AddHandler(_sheetsListBox, "SelectionChanged", OnSheetSelectionChanged);
            if (_sheetsListBox != null)
                _sheetsListBox.PreviewMouseRightButtonDown += OnSheetsListBoxPreviewMouseRightButtonDown;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                WeakEventManager<Thumb, DragCompletedEventArgs>.AddHandler(_hScrollBar.Track.Thumb, "DragCompleted", OnHorizontalScrollDragCompleted);
                WeakEventManager<Thumb, DragCompletedEventArgs>.AddHandler(_vScrollBar.Track.Thumb, "DragCompleted", OnVerticalScrollDragCompleted);
            }), DispatcherPriority.Loaded);

            _eventsRegistered = true;
        }

        /// <summary>
        /// Unregister internal event handlers.
        /// </summary>
        private void UnRegisterInternalEventHandlers()
        {
            if (!_eventsRegistered)
                return;

            WeakEventManager<ScrollBar, RoutedPropertyChangedEventArgs<double>>.RemoveHandler(_hScrollBar, "ValueChanged", OnHorizontalScrollBarValueChanged);
            WeakEventManager<Thumb, DragCompletedEventArgs>.RemoveHandler(_hScrollBar.Track.Thumb, "DragCompleted", OnHorizontalScrollDragCompleted);
            WeakEventManager<ScrollBar, RoutedPropertyChangedEventArgs<double>>.RemoveHandler(_vScrollBar, "ValueChanged", OnVerticalScrollBarValueChanged);
            WeakEventManager<Thumb, DragCompletedEventArgs>.RemoveHandler(_vScrollBar.Track.Thumb, "DragCompleted", OnVerticalScrollDragCompleted);
            WeakEventManager<Button, RoutedEventArgs>.RemoveHandler(_addButton, "Click", OnAddSheetClick);
            WeakEventManager<ListBox, SelectionChangedEventArgs>.RemoveHandler(_sheetsListBox, "SelectionChanged", OnSheetSelectionChanged);
            WeakEventManager<RepeatButton, RoutedEventArgs>.RemoveHandler(_nextButton, "Click", OnNextSheetClick);
            WeakEventManager<RepeatButton, RoutedEventArgs>.RemoveHandler(_previousButton, "Click", OnPreviousSheetClick);
            if (_sheetsListBox != null)
                _sheetsListBox.PreviewMouseRightButtonDown -= OnSheetsListBoxPreviewMouseRightButtonDown;
            _eventsRegistered = false;
        }

        /// <summary>
        /// Updates the scrollbars according to the sheet size and viewport.
        /// </summary>
        internal void UpdateScrollbars()
        {
            var sheetView = (SheetView)Spread.Sheets.ActiveSheet;
            var sheet = sheetView.WorkSheet;
            var columns = (Columns)sheet.Columns;
            var rows = (Rows)sheet.Rows;
            
            var actualWidth = sheetView.CellsSurface.ActualWidth;
            _hScrollBar.LargeChange = actualWidth;
            
            if (sheet.ColumnCount > 0)
            {
                var totalWidth = sheetView.ViewPort.GetColumnLocation(sheet.ColumnCount - 1) + columns.GetColumnWidth(sheet.ColumnCount - 1);
                var maxScrollX = totalWidth - actualWidth + sheet.DefaultColumnWidth + 30;
                _hScrollBar.Maximum = Math.Max(0, maxScrollX);

                double minThumbSize = 20;
                if (actualWidth > minThumbSize && _hScrollBar.Maximum > 0)
                {
                    double minViewportSize = (minThumbSize * _hScrollBar.Maximum) / (actualWidth - minThumbSize);
                    _hScrollBar.ViewportSize = Math.Max(actualWidth, minViewportSize);
                }
                else
                {
                    _hScrollBar.ViewportSize = actualWidth;
                }
            }

            var actualHeight = sheetView.CellsSurface.ActualHeight;
            _vScrollBar.LargeChange = actualHeight;
            
            if (sheet.RowCount > 0)
            {
                var totalHeight = sheetView.ViewPort.GetRowLocation(sheet.RowCount - 1) + rows.GetRowHeight(sheet.RowCount - 1);
                var maxScrollY = totalHeight - actualHeight + sheet.DefaultRowHeight + 30;
                _vScrollBar.Maximum = Math.Max(0, maxScrollY);

                double minThumbSize = 20;
                if (actualHeight > minThumbSize && _vScrollBar.Maximum > 0)
                {
                    double minViewportSize = (minThumbSize * _vScrollBar.Maximum) / (actualHeight - minThumbSize);
                    _vScrollBar.ViewportSize = Math.Max(actualHeight, minViewportSize);
                }
                else
                {
                    _vScrollBar.ViewportSize = actualHeight;
                }
            }

            if (_vScrollBar.Maximum == _vScrollBar.Minimum)
                _vScrollBar.Visibility = Visibility.Hidden;
            else
                _vScrollBar.Visibility = Visibility.Visible;

            if (_hScrollBar.Maximum == _hScrollBar.Minimum)
                _hScrollBar.Visibility = Visibility.Hidden;
            else
                _hScrollBar.Visibility = Visibility.Visible;
        }

        #region Scrolling
        private void OnVerticalScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_vScrollBar.Track.Thumb.IsDragging && Spread.ScrollMode == SheetScrollMode.Deferred)
                return;

            Spread.Sheets.ActiveSheet.ScrollToVerticalOffset(e.NewValue);
        }

        private void OnHorizontalScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_hScrollBar.Track.Thumb.IsDragging && Spread.ScrollMode == SheetScrollMode.Deferred)
                return;

            Spread.Sheets.ActiveSheet.ScrollToHorizontalOffset(e.NewValue);
        }

        private void OnHorizontalScrollDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (Spread.ScrollMode == SheetScrollMode.Deferred)
            {
                Spread.Sheets.ActiveSheet.ScrollToHorizontalOffset(_hScrollBar.Value);
            }
        }

        private void OnVerticalScrollDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (Spread.ScrollMode == SheetScrollMode.Deferred)
            {
                Spread.Sheets.ActiveSheet.ScrollToVerticalOffset(_vScrollBar.Value);
            }
        }
        #endregion

        public void Dispose()
        {
            UnRegisterInternalEventHandlers();
            _sheetsListBox.ItemsSource = null;
            _root.Children.Clear();
            _hScrollBar = null;
            _vScrollBar = null;
            _sheetsListBox = null;
            _nextButton = null;
            _previousButton = null;
            _addButton = null;
            _sheetViewPaneBorder = null;
            _root = null;
        }
    }
}
