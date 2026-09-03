using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SliderCellTypeTests
    {
        [Test]
        public void SliderCellType_DefaultProperties()
        {
            var cellType = new SliderCellType();

            Assert.That(cellType.Minimum, Is.EqualTo(0.0));
            Assert.That(cellType.Maximum, Is.EqualTo(100.0));
            Assert.That(cellType.Step, Is.EqualTo(1.0));
            Assert.That(cellType.TrackHeight, Is.EqualTo(5.0));
            Assert.That(cellType.ThumbSize, Is.EqualTo(14.0));
            Assert.That(cellType.BarMargin, Is.EqualTo(6.0));
            Assert.That(cellType.ShowValue, Is.True);
            Assert.That(cellType.ValueFormat, Is.EqualTo("{0:0}"));
            Assert.That(cellType.ValuePlacement, Is.EqualTo(SliderValuePlacement.Right));
            Assert.That(cellType.ShowTicks, Is.False);
            Assert.That(cellType.TickFrequency, Is.EqualTo(10.0));
            Assert.That(cellType.IsReadOnly, Is.False);
            Assert.That(cellType.SupportsEditing, Is.True);
            Assert.That(cellType.CreateEditor(null), Is.TypeOf<NumericCellEditor>());
        }

        [Test]
        public void SliderCellType_ParseValue_NumericTypes()
        {
            var cellType = new SliderCellType { Minimum = 0.0, Maximum = 100.0, Step = 1.0 };

            Assert.That(cellType.ParseValue(42.0), Is.EqualTo(42.0));
            Assert.That(cellType.ParseValue(42), Is.EqualTo(42.0));
            Assert.That(cellType.ParseValue((float)42.0), Is.EqualTo(42.0));
            Assert.That(cellType.ParseValue((decimal)42.0), Is.EqualTo(42.0));
            Assert.That(cellType.ParseValue("42"), Is.EqualTo(42.0));
            Assert.That(cellType.ParseValue("42.4"), Is.EqualTo(42.0)); // Snapped to Step 1.0
            Assert.That(cellType.ParseValue(null), Is.EqualTo(0.0));
            Assert.That(cellType.ParseValue("invalid"), Is.EqualTo(0.0));
        }

        [Test]
        public void SliderCellType_ClampAndStep_SnapsProperly()
        {
            var cellType = new SliderCellType { Minimum = 10.0, Maximum = 50.0, Step = 5.0 };

            // Clamping
            Assert.That(cellType.ClampAndStep(5.0), Is.EqualTo(10.0));
            Assert.That(cellType.ClampAndStep(65.0), Is.EqualTo(50.0));

            // Step snapping
            Assert.That(cellType.ClampAndStep(12.0), Is.EqualTo(10.0));
            Assert.That(cellType.ClampAndStep(13.0), Is.EqualTo(15.0));
            Assert.That(cellType.ClampAndStep(47.4), Is.EqualTo(45.0));
            Assert.That(cellType.ClampAndStep(47.6), Is.EqualTo(50.0));
        }

        [Test]
        public void SliderCellType_ComputeRatio()
        {
            var cellType = new SliderCellType { Minimum = 200.0, Maximum = 400.0 };

            Assert.That(cellType.ComputeRatio(200.0), Is.EqualTo(0.0));
            Assert.That(cellType.ComputeRatio(300.0), Is.EqualTo(0.5));
            Assert.That(cellType.ComputeRatio(400.0), Is.EqualTo(1.0));
            Assert.That(cellType.ComputeRatio(150.0), Is.EqualTo(0.0)); // Clamped min
            Assert.That(cellType.ComputeRatio(450.0), Is.EqualTo(1.0)); // Clamped max
        }

        [Test]
        public void SliderCellType_CalculateLayout()
        {
            var cellType = new SliderCellType
            {
                Minimum = 0.0,
                Maximum = 100.0,
                ThumbSize = 14.0,
                BarMargin = 6.0,
                TrackHeight = 6.0,
                ShowValue = true,
                ValuePlacement = SliderValuePlacement.Right
            };

            var cellRect = new Rect(0, 0, 200, 30);
            var (trackRect, thumbCenter, textRect) = cellType.CalculateLayout(cellRect, 1.0, 50.0);

            // Track height should match TrackHeight * zoom
            Assert.That(trackRect.Height, Is.EqualTo(6.0));
            // Y position should be centered in cellRect
            Assert.That(trackRect.Y, Is.EqualTo((30 - 6.0) / 2.0));
            // Thumb center Y should be vertical center
            Assert.That(thumbCenter.Y, Is.EqualTo(15.0));
            // At 50%, thumbCenter.X should be at the midpoint of trackRect
            Assert.That(thumbCenter.X, Is.EqualTo(trackRect.X + trackRect.Width * 0.5));
            // TextRect should be on the right
            Assert.That(textRect.Right, Is.EqualTo(cellRect.Right - 6.0));
        }

        [Test]
        public void SliderCellType_CustomBrushes()
        {
            var cellType = new SliderCellType
            {
                TrackBrush = Brushes.SlateGray,
                FillBrush = Brushes.Indigo,
                ThumbBrush = Brushes.Gold,
                ThumbBorderBrush = Brushes.DarkGoldenrod
            };

            Assert.That(cellType.TrackBrush, Is.EqualTo(Brushes.SlateGray));
            Assert.That(cellType.FillBrush, Is.EqualTo(Brushes.Indigo));
            Assert.That(cellType.ThumbBrush, Is.EqualTo(Brushes.Gold));
            Assert.That(cellType.ThumbBorderBrush, Is.EqualTo(Brushes.DarkGoldenrod));
        }

        [Test]
        public void SliderCellType_ReadOnlyMode()
        {
            var cellType = new SliderCellType { IsReadOnly = true };

            Assert.That(cellType.IsReadOnly, Is.True);
            Assert.That(cellType.SupportsEditing, Is.False);
            Assert.That(cellType.GetElements(null, 0, 0), Is.Empty);
        }

        [Test]
        public void SliderCellType_FormulasUpdateWhenCellChanges()
        {
            var spread = new Spread();
            var sheet = spread.Sheets.ActiveSheet.WorkSheet;
            var slider = new SliderCellType { Minimum = 100, Maximum = 1000 };
            sheet.Cells[0, 1].CellType = slider;
            sheet.Cells[0, 1].Value = 500.0;
            sheet.Cells[1, 1].Formula = "=B1*2";

            Assert.That(Convert.ToDouble(sheet.Cells[1, 1].Value), Is.EqualTo(1000.0));

            sheet.SetValue(0, 1, 600.0);
            Assert.That(Convert.ToDouble(sheet.Cells[1, 1].Value), Is.EqualTo(1200.0));

            // Set up mortgage scenario
            sheet.Cells[1, 1].Value = 450000.0; // B2: Price
            sheet.Cells[2, 1].Value = 20.0;     // B3: Down %
            sheet.Cells[3, 1].Value = 6.5;      // B4: Rate %
            sheet.Cells[4, 1].Value = 30.0;     // B5: Term Yrs

            // C7: Down Amount
            sheet.Cells[6, 2].Formula = "=B2*(B3/100)";
            Assert.That(Convert.ToDouble(sheet.Cells[6, 2].Value), Is.EqualTo(90000.0));

            // C8: Principal
            sheet.Cells[7, 2].Formula = "=B2-C7";
            Assert.That(Convert.ToDouble(sheet.Cells[7, 2].Value), Is.EqualTo(360000.0));

            // C9: Monthly payment
            sheet.Cells[8, 2].Formula = "=(C8*(B4/1200)*POWER(1+B4/1200,B5*12))/(POWER(1+B4/1200,B5*12)-1)";
            double monthly = Convert.ToDouble(sheet.Cells[8, 2].Value);
            Assert.That(monthly, Is.InRange(2270.0, 2280.0));

            // C11: Total Repaid
            sheet.Cells[10, 2].Formula = "=C9*B5*12";
            double totalRepaid = Convert.ToDouble(sheet.Cells[10, 2].Value);
            Assert.That(totalRepaid, Is.GreaterThan(800000.0));

            // C10: Total Interest
            sheet.Cells[9, 2].Formula = "=C11-C8";
            double totalInterest = Convert.ToDouble(sheet.Cells[9, 2].Value);
            Assert.That(totalInterest, Is.GreaterThan(450000.0));
        }
    }
}
