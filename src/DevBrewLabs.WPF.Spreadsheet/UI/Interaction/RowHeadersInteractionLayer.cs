using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal class RowHeadersInteractionLayer : InteractionLayer
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var hitTest = HitTest();

            if (hitTest.Element == VisualElement.RowHeaderResizeBar && SheetView.Spread.AllowRowResize)
            {
                if (e.ClickCount == 2)
                {
                    SheetView.AutoSizeRow(hitTest.Row);
                    SheetView.ViewPort.As<ViewPort>().CalculateVisibleRange();
                    SheetView.Spread.Invalidate();
                    return;
                }

                CaptureMouse();
                SheetView.Spread.RowResizeManager.BeginResize(SheetView, hitTest.Row, (int)hitTest.Position.Y);
                Children.Add(SheetView.Spread.RowResizeManager.ResizeLine);
            }
            else
            {
                if (SheetView.Spread.EditingManager.IsEditing)
                {
                    if (!SheetView.Spread.EditingManager.EndEdit(true))
                        return;
                }

                SheetView.SelectRow(hitTest.Row);
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            var hitTest = HitTest();

            if (SheetView.Spread.EditingManager.IsEditing)
            {
                if (!SheetView.Spread.EditingManager.EndEdit(true))
                    return;
            }

            if(SheetView.Selection.RowCount <= 1)
                SheetView.SelectRow(hitTest.Row);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            var hitTest = HitTest();

            if (SheetView.Spread.RowResizeManager.IsResizing)
            {
                SheetView.Spread.RowResizeManager.EndResize(SheetView);
                Children.Remove(SheetView.Spread.RowResizeManager.ResizeLine);
                ReleaseMouseCapture();
            }

            if (hitTest != null && hitTest.Element != VisualElement.RowHeaderResizeBar)
                Cursor = SheetUtils.RowHeaderCursor;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SheetView.Spread.RowResizeManager.IsResizing)
            {
                SheetView.Spread.RowResizeManager.Resize(SheetView, (int)e.GetPosition(this).Y);
                return;
            }

            var hitTest = HitTest();

            if (hitTest == null)
                return;

            if (hitTest.Element == VisualElement.RowHeaderResizeBar && SheetView.Spread.AllowRowResize)
            {
                Cursor = SheetUtils.RowResizeCursor;
            }
            else
            {
                Cursor = SheetUtils.RowHeaderCursor;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            int topRow = Math.Min(hitTest.Row, SheetView.ActiveRow);
            int bottomRow = Math.Max(hitTest.Row, SheetView.ActiveRow);
            SheetView.SelectRows(topRow, bottomRow - topRow + 1);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (SheetView == null)
                return;

            double zoom = SheetView.ZoomFactor > 0 ? SheetView.ZoomFactor : 1.0;
            var selectionRangeRect = ToSheetViewRect(SheetView.ViewPort.GetRangeRect(SheetView.Selection));
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth + 0.5, ActualHeight)));
            dc.DrawLine(SheetView.Spread.SelectionBorderPen,
                new Point(ActualWidth, selectionRangeRect.Top * zoom),
                new Point(ActualWidth, selectionRangeRect.Bottom * zoom));
            dc.Pop();
        }
    }
}
