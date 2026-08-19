using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Rendering
{
    /// <summary>
    /// An abstract class for sheet UI surface.
    /// </summary>
    internal abstract class SurfaceBase : Canvas
    {
        private InteractionLayer _interactionLayer;
        private DrawingGroup _drawing;

        protected SheetView SheetView { get; }

        public SurfaceBase(SheetView view)
        {
            SnapsToDevicePixels = true;
            SheetView = view;
            _drawing = CreateDrawing();
            _interactionLayer = CreateInteractionLayer();
            Children.Add(_interactionLayer);
            SetZIndex(_interactionLayer, 1);
        }

        public InteractionLayer GetInteractionLayer()
        {
            return _interactionLayer;
        }

        public DrawingGroup GetDrawing()
        {
            return _drawing;
        }

        /// <summary>
        /// Hit tests sheet on the provided coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public SpreadHitTestResult HitTest(Point point)
        {
            return HitTestCore(point);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawDrawing(_drawing);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_interactionLayer == null)
                return;

            _interactionLayer.Width = sizeInfo.NewSize.Width;
            _interactionLayer.Height = sizeInfo.NewSize.Height;
        }

        protected abstract InteractionLayer CreateInteractionLayer();

        /// <summary>
        /// Gets the drawing responsible for this canvas UI.
        /// </summary>
        /// <returns></returns>
        protected abstract DrawingGroup CreateDrawing();

        /// <summary>
        /// Provides hit test support.
        /// </summary>
        /// <param name="dc"></param>
        protected abstract SpreadHitTestResult HitTestCore(Point hitPoint);
    }
}
