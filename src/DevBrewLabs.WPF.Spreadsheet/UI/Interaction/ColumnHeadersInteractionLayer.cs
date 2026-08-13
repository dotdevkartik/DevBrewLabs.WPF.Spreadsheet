using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal class ColumnHeadersInteractionLayer : InteractionLayer
    {
        private ColumnResizeManager _resizeManager;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var hitTest = HitTest();

            if (hitTest.Element == VisualElement.ColumnHeaderResizeBar && SheetView.Spread.AllowColumnResize)
            {
                if (e.ClickCount == 2)
                {
                    SheetView.AutoSizeColumn(hitTest.Column);
                    return;
                }

                _resizeManager.BeginResizeColumn(SheetView, hitTest.Column, (int)hitTest.Position.X);
                Children.Add(_resizeManager.ResizeLine);
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

            if (SheetView.Spread.EditingManager.IsEditing)
            {
                if (!SheetView.Spread.EditingManager.EndEdit(true))
                    return;
            }

            if (SheetView.Selection.ColumnCount <= 1)
                SheetView.SelectColumn(hitTest.Column);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            var hitTest = HitTest();

            if (_resizeManager.IsResizing)
            {
                _resizeManager.EndResizeColumn(SheetView);
                Children.Remove(_resizeManager.ResizeLine);
                SheetView.Spread.SheetTabControl.UpdateScrollbars();
            }

            if(hitTest != null && hitTest.Element != VisualElement.ColumnHeaderResizeBar)
                Cursor = SheetUtils.ColumnHeaderCursor;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(_resizeManager.IsResizing)
            {
                _resizeManager.ResizeColumn(SheetView, (int)e.GetPosition(this).X);
                return;
            }

            var hitTest = HitTest();

            if (hitTest == null)
                return;

            if(hitTest.Element == VisualElement.ColumnHeaderResizeBar && SheetView.Spread.AllowColumnResize)
            {
                Cursor = SheetUtils.ColumnResizeCursor;
            }
            else
            {
                Cursor = SheetUtils.ColumnHeaderCursor;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            int leftColumn = Math.Min(hitTest.Column, SheetView.ActiveColumn);
            int rightColumn = Math.Max(hitTest.Column, SheetView.ActiveColumn);
            SheetView.SelectColumns(leftColumn, rightColumn - leftColumn + 1);
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

        public override void AttachToRegion(SheetViewSurface region)
        {
            base.AttachToRegion(region);

            if (_resizeManager == null)
                _resizeManager = new ColumnResizeManager(region.SheetView.Spread);
        }

        public override void DetachFromRegion()
        {
            base.DetachFromRegion();
            if (_resizeManager != null)
            {
                _resizeManager.Dispose();
                _resizeManager = null;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            //Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight + 0.5));
        }
    }
}
