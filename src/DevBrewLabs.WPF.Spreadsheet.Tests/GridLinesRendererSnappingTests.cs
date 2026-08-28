using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Rendering.Text;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GridLinesRendererSnappingTests
    {
        [TestCase(1.0, 1.0, Description = "100% DPI / 100% Zoom")]
        [TestCase(1.25, 1.0, Description = "125% DPI / 100% Zoom")]
        [TestCase(1.5, 1.0, Description = "150% DPI / 100% Zoom")]
        [TestCase(2.0, 1.0, Description = "200% DPI / 100% Zoom")]
        [TestCase(1.0, 0.8, Description = "100% DPI / 80% Zoom")]
        [TestCase(1.0, 0.9, Description = "100% DPI / 90% Zoom")]
        [TestCase(1.0, 1.1, Description = "100% DPI / 110% Zoom")]
        [TestCase(1.0, 1.25, Description = "100% DPI / 125% Zoom")]
        [TestCase(1.5, 1.25, Description = "150% DPI / 125% Zoom")]
        public void SnapLine_ProducesExactIntegerDevicePixelBoundaries(double dpi, double zoom)
        {
            double penThickness = 1.0;
            double rowHeight = 20.0;
            double colWidth = 80.0;

            for (int row = 0; row < 10; row++)
            {
                double rawY = (row + 1) * rowHeight * zoom;
                double snappedY = PixelSnapper.SnapLine(rawY, dpi, penThickness);

                // Check outer edge in device pixels: (snappedY + halfPenDip) * dpi
                double outerEdgePx = (snappedY + (penThickness / 2.0)) * dpi;
                double roundedOuterEdgePx = Math.Round(outerEdgePx);

                Assert.That(Math.Abs(outerEdgePx - roundedOuterEdgePx), Is.LessThan(1e-9),
                    $"Row {row} outer edge at DPI {dpi} and Zoom {zoom} must align to integer device pixel.");
            }

            for (int col = 0; col < 10; col++)
            {
                double rawX = (col + 1) * colWidth * zoom;
                double snappedX = PixelSnapper.SnapLine(rawX, dpi, penThickness);

                double outerEdgePx = (snappedX + (penThickness / 2.0)) * dpi;
                double roundedOuterEdgePx = Math.Round(outerEdgePx);

                Assert.That(Math.Abs(outerEdgePx - roundedOuterEdgePx), Is.LessThan(1e-9),
                    $"Col {col} outer edge at DPI {dpi} and Zoom {zoom} must align to integer device pixel.");
            }
        }

        [Test]
        public void SnapLine_MonotonicityAndNoSpacingDegradation()
        {
            double dpi = 1.25;
            double rowHeight = 21.0; // odd row height to test non-trivial rounding

            double prevSnappedY = PixelSnapper.SnapLine(0, dpi, 1.0);
            for (int row = 1; row <= 50; row++)
            {
                double rawY = row * rowHeight;
                double snappedY = PixelSnapper.SnapLine(rawY, dpi, 1.0);

                Assert.That(snappedY, Is.GreaterThan(prevSnappedY),
                    $"Line at row {row} must be strictly after line at row {row - 1}");

                prevSnappedY = snappedY;
            }
        }

        [Test]
        public void SnapLine_HandlesFractionalScrollOffsets()
        {
            double dpi = 1.5;
            double penThickness = 1.0;
            double[] scrollOffsets = { 0.0, 0.25, 0.333, 0.5, 0.75, 12.37, 45.89 };

            foreach (var scroll in scrollOffsets)
            {
                double rawY = (100.0 - scroll);
                double snappedY = PixelSnapper.SnapLine(rawY, dpi, penThickness);

                double outerEdgePx = (snappedY + (penThickness / 2.0)) * dpi;
                double roundedOuterEdgePx = Math.Round(outerEdgePx);

                Assert.That(Math.Abs(outerEdgePx - roundedOuterEdgePx), Is.LessThan(1e-9),
                    $"Outer edge with scroll offset {scroll} at DPI {dpi} must align to integer device pixel.");
            }
        }

        [Test]
        public void GridLinesRenderer_RendersSuccessfullyOnSpread()
        {
            var spread = new Spread();
            var activeSheet = (SheetView)spread.Sheets.ActiveSheet;

            // Trigger full render pass
            spread.Measure(new Size(800, 600));
            spread.Arrange(new Rect(0, 0, 800, 600));
            spread.Invalidate();

            // Verify grid lines renderer runs without exceptions
            var renderer = new GridLinesRenderer();
            var drawing = new DrawingGroup();
            using (var context = new RenderContext(drawing, activeSheet))
            {
                Assert.DoesNotThrow(() => renderer.OnRender(context, 0, 0, 10, 10));
            }
        }

        [Test]
        public void GridLinesRenderer_HandlesSpanCellsCorrectly()
        {
            var spread = new Spread();
            var activeSheet = (SheetView)spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)activeSheet.WorkSheet;

            // Merge cells B2:D4 (rows 1..3, cols 1..3)
            worksheet.AddSpan(1, 1, 3, 3);

            var renderer = new GridLinesRenderer();
            var drawing = new DrawingGroup();
            using (var context = new RenderContext(drawing, activeSheet))
            {
                Assert.DoesNotThrow(() => renderer.OnRender(context, 0, 0, 5, 5));
            }
        }

        [Test]
        public void GridLinesRenderer_HandlesHiddenAndZeroSizeRowsAndColumns()
        {
            var spread = new Spread();
            var activeSheet = (SheetView)spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)activeSheet.WorkSheet;

            // Hide row 2 and column 2
            worksheet.Rows[2].IsHidden = true;
            worksheet.Columns[2].Width = 0;

            // Zero height row 4 and zero width column 4
            worksheet.Rows[4].Height = 0;
            worksheet.Columns[4].Width = 0;

            var renderer = new GridLinesRenderer();
            var drawing = new DrawingGroup();
            using (var context = new RenderContext(drawing, activeSheet))
            {
                Assert.DoesNotThrow(() => renderer.OnRender(context, 0, 0, 6, 6));
            }
        }

        [TestCase(GridLineVisibility.Both)]
        [TestCase(GridLineVisibility.Horizontal)]
        [TestCase(GridLineVisibility.Vertical)]
        public void GridLinesRenderer_RespectsGridLineVisibility(GridLineVisibility visibility)
        {
            var spread = new Spread();
            var activeSheet = (SheetView)spread.Sheets.ActiveSheet;
            activeSheet.GridLineVisibility = visibility;

            var renderer = new GridLinesRenderer();
            var drawing = new DrawingGroup();
            using (var context = new RenderContext(drawing, activeSheet))
            {
                Assert.DoesNotThrow(() => renderer.OnRender(context, 0, 0, 5, 5));
            }
        }
    }
}
