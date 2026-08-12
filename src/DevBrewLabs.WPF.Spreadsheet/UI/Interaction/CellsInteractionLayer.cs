using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Utils;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal class CellsInteractionLayer : InteractionLayer
    {
        private bool _scrolling = false;
        private bool _isDragging = false;

        public CellsInteractionLayer()
        {

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var hitTest = HitTest();

            if (hitTest == null || hitTest.Row == -1 || hitTest.Column == -1)
                return;

            switch(hitTest.Element)
            {
                case VisualElement.Cell:
                    // Starts editing
                    if (e.ClickCount == 2)
                    {
                        SheetView.Spread.EditingManager.BeginEdit(hitTest.Row, hitTest.Column);
                    }
                    else
                    {
                        // End editing if active.
                        if (SheetView.Spread.EditingManager.IsEditing)
                        {
                            if (!SheetView.Spread.EditingManager.EndEdit(true))
                                return;
                        }

                        SheetView.Spread.SelectionManager.SelectCell(hitTest.Row, hitTest.Column);
                    }
                    break;

                case VisualElement.DragFill:
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
                case VisualElement.Cell:
                    if (SheetView.Spread.EditingManager.IsEditing)
                    {
                        if (!SheetView.Spread.EditingManager.EndEdit(true))
                            return;
                    }

                    if (!SheetView.Selection.ContainsCell(hitTest.Row, hitTest.Column))
                    {
                        SheetView.Spread.SelectionManager.SelectCell(hitTest.Row, hitTest.Column);
                    }
                    break;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if(_isDragging)
            {
                _isDragging = false;
                Cursor = null;
            }
        }

        #region Keyboard Selection
        private void MoveDownCellSelection()
        {
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
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow + rowSpan);
                }
            }

            SheetView.Spread.SelectionManager.SelectCell(nextRow, SheetView.ActiveColumn);
        }

        private void MoveUpCellSelection()
        {
            if (SheetView.ActiveRow == 0)
                return;

            if (SheetView.ActiveRow - 1 <= SheetView.ViewPort.ViewRange.TopRow)
            {
                double renderedRowHeight = SheetView.GetRowRenderedHeight(SheetView.ActiveRow - 1);
                var rowRect = SheetView.ViewPort.GetRowRect(SheetView.ActiveRow - 1);

                if (renderedRowHeight < rowRect.Height)
                {
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow - 1);
                }
            }

            SheetView.Spread.SelectionManager.SelectCell(SheetView.ActiveRow - 1, SheetView.ActiveColumn);
        }

        private void MoveRightCellSelection()
        {
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
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn + colSpan);
                }
            }

            SheetView.Spread.SelectionManager.SelectCell(SheetView.ActiveRow, nextCol);
        }

        private void MoveLeftCellSelection()
        {
            if (SheetView.ActiveColumn == 0)
                return;

            if (SheetView.ActiveColumn - 1 <= SheetView.ViewPort.ViewRange.LeftColumn)
            {
                double renderedColumnWidth = SheetView.GetColumnRenderedWidth(SheetView.ActiveColumn - 1);
                var colRect = SheetView.ViewPort.GetColumnRect(SheetView.ActiveColumn - 1);

                if (renderedColumnWidth < colRect.Width)
                {
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn - 1);
                }
            }

            SheetView.Spread.SelectionManager.SelectCell(SheetView.ActiveRow, SheetView.ActiveColumn - 1);
        }
        #endregion

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            var editingManager = SheetView.Spread.EditingManager.As<EditingManager>();
            
            if(e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                if (editingManager.IsEditing)
                    return;

                //if (!editingManager.EndEdit(true) && editingManager.ActiveEditor != null)
                //{
                //    editingManager.ActiveEditor.Focus();
                //    return;
                //}
            }

            if(e.Key == Key.Tab && editingManager.IsEditing && !TextEditor.IsShowingFormulaSuggestion)
            {
                if (!editingManager.EndEdit(true) && editingManager.ActiveEditor != null)
                {
                    editingManager.ActiveEditor.Focus();
                    return;
                }
            }

            switch (e.Key)
            {
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
                    if (!TextEditor.IsShowingFormulaSuggestion)
                    {
                        e.Handled = true;
                        MoveRightCellSelection();
                    }
                    break;

                case Key.System:
                case Key.Enter:
                    Key actualKey = e.Key == Key.System ? e.SystemKey : e.Key;
                    if (actualKey == Key.Enter && ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
                    {
                        if (editingManager.IsEditing && editingManager.ActiveEditor is TextEditor textBox)
                        {
                            if (!textBox.AcceptsReturn)
                                return;

                            e.Handled = true;
                            int caretIndex = textBox.CaretIndex;
                            string currentText = textBox.Text ?? string.Empty;
                            if (textBox.SelectionLength > 0)
                            {
                                currentText = currentText.Remove(textBox.SelectionStart, textBox.SelectionLength);
                                caretIndex = textBox.SelectionStart;
                            }
                            textBox.Text = currentText.Insert(caretIndex, Environment.NewLine);
                            textBox.CaretIndex = caretIndex + Environment.NewLine.Length;

                            var cellRect = SheetView.ViewPort.GetCellRect(textBox.Row, textBox.Column);
                            int lineCount = TextUtils.GetLineCount(textBox.Text);
                            double fontLineHeight = textBox.FontSize * 1.3;
                            double requiredHeight = Math.Max(cellRect.Height - 3, lineCount * fontLineHeight + 6);
                            textBox.Height = requiredHeight;
                        }
                        return;
                    }

                    if (actualKey == Key.Enter)
                    {
                        e.Handled = true;

                        if (editingManager.IsEditing && !editingManager.EndEdit(true))
                            return;

                        MoveDownCellSelection();
                    }
                    break;

                case Key.Delete:
                    e.Handled = true;
                    SheetView.Spread.SuspendUpdates = true;
                    for (int row = SheetView.Selection.TopRow; row <= SheetView.Selection.BottomRow; row++)
                    {
                        for (int column = SheetView.Selection.LeftColumn; column <= SheetView.Selection.RightColumn; column++)
                        {
                            var ws = (WorkSheet)SheetView.WorkSheet;
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

                default:
                    if (e.KeyboardDevice.Modifiers != ModifierKeys.None || e.Key == Key.CapsLock)
                        return;

                    editingManager.UseCellValue = false;
                    editingManager.BeginEdit(SheetView.ActiveRow, SheetView.ActiveColumn);
                    editingManager.UseCellValue = true;
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SheetView.Spread.EditingManager.IsEditing)
            {
                return;
            }

            if (SheetView.SelectionMode == SelectionMode.Cell ||
                SheetView.SelectionMode == SelectionMode.Row || SheetView.SelectionMode == SelectionMode.Column)
                return;

            if (_scrolling)
                return;

            var hitTest = HitTest();

            if (hitTest == null)
            {
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
                if (hitTest.Element == VisualElement.DragFill || _isDragging)
                    Cursor = SheetUtils.DragFillCursor;
                else if (hitTest.Element == VisualElement.Cell)
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
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow + 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn + 1 * xSpeed);
                }
                else if (up && right)
                {
                    hitTest = HitTest(new Point(SheetView.ViewPort.ActualBounds.Width - 5, 0));
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow - 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn + 1 * xSpeed);
                }
                else if (left && up)
                {
                    hitTest = HitTest(new Point(SheetView.GetRowHeaderWidth() + 5, SheetView.GetColumnHeaderHeight() + 5));
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow - 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn - 1 * xSpeed);
                }
                else if (left && down)
                {
                    hitTest = HitTest(new Point(0, SheetView.ViewPort.ActualBounds.Height - 5));
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow + 1 * ySpeed);
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn - 1 * xSpeed);
                }
                else if (up)
                {
                    hitTest = HitTest(new Point(position.X, 0));
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow - 1 * ySpeed);
                }
                else if (down)
                {
                    var workSheet = SheetView.WorkSheet;
                    hitTest = HitTest(new Point(position.X, SheetView.ViewPort.ActualBounds.Height - 5));
                    SheetView.Spread.ScrollToRow(SheetView, SheetView.ViewPort.ViewRange.TopRow + 1 * ySpeed);
                    var bottomRow = SheetView.ViewPort.ViewRange.BottomRow;
                    var renderedHeight = SheetView.GetRowRenderedHeight(bottomRow);
                    var actualHeight = workSheet.Rows.GetRowHeight(bottomRow);
                    hitTest.Row = actualHeight == renderedHeight ? bottomRow : bottomRow - 1;
                }
                else if (left)
                {
                    hitTest = HitTest(new Point(0, position.Y));
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn - 1 * xSpeed);
                }
                else if (right)
                {
                    hitTest = HitTest(new Point(SheetView.ViewPort.ActualBounds.Width - 5, position.Y));
                    SheetView.Spread.ScrollToColumn(SheetView, SheetView.ViewPort.ViewRange.LeftColumn + 1 * xSpeed);
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

            if (_isDragging)
            {
                
            }
            else
            {
                int topRow = Math.Min(hitTest.Row, SheetView.ActiveRow);
                int leftColumn = Math.Min(hitTest.Column, SheetView.ActiveColumn);
                int bottomRow = Math.Max(hitTest.Row, SheetView.ActiveRow);
                int rightColumn = Math.Max(hitTest.Column, SheetView.ActiveColumn);
                SheetView.Spread.SelectionManager.SelectRange(topRow, leftColumn, bottomRow + 1 - topRow, rightColumn + 1 - leftColumn);
            }
        }

        public void AddFilterTools(CellRange cellRange)
        {
            for(int col = cellRange.LeftColumn; col <= cellRange.RightColumn; col++)
            {
                
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (SheetView == null)
                return;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var workSheet = SheetView.WorkSheet;

            var unscaledSelection = ToSheetViewRect(SheetView.ViewPort.GetRangeRect(SheetView.Selection));
            var selectionRangeRect = new Rect(
                unscaledSelection.X * zoom - 1,
                unscaledSelection.Y * zoom - 0.5,
                unscaledSelection.Width * zoom + 1,
                unscaledSelection.Height * zoom + 1);

            var unscaledActive = ToSheetViewRect(SheetView.ViewPort.GetCellRect(SheetView.ActiveRow, SheetView.ActiveColumn));
            var activeCellRect = new Rect(
                unscaledActive.X * zoom,
                unscaledActive.Y * zoom,
                unscaledActive.Width * zoom,
                unscaledActive.Height * zoom);

            double halfPenWidth = SheetView.Spread.GridLinePen.Thickness / 2;
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesY.Add(selectionRangeRect.TopRight.Y + halfPenWidth);
            guidelines.GuidelinesY.Add(selectionRangeRect.BottomRight.Y + halfPenWidth);
            guidelines.GuidelinesX.Add(selectionRangeRect.BottomLeft.X + halfPenWidth);
            guidelines.GuidelinesX.Add(selectionRangeRect.BottomRight.X + halfPenWidth);

            dc.PushGuidelineSet(guidelines);
            dc.DrawLine(SheetView.Spread.SelectionBorderPen, selectionRangeRect.TopLeft, selectionRangeRect.TopRight);
            dc.DrawLine(SheetView.Spread.SelectionBorderPen, selectionRangeRect.TopRight,
                new Point(selectionRangeRect.BottomRight.X, selectionRangeRect.BottomRight.Y - 4.5));
            dc.DrawLine(SheetView.Spread.SelectionBorderPen, selectionRangeRect.BottomLeft, selectionRangeRect.TopLeft);
            dc.DrawLine(SheetView.Spread.SelectionBorderPen, selectionRangeRect.BottomLeft,
                new Point(selectionRangeRect.BottomRight.X - 4.5, selectionRangeRect.BottomRight.Y));

            if (activeCellRect != selectionRangeRect)
            {
                double margin = 1.5;
                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(new PathFigure(new Point(selectionRangeRect.Left + margin, selectionRangeRect.Top + margin), 
                    GetSelectionBackgroundSegments(selectionRangeRect, activeCellRect), true));
                dc.DrawGeometry(SheetView.Spread.SelectionBackground, null, pathGeometry);
            }
            else
            {
                // If editing is active then update editor location
                if(SheetView.Spread.EditingManager.ActiveEditor != null)
                {
                    SetLeft(SheetView.Spread.EditingManager.ActiveEditor, activeCellRect.Left + 1);
                    SetTop(SheetView.Spread.EditingManager.ActiveEditor, activeCellRect.Top + 1);
                }
            }

            selectionRangeRect.X = selectionRangeRect.BottomRight.X - 1.5;
            selectionRangeRect.Y = selectionRangeRect.BottomRight.Y - 1.5;
            selectionRangeRect.Width = 2;
            selectionRangeRect.Height = 2;

            dc.DrawRectangle(null, SheetView.Spread.SelectionBorderPen, selectionRangeRect);
            dc.Pop();
        }

        private IEnumerable<PathSegment> GetSelectionBackgroundSegments(Rect selectionRect, Rect activeCellRect)
        {
            if(selectionRect.TopLeft == activeCellRect.TopLeft)
            {
                yield return new LineSegment(activeCellRect.TopRight, false);
                yield return new LineSegment(selectionRect.TopRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.TopRight, false);
            }
            else if(selectionRect.TopRight == activeCellRect.TopRight)
            {
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(activeCellRect.BottomLeft, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(selectionRect.BottomLeft, false);
                yield return new LineSegment(selectionRect.TopLeft, false);
            }
            else if(selectionRect.BottomLeft == activeCellRect.BottomLeft)
            {
                yield return new LineSegment(selectionRect.TopLeft, false);
                yield return new LineSegment(selectionRect.TopRight, false);
                yield return new LineSegment(selectionRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.BottomRight, false);
                yield return new LineSegment(activeCellRect.TopRight, false);
                yield return new LineSegment(activeCellRect.TopLeft, false);
                yield return new LineSegment(selectionRect.TopLeft, false);
            }
            else if(selectionRect.BottomRight == activeCellRect.BottomRight)
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