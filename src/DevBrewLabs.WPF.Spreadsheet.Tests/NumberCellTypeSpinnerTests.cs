using System.Linq;
using System.Threading;
using System.Windows;
using NUnit.Framework;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Elements;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class NumberCellTypeSpinnerTests
    {
        [Test]
        public void NumberCellType_GetElements_ReturnsEmptyWhenSpinnersDisabled()
        {
            var cellType = new NumberCellType { ShowSpinners = false };
            var elements = cellType.GetElements(null, 0, 0).ToList();

            Assert.That(elements, Is.Empty);
        }

        [Test]
        public void NumberCellType_GetElements_ReturnsUpAndDownSpinnersWhenEnabled()
        {
            var cellType = new NumberCellType { ShowSpinners = true };
            var elements = cellType.GetElements(null, 0, 0).OfType<SpinnerButton>().ToList();

            Assert.That(elements.Count, Is.EqualTo(2));
            Assert.That(elements[0].Direction, Is.EqualTo(SpinDirection.Up));
            Assert.That(elements[1].Direction, Is.EqualTo(SpinDirection.Down));

            // Verify caching per instance
            var secondCallElements = cellType.GetElements(null, 0, 0).OfType<SpinnerButton>().ToList();
            Assert.That(secondCallElements[0], Is.SameAs(elements[0]));
            Assert.That(secondCallElements[1], Is.SameAs(elements[1]));
        }

        [Test]
        public void NumberCellType_DistinctInstances_DoNotShareElements()
        {
            var cellType1 = new NumberCellType { ShowSpinners = true };
            var cellType2 = new NumberCellType { ShowSpinners = true };

            var elements1 = cellType1.GetElements(null, 0, 0).ToList();
            var elements2 = cellType2.GetElements(null, 0, 0).ToList();

            Assert.That(elements1[0], Is.Not.SameAs(elements2[0]));
            Assert.That(elements1[1], Is.Not.SameAs(elements2[1]));
        }

        [Test]
        public void SpinnerCellType_CustomBrushes_CreateFrozenPensAndAreExposed()
        {
            var cellType = new NumberCellType();
            var customBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);

            cellType.SeparatorBrush = customBrush;
            Assert.That(cellType.SeparatorPen, Is.Not.Null);
            Assert.That(cellType.SeparatorPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.SeparatorPen.IsFrozen, Is.True);
        }

        [Test]
        public void SpinnerButton_Bounds_CalculatedCorrectlyForUpAndDown()
        {
            var cellType = new NumberCellType { ShowSpinners = true };
            var elements = cellType.GetElements(null, 0, 0).OfType<SpinnerButton>().ToList();
            var upBtn = elements[0];
            var downBtn = elements[1];

            var cellRect = new Rect(100, 50, 120, 30);
            var upBounds = upBtn.GetBounds(cellRect, 1.0);
            var downBounds = downBtn.GetBounds(cellRect, 1.0);

            // Total spinner width = 16
            Assert.That(upBounds.X, Is.EqualTo(220 - 16));
            Assert.That(upBounds.Y, Is.EqualTo(50));
            Assert.That(upBounds.Width, Is.EqualTo(16));
            Assert.That(upBounds.Height, Is.EqualTo(15));

            Assert.That(downBounds.X, Is.EqualTo(220 - 16));
            Assert.That(downBounds.Y, Is.EqualTo(65));
            Assert.That(downBounds.Width, Is.EqualTo(16));
            Assert.That(downBounds.Height, Is.EqualTo(15));
        }

        [Test]
        public void SpinnerButton_SpinUp_IncrementsValueByStep()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 100 };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 20.0);

            var upBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Up);
            upBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(25.0));
        }

        [Test]
        public void SpinnerButton_SpinDown_DecrementsValueByStep()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 100 };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 20.0);

            var downBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Down);
            downBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(15.0));
        }

        [Test]
        public void SpinnerButton_SpinUp_ClampsToMaximum()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 22, SpinWrap = false };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 20.0);

            var upBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Up);
            upBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(22.0));
        }

        [Test]
        public void SpinnerButton_SpinDown_ClampsToMinimum()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 5, Minimum = 18, Maximum = 100, SpinWrap = false };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 20.0);

            var downBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Down);
            downBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(18.0));
        }

        [Test]
        public void SpinnerButton_SpinUp_WrapsToMinimumWhenSpinWrapEnabled()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 20, SpinWrap = true };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 20.0);

            var upBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Up);
            upBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(0.0));
        }

        [Test]
        public void SpinnerButton_SpinDown_WrapsToMaximumWhenSpinWrapEnabled()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 5, Minimum = 0, Maximum = 20, SpinWrap = true };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 0.0);

            var downBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Down);
            downBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(20.0));
        }

        [Test]
        public void SpinnerButton_SpinUp_HandlesNullInitialValue()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 1, Minimum = 0, Maximum = 10 };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, null);

            var upBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Up);
            upBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(1.0));
        }

        [Test]
        public void BaseCellType_GetContentRect_ReturnsFullRectByDefault()
        {
            var cellType = TextCellType.Default;
            var cellRect = new Rect(10, 20, 100, 30);
            var contentRect = cellType.GetContentRect(null, 0, 0, cellRect, 1.0);

            Assert.That(contentRect, Is.EqualTo(cellRect));
        }

        [Test]
        public void NumberCellType_GetContentRect_ReservesSpaceForSpinners()
        {
            var cellTypeWithoutSpinners = new NumberCellType { ShowSpinners = false };
            var cellTypeWithSpinners = new NumberCellType { ShowSpinners = true };

            var cellRect = new Rect(10, 20, 100, 30);

            var contentWithout = cellTypeWithoutSpinners.GetContentRect(null, 0, 0, cellRect, 1.0);
            Assert.That(contentWithout, Is.EqualTo(cellRect));

            var contentWith = cellTypeWithSpinners.GetContentRect(null, 0, 0, cellRect, 1.0);
            Assert.That(contentWith.X, Is.EqualTo(10));
            Assert.That(contentWith.Y, Is.EqualTo(20));
            Assert.That(contentWith.Width, Is.EqualTo(100 - 16));
            Assert.That(contentWith.Height, Is.EqualTo(30));

            var contentWithZoom = cellTypeWithSpinners.GetContentRect(null, 0, 0, new Rect(20, 40, 200, 60), 2.0);
            Assert.That(contentWithZoom.Width, Is.EqualTo(200 - 32));
        }

        [Test]
        public void SpinnerButton_SpinDown_StepsIntoNegativeNumbers()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new NumberCellType { ShowSpinners = true, Step = 10, Minimum = -100, Maximum = 100 };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, 0.0);

            var downBtn = cellType.GetElements(sheetView, 0, 1).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Down);
            downBtn.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(-10.0));

            downBtn.OnClick(sheetView, 0, 1);
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(-20.0));
        }

        [Test]
        public void SpinnerButton_GetBounds_ScalesProperlyWithZoomFactor()
        {
            var cellType = new NumberCellType { ShowSpinners = true };
            var upBtn = cellType.GetElements(null, 0, 0).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Up);
            var downBtn = cellType.GetElements(null, 0, 0).OfType<SpinnerButton>().First(b => b.Direction == SpinDirection.Down);

            // Model cell at unscaled coordinates (0, 0, 100, 30)
            // At zoom 1.5, scaledCellRect = (0, 0, 150, 45)
            double zoom = 1.5;
            var scaledCellRect = new Rect(0, 0, 100 * zoom, 30 * zoom);

            var upBounds = upBtn.GetBounds(scaledCellRect, zoom);
            var downBounds = downBtn.GetBounds(scaledCellRect, zoom);

            // Spinner width = 16 * 1.5 = 24
            Assert.That(upBounds.Width, Is.EqualTo(24));
            Assert.That(upBounds.Height, Is.EqualTo(22.5));
            Assert.That(upBounds.X, Is.EqualTo(150 - 24));
            Assert.That(upBounds.Y, Is.EqualTo(0));

            Assert.That(downBounds.Width, Is.EqualTo(24));
            Assert.That(downBounds.Height, Is.EqualTo(22.5));
            Assert.That(downBounds.X, Is.EqualTo(150 - 24));
            Assert.That(downBounds.Y, Is.EqualTo(22.5));
        }
    }
}
