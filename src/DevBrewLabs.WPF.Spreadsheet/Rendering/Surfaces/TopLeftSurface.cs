using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    internal class TopLeftSurface : SurfaceBase
    {
        private SpreadHitTestResult _hitTest;

        public TopLeftSurface(SheetView view) : base(view)
        {
        }

        protected override InteractionLayer CreateInteractionLayer()
        {
            return new TopLeftInteractionLayer(SheetView);
        }

        protected override DrawingGroup CreateDrawing()
        {
            return new DrawingGroup();
        }

        protected override SpreadHitTestResult HitTestCore(Point point)
        {
            if (_hitTest == null)
            {              
                _hitTest = new SpreadHitTestResult()
                {
                    ActualHitTestPoint = point,
                    Position = new Point(0, 0),
                    Element = VisualElement.TopLeft,
                    Row = -1,
                    Column = -1,
                    Sheet = SheetView
                };
            }

            return _hitTest;
        }
    }
}
