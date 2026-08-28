using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using NUnit.Framework;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Elements;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ButtonCellTypeTests
    {
        private class TestWpfCommand : ICommand
        {
            public bool Executed { get; private set; }
            public object ExecutedParameter { get; private set; }
            public bool CanExecuteReturn { get; set; } = true;

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => CanExecuteReturn;

            public void Execute(object parameter)
            {
                Executed = true;
                ExecutedParameter = parameter;
            }
        }

        [Test]
        public void ButtonCellType_GetElements_ReturnsButtonElementInstance()
        {
            var cellType = new ButtonCellType();
            var elements = cellType.GetElements(null, 0, 0).ToList();

            Assert.That(elements.Count, Is.EqualTo(1));
            Assert.That(elements[0], Is.TypeOf<ButtonElement>());
            Assert.That(cellType.GetElements(null, 0, 0).First(), Is.SameAs(elements[0]));
        }

        [Test]
        public void ButtonCellType_DistinctInstances_DoNotShareElements()
        {
            var cellType1 = new ButtonCellType();
            var cellType2 = new ButtonCellType();

            var element1 = cellType1.GetElements(null, 0, 0).First();
            var element2 = cellType2.GetElements(null, 0, 0).First();

            Assert.That(element1, Is.Not.SameAs(element2));
        }

        [Test]
        public void ButtonCellType_CustomBrushes_CreateFrozenPensAndAreExposed()
        {
            var cellType = new ButtonCellType();
            var customBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

            cellType.BorderBrush = customBrush;
            Assert.That(cellType.BorderPen, Is.Not.Null);
            Assert.That(cellType.BorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.BorderPen.IsFrozen, Is.True);

            cellType.HoverBorderBrush = customBrush;
            Assert.That(cellType.HoverBorderPen, Is.Not.Null);
            Assert.That(cellType.HoverBorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.HoverBorderPen.IsFrozen, Is.True);

            cellType.PressedBorderBrush = customBrush;
            Assert.That(cellType.PressedBorderPen, Is.Not.Null);
            Assert.That(cellType.PressedBorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.PressedBorderPen.IsFrozen, Is.True);

            cellType.DisabledBorderBrush = customBrush;
            Assert.That(cellType.DisabledBorderPen, Is.Not.Null);
            Assert.That(cellType.DisabledBorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.DisabledBorderPen.IsFrozen, Is.True);
        }

        [Test]
        public void ButtonElement_GetBounds_CalculatedWithMarginAndZoom()
        {
            var cellType = new ButtonCellType { ButtonMargin = 3.0 };
            var element = cellType.GetElements(null, 0, 0).First();
            var cellRect = new Rect(100, 50, 100, 40);

            var bounds1 = element.GetBounds(cellRect, 1.0);
            Assert.That(bounds1.X, Is.EqualTo(103.0));
            Assert.That(bounds1.Y, Is.EqualTo(53.0));
            Assert.That(bounds1.Width, Is.EqualTo(94.0));
            Assert.That(bounds1.Height, Is.EqualTo(34.0));

            var bounds2 = element.GetBounds(cellRect, 2.0);
            // Margin at 2x zoom = 6.0
            Assert.That(bounds2.X, Is.EqualTo(106.0));
            Assert.That(bounds2.Y, Is.EqualTo(56.0));
            Assert.That(bounds2.Width, Is.EqualTo(88.0));
            Assert.That(bounds2.Height, Is.EqualTo(28.0));
        }

        [Test]
        public void ButtonCellType_OnClick_RaisesClickEvent()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var cellType = new ButtonCellType { Text = "Click Me" };

            bool cellTypeClickFired = false;

            cellType.Click += (s, e) =>
            {
                cellTypeClickFired = true;
                Assert.That(e.Row, Is.EqualTo(1));
                Assert.That(e.Column, Is.EqualTo(2));
                Assert.That(e.CellType, Is.SameAs(cellType));
            };

            var element = cellType.GetElements(sheetView, 1, 2).First();
            element.OnClick(sheetView, 1, 2);

            Assert.That(cellTypeClickFired, Is.True);
        }

        [Test]
        public void ButtonCellType_OnClick_ExecutesICommandWithParameter()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var wpfCommand = new TestWpfCommand();
            var param = "CustomParameter";
            var cellType = new ButtonCellType
            {
                Command = wpfCommand,
                CommandParameter = param
            };

            var element = cellType.GetElements(sheetView, 0, 0).First();
            element.OnClick(sheetView, 0, 0);

            Assert.That(wpfCommand.Executed, Is.True);
            Assert.That(wpfCommand.ExecutedParameter, Is.SameAs(param));
        }

        [Test]
        public void ButtonCellType_OnClick_RespectsLockedCell()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var command = new TestWpfCommand();
            var cellType = new ButtonCellType { Command = command };
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetLocked(0, 1, true);

            var element = cellType.GetElements(sheetView, 0, 1).First();
            element.OnClick(sheetView, 0, 1);

            Assert.That(command.Executed, Is.False);
        }
    }
}
