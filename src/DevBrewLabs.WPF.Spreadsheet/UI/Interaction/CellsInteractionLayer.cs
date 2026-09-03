using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Utils;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal class CellsInteractionLayer : InteractionLayer
    {
        private bool _scrolling = false;
        private bool _isDragging = false;
        private Rect _targetSelectionRect;
        private Rect _targetActiveCellRect;
        private bool _isFirstRender = true;
        private int _lastActiveRow = -1;
        private int _lastActiveColumn = -1;
        private int _preferredColumn = -1;
        private int _preferredRow = -1;

        public static readonly DependencyProperty AnimatedSelectionRectProperty =
            DependencyProperty.Register("AnimatedSelectionRect", typeof(Rect), typeof(CellsInteractionLayer),
                new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty AnimatedActiveCellRectProperty =
            DependencyProperty.Register("AnimatedActiveCellRect", typeof(Rect), typeof(CellsInteractionLayer),
                new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public Rect AnimatedSelectionRect
        {
            get { return (Rect)GetValue(AnimatedSelectionRectProperty); }
            set { SetValue(AnimatedSelectionRectProperty, value); }
        }

        public Rect AnimatedActiveCellRect
        {
            get { return (Rect)GetValue(AnimatedActiveCellRectProperty); }
            set { SetValue(AnimatedActiveCellRectProperty, value); }
        }

        public CellsInteractionLayer(SheetView view) : base(view)
        {

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var hitTest = HitTest();

            if (hitTest == null || hitTest.Row == -1 || hitTest.Column == -1)
                return;

            if (hitTest.CellElement != null)
            {
                if (SheetView.Spread.CellInteractionManager.OnMouseLeftButtonDown(SheetView, hitTest.Row, hitTest.Column, hitTest.CellElement))
                {
                    e.Handled = true;
                    return;
                }
            }

            switch(hitTest.Element)
            {
                case SheetElement.CellElement:
                    break;

                case SheetElement.Cell:
                    // Starts editing
                    if (e.ClickCount == 2)
                    {
                        SheetView.Spread.EditingManager.BeginEdit(SheetView, hitTest.Row, hitTest.Column, EditTrigger.DoubleClick);
                    }
                    else
                    {
                        // End editing if active.
                        if (SheetView.Spread.EditingManager.IsEditing)
                        {
                            if (!SheetView.Spread.EditingManager.EndEdit(true))
                                return;
                        }

                        if (SheetView.Spread.FilterManager.IsFilterDropdownOpen)
                        {
                            SheetView.Spread.FilterManager.HideFilterDropdown();
                        }

                        SheetView.SelectCell(hitTest.Row, hitTest.Column);
                    }
                    break;

                case SheetElement.DragFill:
                    _isDragging = true;
                    break;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            var hitTest = HitTest();

            if (hitTest == null || hitTest.Row == -1 || hitTest.Column == -1)
                return;

            switch (hitTest.Element)
            {
                case SheetElement.Cell:
                    if (SheetView.Spread.EditingManager.IsEditing)
                    {
                        if (!SheetView.Spread.EditingManager.EndEdit(true))
                            return;
                    }

                    if (!SheetView.Selection.ContainsCell(hitTest.Row, hitTest.Column))
                    {
                        SheetView.SelectCell(hitTest.Row, hitTest.Column);
                    }
                    break;
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            var hitTest = HitTest();
            if (hitTest != null && (hitTest.Element == SheetElement.Cell || hitTest.Element == SheetElement.TopLeft))
            {
                SheetView.Spread?.ContextMenuManager?.ShowContextMenu(SheetView, hitTest, this);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isDragging)
            {
                _isDragging = false;
                Cursor = null;
            }

            var hitTest = HitTest();
            SheetView.Spread.CellInteractionManager.OnMouseLeftButtonUp(SheetView, hitTest?.Row ?? -1, hitTest?.Column ?? -1, hitTest?.CellElement);
        }

        private void UpdatePreferredCoordinatesBeforeMove()
        {
            if (SheetView.ActiveRow != _lastActiveRow || SheetView.ActiveColumn != _lastActiveColumn)
            {
                _preferredRow = SheetView.ActiveRow;
                _preferredColumn = SheetView.ActiveColumn;
            }
        }

        private void CommitCoordinatesAfterMove(int conceptuallyRequestedRow, int conceptuallyRequestedColumn)
        {
            _preferredRow = conceptuallyRequestedRow;
            _preferredColumn = conceptuallyRequestedColumn;
            _lastActiveRow = SheetView.ActiveRow;
            _lastActiveColumn = SheetView.ActiveColumn;
        }

        #region Keyboard Selection
        private void MoveDownCellSelection()
        {
            UpdatePreferredCoordinatesBeforeMove();
            var workSheet = SheetView.WorkSheet;
            int rowSpan = Math.Max(1, workSheet.GetRowSpan(SheetView.ActiveRow, SheetView.ActiveColumn));
            int nextRow = SheetView.ActiveRow + rowSpan;

            if (nextRow >= workSheet.RowCount)
                return;

            if (nextRow >= SheetView.ViewPort.ViewRange.BottomRow)
            {
                double renderedRowHeight = SheetView.GetRowRenderedHeight(nextRow);
                var rowRect = SheetView.ViewPort.GetRowRect(nextRow);

                if (renderedRowHeight < rowRect.Height)
                {
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow + rowSpan);
                }
            }

            SheetView.SelectCell(nextRow, _preferredColumn);
            CommitCoordinatesAfterMove(nextRow, _preferredColumn);
        }

        private void MoveUpCellSelection()
        {
            UpdatePreferredCoordinatesBeforeMove();
            if (SheetView.ActiveRow == 0)
                return;

            int nextRow = SheetView.ActiveRow - 1;

            if (nextRow <= SheetView.ViewPort.ViewRange.TopRow)
            {
                double renderedRowHeight = SheetView.GetRowRenderedHeight(nextRow);
                var rowRect = SheetView.ViewPort.GetRowRect(nextRow);

                if (renderedRowHeight < rowRect.Height)
                {
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow - 1);
                }
            }

            SheetView.SelectCell(nextRow, _preferredColumn);
            CommitCoordinatesAfterMove(nextRow, _preferredColumn);
        }

        private void MoveRightCellSelection()
        {
            UpdatePreferredCoordinatesBeforeMove();
            var workSheet = SheetView.WorkSheet;
            int colSpan = Math.Max(1, workSheet.GetColumnSpan(SheetView.ActiveRow, SheetView.ActiveColumn));
            int nextCol = SheetView.ActiveColumn + colSpan;

            if (nextCol >= workSheet.ColumnCount)
                return;

            if (nextCol >= SheetView.ViewPort.ViewRange.RightColumn)
            {
                double renderedColumnWidth = SheetView.GetColumnRenderedWidth(nextCol);
                var colRect = SheetView.ViewPort.GetColumnRect(nextCol);

                if (renderedColumnWidth < colRect.Width)
                {
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn + colSpan);
                }
            }

            SheetView.SelectCell(_preferredRow, nextCol);
            CommitCoordinatesAfterMove(_preferredRow, nextCol);
        }

        private void MoveLeftCellSelection()
        {
            UpdatePreferredCoordinatesBeforeMove();
            if (SheetView.ActiveColumn == 0)
                return;

            int nextCol = SheetView.ActiveColumn - 1;

            if (nextCol <= SheetView.ViewPort.ViewRange.LeftColumn)
            {
                double renderedColumnWidth = SheetView.GetColumnRenderedWidth(nextCol);
                var colRect = SheetView.ViewPort.GetColumnRect(nextCol);

                if (renderedColumnWidth < colRect.Width)
                {
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn - 1);
                }
            }

            SheetView.SelectCell(_preferredRow, nextCol);
            CommitCoordinatesAfterMove(_preferredRow, nextCol);
        }
        #endregion

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            var editingManager = SheetView.Spread.EditingManager.As<EditingManager>();

            if (editingManager.IsEditing && editingManager.ActiveEditor != null)
            {
                if (editingManager.ActiveEditor.HandlesKeyDown(e))
                    return;
            }

            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                if (editingManager.IsEditing)
                    return;
            }

            if (e.Key == Key.Tab && editingManager.IsEditing && !editingManager.IsShowingFormulaSuggestion)
            {
                if (!editingManager.EndEdit(true) && editingManager.ActiveEditor != null)
                {
                    editingManager.ActiveEditorElement?.Focus();
                    return;
                }
            }

            switch (e.Key)
            {
                case Key.F2:
                    if (!editingManager.IsEditing)
                    {
                        e.Handled = true;
                        editingManager.BeginEdit(SheetView, SheetView.ActiveRow, SheetView.ActiveColumn, EditTrigger.F2Key);
                    }
                    break;

                case Key.Escape:
                    if (editingManager.IsEditing)
                    {
                        e.Handled = true;
                        editingManager.EndEdit(false);
                    }
                    break;

                case Key.Down:
                    e.Handled = true;
                    MoveDownCellSelection();
                    break;

                case Key.Up:
                    e.Handled = true;
                    MoveUpCellSelection();
                    break;

                case Key.Right:
                    e.Handled = true;
                    MoveRightCellSelection();
                    break;

                case Key.Left:
                    e.Handled = true;
                    MoveLeftCellSelection();
                    break;

                case Key.Tab:
                    if (!editingManager.IsShowingFormulaSuggestion)
                    {
                        e.Handled = true;
                        MoveRightCellSelection();
                    }
                    break;

                case Key.System:
                case Key.Enter:
                    Key actualKey = e.Key == Key.System ? e.SystemKey : e.Key;
                    if (actualKey == Key.Enter)
                    {
                        e.Handled = true;

                        if (editingManager.IsEditing && !editingManager.EndEdit(true))
                            return;

                        MoveDownCellSelection();
                    }
                    break;

                case Key.Delete:
                    if (editingManager.IsEditing)
                        return;

                    e.Handled = true;
                    SheetView.Spread.SuspendUpdates = true;
                    for (int row = SheetView.Selection.TopRow; row <= SheetView.Selection.BottomRow; row++)
                    {
                        for (int column = SheetView.Selection.LeftColumn; column <= SheetView.Selection.RightColumn; column++)
                        {
                            var ws = (Worksheet)SheetView.WorkSheet;
                            ws.SetValue(row, column, null);
                            ws.SetFormula(row, column, null);
                        }
                    }
                    SheetView.Spread.SuspendUpdates = false;

                    for (int row = SheetView.Selection.TopRow; row <= SheetView.Selection.BottomRow; row++)
                    {
                        SheetView.AutoSizeRow(row);
                    }
                    break;

                case Key.Space:
                    if (editingManager.IsEditing)
                        return;

                    var activeWs = SheetView.WorkSheet as Worksheet;
                    if (activeWs != null)
                    {
                        var activeColItem = ((Columns)activeWs.Columns)?.GetItem(SheetView.ActiveColumn);
                        var activeType = (activeWs.GetCellType(SheetView.ActiveRow, SheetView.ActiveColumn) ?? activeColItem?.CellType) as CheckBoxCellType;
                        if (activeType != null)
                        {
                            e.Handled = true;
                            ToggleSelectedCheckBoxes();
                            return;
                        }
                    }
                    break;

                default:
                    if (editingManager.IsEditing)
                        return;

                    if (e.KeyboardDevice.Modifiers != ModifierKeys.None && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
                        return;

                    if (e.Key == Key.CapsLock || e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                        e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                        e.Key == Key.LWin || e.Key == Key.RWin || e.Key == Key.PageUp || e.Key == Key.PageDown ||
                        e.Key == Key.Home || e.Key == Key.End || e.Key == Key.Insert || e.Key == Key.Scroll ||
                        e.Key == Key.Pause || e.Key == Key.PrintScreen || e.Key == Key.F1 || e.Key == Key.F3 ||
                        e.Key == Key.F4 || e.Key == Key.F5 || e.Key == Key.F6 || e.Key == Key.F7 || e.Key == Key.F8 ||
                        e.Key == Key.F9 || e.Key == Key.F10 || e.Key == Key.F11 || e.Key == Key.F12)
                        return;

                    editingManager.BeginEdit(SheetView, SheetView.ActiveRow, SheetView.ActiveColumn, EditTrigger.DirectTyping);
                    break;
            }
        }

        private void ToggleSelectedCheckBoxes()
        {
            var sheetView = SheetView;
            var worksheet = sheetView?.WorkSheet as Worksheet;
            if (worksheet == null) return;

            var columns = worksheet.Columns as Columns;
            var rows = worksheet.Rows as Rows;
            var selection = sheetView.Selection;

            var compositeAction = new CompositeSheetAction();
            bool anyToggled = false;

            for (int r = selection.TopRow; r <= selection.BottomRow; r++)
            {
                var sheetRow = rows?.GetItem(r);
                for (int c = selection.LeftColumn; c <= selection.RightColumn; c++)
                {
                    var sheetCol = columns?.GetItem(c);
                    bool locked = worksheet.GetLocked(r, c) ||
                        (sheetRow != null && sheetRow.Locked) ||
                        (sheetCol != null && sheetCol.Locked);

                    if (locked) continue;

                    var cellType = (worksheet.GetCellType(r, c) ?? sheetCol?.CellType) as CheckBoxCellType;
                    if (cellType != null)
                    {
                        var startingArgs = new CellEditStartingEventArgs(sheetView, r, c, EditTrigger.DirectTyping);
                        if (sheetView.Spread != null && !sheetView.Spread.RaiseCellEditStarting(startingArgs))
                            continue;

                        object currentValue = worksheet.GetValue(r, c);
                        object nextValue = cellType.GetNextValue(currentValue);

                        var endingArgs = new CellEditEndingEventArgs(sheetView, r, c, nextValue);
                        if (sheetView.Spread != null && !sheetView.Spread.RaiseCellEditEnding(endingArgs))
                        {
                            sheetView.Spread?.RaiseCellEditEnded(new CellEditEndedEventArgs(sheetView, r, c, false));
                            continue;
                        }

                        var action = new CellChangedAction { SheetView = sheetView };
                        action.OldState.Value = currentValue;
                        action.OldState.Row = r;
                        action.OldState.Column = c;
                        action.OldState.Selection = selection.Clone();

                        worksheet.SetValue(r, c, nextValue);

                        action.NewState.Value = worksheet.GetValue(r, c);
                        action.NewState.Row = r;
                        action.NewState.Column = c;
                        action.NewState.Selection = selection.Clone();

                        compositeAction.AddAction(action);
                        sheetView.Spread?.RaiseCellEditEnded(new CellEditEndedEventArgs(sheetView, r, c, true));
                        anyToggled = true;
                    }
                }
            }

            if (anyToggled)
            {
                sheetView.Spread?.UndoRedoManager?.AddAction(compositeAction);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SheetView.Spread.EditingManager.IsEditing)
            {
                SheetView.Spread.CellInteractionManager.ClearState(SheetView);
                return;
            }

            if (_scrolling)
                return;

            var hitTest = HitTest();

            if (hitTest == null)
            {
                SheetView.Spread.CellInteractionManager.ClearState(SheetView);

                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                Dispatcher.Invoke(new Action(async () =>
                {
                    _scrolling = true;
                    await SelectiveMouseScroll();
                    _scrolling = false;
                }));
            }
            else
            {
                var cellElement = hitTest.CellElement;
                bool isElementHovered = cellElement != null;
                SheetView.Spread.CellInteractionManager.UpdateHover(SheetView, hitTest.Row, hitTest.Column, cellElement);

                if (hitTest.Element == SheetElement.DragFill || _isDragging)
                {
                    Cursor = SheetUtils.DragFillCursor;
                }
                else if (isElementHovered)
                {
                    Cursor = cellElement.Cursor;
                }
                else
                {
                    Cursor = null;
                }
 
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                if (hitTest.Row == -1 || hitTest.Column == -1)
                    return;

                SelectRange(hitTest);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            SheetView.Spread.CellInteractionManager.OnMouseLeave(SheetView);
        }

        private async Task SelectiveMouseScroll()
        {
            SpreadHitTestResult hitTest = null;
            int xSpeed = 1, ySpeed = 1;

            do
            {
                var position = Mouse.GetPosition(this);

                bool up = position.Y < 0;
                bool down = position.Y > SheetView.ViewPort.ActualBounds.Height;
                bool right = position.X > SheetView.ViewPort.ActualBounds.Width;
                bool left = position.X < 0;

                if (down && right)
                {
                    hitTest = HitTest(new Point(SheetView.ViewPort.ActualBounds.Width - 5, SheetView.ViewPort.ActualBounds.Height - 5));
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow + 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn + 1 * xSpeed);
                }
                else if (up && right)
                {
                    hitTest = HitTest(new Point(SheetView.ViewPort.ActualBounds.Width - 5, 0));
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow - 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn + 1 * xSpeed);
                }
                else if (left && up)
                {
                    hitTest = HitTest(new Point(SheetView.GetRowHeaderWidth() + 5, SheetView.GetColumnHeaderHeight() + 5));
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow - 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn - 1 * xSpeed);
                }
                else if (left && down)
                {
                    hitTest = HitTest(new Point(0, SheetView.ViewPort.ActualBounds.Height - 5));
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow + 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn - 1 * xSpeed);
                }
                else if (up)
                {
                    hitTest = HitTest(new Point(position.X, 0));
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow - 1 * ySpeed);
                }
                else if (down)
                {
                    var workSheet = SheetView.WorkSheet;
                    hitTest = HitTest(new Point(position.X, SheetView.ViewPort.ActualBounds.Height - 5));
                    SheetView.Spread.ScrollToRow(SheetView.ViewPort.ViewRange.TopRow + 1 * ySpeed);
                    var bottomRow = SheetView.ViewPort.ViewRange.BottomRow;
                    var renderedHeight = SheetView.GetRowRenderedHeight(bottomRow);
                    var actualHeight = workSheet.Rows.GetRowHeight(bottomRow);
                    hitTest.Row = actualHeight == renderedHeight ? bottomRow : bottomRow - 1;
                }
                else if (left)
                {
                    hitTest = HitTest(new Point(0, position.Y));
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn - 1 * xSpeed);
                }
                else if (right)
                {
                    hitTest = HitTest(new Point(SheetView.ViewPort.ActualBounds.Width - 5, position.Y));
                    SheetView.Spread.ScrollToColumn(SheetView.ViewPort.ViewRange.LeftColumn + 1 * xSpeed);
                }
                else
                {
                    break;
                }

                SelectRange(hitTest);
                await Task.Delay(1);

            }while (IsMouseCaptured);
        }

        private void SelectRange(SpreadHitTestResult hitTest)
        {
            if (hitTest == null)
                return;

            if (SheetView.SelectionMode == SelectionMode.Cell ||
                SheetView.SelectionMode == SelectionMode.Row ||
                SheetView.SelectionMode == SelectionMode.Column)
                return;

            if (_isDragging)
            {
                
            }
            else
            {
                int topRow = Math.Min(hitTest.Row, SheetView.ActiveRow);
                int leftColumn = Math.Min(hitTest.Column, SheetView.ActiveColumn);
                int bottomRow = Math.Max(hitTest.Row, SheetView.ActiveRow);
                int rightColumn = Math.Max(hitTest.Column, SheetView.ActiveColumn);
                SheetView.SelectRange(topRow, leftColumn, bottomRow + 1 - topRow, rightColumn + 1 - leftColumn);
            }
        }

        public void UpdateSelectionRects()
        {
            if (SheetView == null) return;
            
            var targetSelectionAbsolute = SheetView.ViewPort.GetRangeRect(SheetView.Selection);
            var targetActiveCellAbsolute = SheetView.ViewPort.GetCellRect(SheetView.ActiveRow, SheetView.ActiveColumn);

            if (_isFirstRender || !SheetView.Spread.IsSelectionAnimationEnabled)
            {
                AnimatedSelectionRect = targetSelectionAbsolute;
                AnimatedActiveCellRect = targetActiveCellAbsolute;
                _targetSelectionRect = targetSelectionAbsolute;
                _targetActiveCellRect = targetActiveCellAbsolute;
                _isFirstRender = false;
                
                BeginAnimation(AnimatedSelectionRectProperty, null);
                BeginAnimation(AnimatedActiveCellRectProperty, null);
            }
            else if (_targetSelectionRect != targetSelectionAbsolute || _targetActiveCellRect != targetActiveCellAbsolute)
            {
                _targetSelectionRect = targetSelectionAbsolute;
                _targetActiveCellRect = targetActiveCellAbsolute;

                var duration = new Duration(TimeSpan.FromMilliseconds(150));
                var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                
                var selAnim = new RectAnimation(targetSelectionAbsolute, duration) { EasingFunction = ease };
                var actAnim = new RectAnimation(targetActiveCellAbsolute, duration) { EasingFunction = ease };

                Timeline.SetDesiredFrameRate(selAnim, 60);
                Timeline.SetDesiredFrameRate(actAnim, 60);

                BeginAnimation(AnimatedSelectionRectProperty, selAnim, HandoffBehavior.SnapshotAndReplace);
                BeginAnimation(AnimatedActiveCellRectProperty, actAnim, HandoffBehavior.SnapshotAndReplace);
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (SheetView == null)
                return;

            RenderContext context = new RenderContext(dc, SheetView);
            DrawCellElements(context);

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var workSheet = SheetView.WorkSheet;

            var targetSelectionAbsolute = SheetView.ViewPort.GetRangeRect(SheetView.Selection);
            var targetActiveCellAbsolute = SheetView.ViewPort.GetCellRect(SheetView.ActiveRow, SheetView.ActiveColumn);

            var currentSelectionAbsolute = AnimatedSelectionRect;
            var currentActiveCellAbsolute = AnimatedActiveCellRect;

            if (currentSelectionAbsolute.IsEmpty || currentSelectionAbsolute.Width <= 0 || currentSelectionAbsolute.Height <= 0)
                currentSelectionAbsolute = targetSelectionAbsolute;
            if (currentActiveCellAbsolute.IsEmpty || currentActiveCellAbsolute.Width <= 0 || currentActiveCellAbsolute.Height <= 0)
                currentActiveCellAbsolute = targetActiveCellAbsolute;

            var unscaledSelection = ToSheetViewRect(currentSelectionAbsolute);
            var selectionRangeRect = new Rect(
                unscaledSelection.X * zoom - 1,
                unscaledSelection.Y * zoom - 0.5,
                unscaledSelection.Width * zoom + 1,
                unscaledSelection.Height * zoom + 1);

            var unscaledActive = ToSheetViewRect(currentActiveCellAbsolute);
            var activeCellRect = new Rect(
                unscaledActive.X * zoom,
                unscaledActive.Y * zoom,
                unscaledActive.Width * zoom,
                unscaledActive.Height * zoom);

            double dpi = SheetUtils.PixelPerDip > 0 ? SheetUtils.PixelPerDip : 1.0;
            double borderPenThickness = SheetView.Spread.SelectionBorderPen != null ? SheetView.Spread.SelectionBorderPen.Thickness : 1.0;
            double selLeft = Rendering.Text.PixelSnapper.SnapLine(selectionRangeRect.Left, dpi, borderPenThickness);
            double selTop = Rendering.Text.PixelSnapper.SnapLine(selectionRangeRect.Top, dpi, borderPenThickness);
            double selRight = Rendering.Text.PixelSnapper.SnapLine(selectionRangeRect.Right, dpi, borderPenThickness);
            double selBottom = Rendering.Text.PixelSnapper.SnapLine(selectionRangeRect.Bottom, dpi, borderPenThickness);
            selectionRangeRect = new Rect(selLeft, selTop, selRight - selLeft, selBottom - selTop);

            var borderGeometry = new StreamGeometry();
            using (var ctx = borderGeometry.Open())
            {
                ctx.BeginFigure(new Point(selectionRangeRect.BottomRight.X, selectionRangeRect.BottomRight.Y - 3), false, false);
                ctx.LineTo(selectionRangeRect.TopRight, true, true);
                ctx.LineTo(selectionRangeRect.TopLeft, true, true);
                ctx.LineTo(selectionRangeRect.BottomLeft, true, true);
                ctx.LineTo(new Point(selectionRangeRect.BottomRight.X - 3, selectionRangeRect.BottomRight.Y), true, true);
            }
            context.DrawGeometry(null, SheetView.Spread.SelectionBorderPen, borderGeometry);

            if (!AreClose(activeCellRect, selectionRangeRect))
            {
                double margin = 1.5;
                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(new PathFigure(new Point(selectionRangeRect.Left + margin, selectionRangeRect.Top + margin), 
                    GetSelectionBackgroundSegments(selectionRangeRect, activeCellRect), true));
                context.DrawGeometry(SheetView.Spread.SelectionBackground, null, pathGeometry);
            }
            else
            {
                // If editing is active then update editor location
                if (SheetView.Spread.EditingManager.ActiveEditorElement != null)
                {
                    SetLeft(SheetView.Spread.EditingManager.ActiveEditorElement, activeCellRect.Left + 1);
                    SetTop(SheetView.Spread.EditingManager.ActiveEditorElement, activeCellRect.Top + 1);
                }
            }

            var handleRect = new Rect(selectionRangeRect.BottomRight.X - 3, selectionRangeRect.BottomRight.Y - 3, 6, 6);

            // Draw a solid square handle using the border pen's brush
            context.DrawRectangle(SheetView.Spread.SelectionBorderPen.Brush, null, handleRect);

            // Give the handle a subtle white border so it pops out over the grid lines
            context.DrawRectangle(null, WpfResourceCache.GetPen(Brushes.White, 1), handleRect);

            context.Dispose();
        }

        private void DrawCellElements(RenderContext context)
        {
            var cellInteractionManager = SheetView.Spread?.CellInteractionManager;
            if (cellInteractionManager == null) return;

            var viewRange = context.ViewPort.ViewRange;

            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
                {
                    var elements = cellInteractionManager.GetCellElements(SheetView, row, col);
                    Rect? scaledCellRect = null;

                    foreach (var element in elements)
                    {
                        if (!scaledCellRect.HasValue)
                        {
                            scaledCellRect = context.GetCellRect(row, col);
                        }

                        var bounds = element.GetBounds(scaledCellRect.Value, context.ZoomFactor);
                        var state = cellInteractionManager.GetElementState(row, col, element);

                        element.Draw(context, bounds, state, row, col);
                    }
                }
            }
        }

        private bool AreClose(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) < 1.0 && Math.Abs(p1.Y - p2.Y) < 1.0;
        }

        private bool AreClose(Rect r1, Rect r2)
        {
            return Math.Abs(r1.X - r2.X) < 1.0 && Math.Abs(r1.Y - r2.Y) < 1.0 && 
                   Math.Abs(r1.Width - r2.Width) < 1.0 && Math.Abs(r1.Height - r2.Height) < 1.0;
        }

        private IEnumerable<PathSegment> GetSelectionBackgroundSegments(Rect selectionRect, Rect activeCellRect)
        {
            if(AreClose(selectionRect.TopLeft, activeCellRect.TopLeft))
            {
                yield return new LineSegment(activeCellRect.TopRight, false);
                yield return new LineSegment(selectionRect.TopRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.TopRight, false);
            }
            else if(AreClose(selectionRect.TopRight, activeCellRect.TopRight))
            {
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(activeCellRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomLeft, false);
                yield return new LineSegment(selectionRect.TopLeft, false);
            }
            else if(AreClose(selectionRect.BottomLeft, activeCellRect.BottomLeft))
            {
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(selectionRect.TopRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.TopRight, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(selectionRect.TopLeft, false);
            }
            else if(AreClose(selectionRect.BottomRight, activeCellRect.BottomRight))
            {
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(selectionRect.TopRight, false);
                yield return new LineSegment(activeCellRect.TopRight, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(activeCellRect.BottomLeft, false);
                yield return new LineSegment(selectionRect.BottomLeft, false);
                yield return new LineSegment(selectionRect.TopLeft, false);
            }
            else
            {
                Point endingPoint = new Point(activeCellRect.TopLeft.X, selectionRect.TopLeft.Y);
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(selectionRect.TopRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomLeft, false);
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(endingPoint, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(activeCellRect.TopRight, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(endingPoint, false);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth + 0.5, ActualHeight + 0.5));
        }
    }
}