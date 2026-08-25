using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal class ColumnHeadersInteractionLayer : InteractionLayer
    {
        public ColumnHeadersInteractionLayer(SheetView view) : base(view)
        {
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var hitTest = HitTest();

            if (hitTest.Element == SheetElement.ColumnHeaderResizeBar && SheetView.Spread.AllowColumnResize)
            {
                CaptureMouse();
                SheetView.Spread.ColumnResizeManager.BeginResize(SheetView, hitTest.Column, (int)hitTest.Position.X);
                Children.Add(SheetView.Spread.ColumnResizeManager.ResizeLine);
            }
            else
            {
                if (SheetView.Spread.EditingManager.IsEditing)
                {
                    if (!SheetView.Spread.EditingManager.EndEdit(true))
                        return;
                }
                SheetView.SelectColumn(hitTest.Column);
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            var hitTest = HitTest();

            if (hitTest == null || hitTest.Column == -1)
                return;

            if (SheetView.Spread.EditingManager.IsEditing)
            {
                if (!SheetView.Spread.EditingManager.EndEdit(true))
                    return;
            }

            if (!SheetView.Selection.ContainsColumn(hitTest.Column) || SheetView.Selection.RowCount < SheetView.WorkSheet.RowCount)
                SheetView.SelectColumn(hitTest.Column);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            var hitTest = HitTest();
            if (hitTest != null && (hitTest.Element == SheetElement.ColumnHeader || hitTest.Element == SheetElement.ColumnHeaderResizeBar))
            {
                SheetView.Spread?.ContextMenuManager?.ShowContextMenu(SheetView, hitTest, this);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            var hitTest = HitTest();

            if (SheetView.Spread.ColumnResizeManager.IsResizing)
            {
                SheetView.Spread.ColumnResizeManager.EndResize(SheetView);
                Children.Remove(SheetView.Spread.ColumnResizeManager.ResizeLine);
                SheetView.Spread.UpdateScrollbars();
                ReleaseMouseCapture();
            }

            if (hitTest != null && hitTest.Element != SheetElement.ColumnHeaderResizeBar)
                Cursor = SheetUtils.ColumnHeaderCursor;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SheetView.Spread.ColumnResizeManager.IsResizing)
            {
                ClearHover();
                SheetView.Spread.ColumnResizeManager.Resize(SheetView, (int)e.GetPosition(this).X);
                return;
            }

            var hitTest = HitTest();

            if (hitTest == null)
            {
                ClearHover();
                return;
            }

            if (hitTest.Element == SheetElement.ColumnHeaderResizeBar && SheetView.Spread.AllowColumnResize)
            {
                Cursor = SheetUtils.ColumnResizeCursor;
            }
            else
            {
                Cursor = SheetUtils.ColumnHeaderCursor;
            }

            int newHover = (hitTest.Element == SheetElement.ColumnHeader) ? hitTest.Column : -1;
            SheetView?.Spread?.HeaderHoverManager?.SetHoveredColumn(SheetView, newHover);

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            int leftColumn = Math.Min(hitTest.Column, SheetView.ActiveColumn);
            int rightColumn = Math.Max(hitTest.Column, SheetView.ActiveColumn);
            SheetView.SelectColumns(leftColumn, rightColumn - leftColumn + 1);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            ClearHover();
        }

        private void ClearHover()
        {
            SheetView?.Spread?.HeaderHoverManager?.ClearHoveredColumn(SheetView);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (SheetView == null)
                return;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var selectionRangeRect = ToSheetViewRect(SheetView.ViewPort.GetRangeRect(SheetView.Selection));
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight + 0.5)));
            dc.DrawLine(SheetView.Spread.SelectionBorderPen,
                new Point(selectionRangeRect.Left * zoom, ActualHeight), 
                new Point(selectionRangeRect.Right * zoom, ActualHeight));
            dc.Pop();
        }
    }
}
