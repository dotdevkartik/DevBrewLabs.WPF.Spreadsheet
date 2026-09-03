using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using NUnit.Framework;
using System.Threading;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ProgressBarCellTypeTests
    {
        [Test]
        public void ProgressBarCellType_DefaultProperties()
        {
            var cellType = new ProgressBarCellType();

            Assert.That(cellType.Minimum, Is.EqualTo(0.0));
            Assert.That(cellType.Maximum, Is.EqualTo(100.0));
            Assert.That(cellType.BarHeight, Is.EqualTo(8.0));
            Assert.That(cellType.BarMargin, Is.EqualTo(4.0));
            Assert.That(cellType.CornerRadius, Is.EqualTo(4.0));
            Assert.That(cellType.ShowText, Is.True);
            Assert.That(cellType.Format, Is.EqualTo("{0:0}%"));
            Assert.That(cellType.TextPlacement, Is.EqualTo(ProgressBarTextPlacement.Right));
            Assert.That(cellType.AutoColor, Is.False);
            Assert.That(cellType.SupportsEditing, Is.True);
            Assert.That(cellType.CreateEditor(null), Is.TypeOf<NumericCellEditor>());
        }

        [Test]
        public void ProgressBarCellType_ComputeProgress_ClampsValuesCorrectly()
        {
            var cellType = new ProgressBarCellType { Minimum = 0, Maximum = 100 };

            // Middle
            Assert.That(cellType.ComputeProgress(50), Is.EqualTo(0.5).Within(1e-6));
            Assert.That(cellType.ComputeProgress(50.0), Is.EqualTo(0.5).Within(1e-6));
            Assert.That(cellType.ComputeProgress("50"), Is.EqualTo(0.5).Within(1e-6));
            Assert.That(cellType.ComputeProgress(50m), Is.EqualTo(0.5).Within(1e-6));

            // Boundaries
            Assert.That(cellType.ComputeProgress(0), Is.EqualTo(0.0).Within(1e-6));
            Assert.That(cellType.ComputeProgress(100), Is.EqualTo(1.0).Within(1e-6));

            // Underflow & Overflow clamped
            Assert.That(cellType.ComputeProgress(-25), Is.EqualTo(0.0).Within(1e-6));
            Assert.That(cellType.ComputeProgress(125), Is.EqualTo(1.0).Within(1e-6));

            // Invalid / Null
            Assert.That(cellType.ComputeProgress(null), Is.EqualTo(0.0));
            Assert.That(cellType.ComputeProgress("invalid"), Is.EqualTo(0.0));
        }

        [Test]
        public void ProgressBarCellType_ComputeProgress_CustomRange()
        {
            var cellType = new ProgressBarCellType { Minimum = 200, Maximum = 400 };

            Assert.That(cellType.ComputeProgress(300), Is.EqualTo(0.5).Within(1e-6));
            Assert.That(cellType.ComputeProgress(200), Is.EqualTo(0.0).Within(1e-6));
            Assert.That(cellType.ComputeProgress(400), Is.EqualTo(1.0).Within(1e-6));
            Assert.That(cellType.ComputeProgress(100), Is.EqualTo(0.0).Within(1e-6));
            Assert.That(cellType.ComputeProgress(500), Is.EqualTo(1.0).Within(1e-6));
        }

        [Test]
        public void ProgressBarCellType_ComputeProgress_InvalidRangeReturnsZero()
        {
            var cellType = new ProgressBarCellType { Minimum = 100, Maximum = 100 };
            Assert.That(cellType.ComputeProgress(50), Is.EqualTo(0.0));

            cellType.Minimum = 200;
            cellType.Maximum = 100;
            Assert.That(cellType.ComputeProgress(150), Is.EqualTo(0.0));
        }

        [Test]
        public void ProgressBarCellType_CustomBorderBrushes_CreatesFrozenPens()
        {
            var cellType = new ProgressBarCellType();
            var customBrush = new SolidColorBrush(Colors.SlateGray);

            cellType.TrackBorderBrush = customBrush;
            Assert.That(cellType.TrackBorderPen, Is.Not.Null);
            Assert.That(cellType.TrackBorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.TrackBorderPen.IsFrozen, Is.True);

            cellType.ProgressBorderBrush = customBrush;
            Assert.That(cellType.ProgressBorderPen, Is.Not.Null);
            Assert.That(cellType.ProgressBorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.ProgressBorderPen.IsFrozen, Is.True);
        }
    }
}
