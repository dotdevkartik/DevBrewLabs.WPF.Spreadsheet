using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Interaction
{
    internal class TopLeftInteractionLayer : InteractionLayer
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var hitTest = HitTest();

            if(hitTest != null && hitTest.Element == VisualElement.TopLeft)
            {
                var workSheet = SheetView.WorkSheet;
                SheetView.ActiveRow = 0;
                SheetView.ActiveColumn = 0;
                SheetView.SelectRange(0, 0, workSheet.RowCount, workSheet.ColumnCount);
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            var hitTest = HitTest();

            if (hitTest != null && hitTest.Element == VisualElement.TopLeft)
            {
                var workSheet = SheetView.WorkSheet;
                SheetView.ActiveRow = 0;
                SheetView.ActiveColumn = 0;
                SheetView.SelectRange(0, 0, workSheet.RowCount, workSheet.ColumnCount);
            }
        }
    }
}
