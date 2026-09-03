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
    public class CheckBoxCellTypeTests
    {
        [Test]
        public void CheckBoxCellType_GetElements_ReturnsCheckBoxElementInstance()
        {
            var cellType = new CheckBoxCellType();
            var elements = cellType.GetElements(null, 0, 0).ToList();

            Assert.That(elements.Count, Is.EqualTo(1));
            Assert.That(elements[0], Is.TypeOf<CheckBoxElement>());
            Assert.That(cellType.GetElements(null, 0, 0).First(), Is.SameAs(elements[0]));
        }

        [Test]
        public void CheckBoxCellType_DistinctInstances_DoNotShareElements()
        {
            var cellType1 = new CheckBoxCellType();
            var cellType2 = new CheckBoxCellType();

            var element1 = cellType1.GetElements(null, 0, 0).First();
            var element2 = cellType2.GetElements(null, 0, 0).First();

            Assert.That(element1, Is.Not.SameAs(element2));
        }

        [Test]
        public void CheckBoxCellType_CustomBrushes_CreateFrozenPensAndAreExposed()
        {
            var cellType = new CheckBoxCellType();
            var customBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);

            cellType.CheckedBrush = customBrush;
            Assert.That(cellType.CheckedPen, Is.Not.Null);
            Assert.That(cellType.CheckedPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.CheckedPen.IsFrozen, Is.True);

            cellType.IndeterminateBrush = customBrush;
            Assert.That(cellType.IndeterminatePen, Is.Not.Null);
            Assert.That(cellType.IndeterminatePen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.IndeterminatePen.IsFrozen, Is.True);

            cellType.UncheckedBorderBrush = customBrush;
            Assert.That(cellType.UncheckedBorderPen, Is.Not.Null);
            Assert.That(cellType.UncheckedBorderPen.Brush, Is.SameAs(customBrush));
            Assert.That(cellType.UncheckedBorderPen.IsFrozen, Is.True);
        }

        [Test]
        public void CheckBoxElement_GetBounds_CalculatedCorrectly()
        {
            var cellType = new CheckBoxCellType();
            var element = cellType.GetElements(null, 0, 0).First();
            var cellRect = new Rect(100, 50, 100, 30);
            
            // CheckBoxSize is 14x14. Centered in (100, 50, 100, 30):
            // Center X = 100 + 50 - 7 = 143
            // Center Y = 50 + 15 - 7 = 58
            // Inflated by 3px on all sides: X -= 3 (140), Y -= 3 (55), Width += 6 (20), Height += 6 (20)
            var bounds = element.GetBounds(cellRect, 1.0);

            Assert.That(bounds.Width, Is.EqualTo(20.0));
            Assert.That(bounds.Height, Is.EqualTo(20.0));
            Assert.That(bounds.X, Is.EqualTo(140.0));
            Assert.That(bounds.Y, Is.EqualTo(55.0));
        }

        [Test]
        public void CheckBoxElement_GetBounds_ScalesWithZoom()
        {
            var cellType = new CheckBoxCellType();
            var element = cellType.GetElements(null, 0, 0).First();
            double zoom = 2.0;
            var cellRect = new Rect(200, 100, 200, 60);

            var bounds = element.GetBounds(cellRect, zoom);

            // CheckBoxSize is 14 * 2 = 28. Inflate by 3 * 2 = 6 on all sides.
            // Width = 28 + 12 = 40. Height = 28 + 12 = 40.
            Assert.That(bounds.Width, Is.EqualTo(40.0));
            Assert.That(bounds.Height, Is.EqualTo(40.0));
            Assert.That(bounds.X, Is.EqualTo(280.0));
            Assert.That(bounds.Y, Is.EqualTo(110.0));
        }

        [Test]
        public void CheckBoxCellType_TwoState_CycleTransitions()
        {
            var cellType = new CheckBoxCellType { IsThreeState = false };

            Assert.That(cellType.GetNextValue(null), Is.EqualTo(true));
            Assert.That(cellType.GetNextValue(false), Is.EqualTo(true));
            Assert.That(cellType.GetNextValue(true), Is.EqualTo(false));
            Assert.That(cellType.GetNextValue("true"), Is.EqualTo(false));
            Assert.That(cellType.GetNextValue("false"), Is.EqualTo(true));
        }

        [Test]
        public void CheckBoxCellType_ThreeState_CycleTransitions()
        {
            var cellType = new CheckBoxCellType { IsThreeState = true };

            // Cycle: false -> true -> null -> false
            Assert.That(cellType.GetNextValue(false), Is.EqualTo(true));
            Assert.That(cellType.GetNextValue(true), Is.Null);
            Assert.That(cellType.GetNextValue(null), Is.EqualTo(false));
        }

        [Test]
        public void CheckBoxElement_OnClick_TogglesCellValue()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new CheckBoxCellType();
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, false);

            var element = cellType.GetElements(sheetView, 0, 1).First();
            element.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(true));

            element.OnClick(sheetView, 0, 1);
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(false));
        }

        [Test]
        public void CheckBoxCellType_ToggleValue_RespectsLockedCell()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new CheckBoxCellType();
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, false);
            worksheet.SetLocked(0, 1, true);

            var element = cellType.GetElements(sheetView, 0, 1).First();
            element.OnClick(sheetView, 0, 1);

            // Should remain false because cell is locked
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(false));
        }

        [Test]
        public void CheckBoxCellType_ToggleValue_SupportsUndoRedo()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new CheckBoxCellType();
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, false);

            var element = cellType.GetElements(sheetView, 0, 1).First();
            element.OnClick(sheetView, 0, 1);

            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(true));

            // Undo
            spread.UndoRedoManager.Undo();
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(false));

            // Redo
            spread.UndoRedoManager.Redo();
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(true));
        }

        [Test]
        public void CheckBoxCellType_KeyboardSpace_TogglesSelectedCellsWithUndo()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new CheckBoxCellType();
            worksheet.Columns[0].CellType = cellType;
            worksheet.SetValue(0, 0, false);
            worksheet.SetValue(1, 0, false);

            sheetView.SelectRange(0, 0, 2, 1);

            var interactionLayer = sheetView.CellsSurface.GetInteractionLayer();
            var keyEvent = new System.Windows.Input.KeyEventArgs(
                System.Windows.Input.Keyboard.PrimaryDevice,
                new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "", System.IntPtr.Zero),
                0,
                System.Windows.Input.Key.Space)
            {
                RoutedEvent = System.Windows.UIElement.PreviewKeyDownEvent
            };
            interactionLayer.RaiseEvent(keyEvent);

            Assert.That(worksheet.GetValue(0, 0), Is.EqualTo(true));
            Assert.That(worksheet.GetValue(1, 0), Is.EqualTo(true));

            // Undo composite action
            spread.UndoRedoManager.Undo();
            Assert.That(worksheet.GetValue(0, 0), Is.EqualTo(false));
            Assert.That(worksheet.GetValue(1, 0), Is.EqualTo(false));
        }

        [Test]
        public void CheckBoxCellType_FiresEditLifecycleEvents()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new CheckBoxCellType();
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, false);

            bool startingFired = false;
            bool endingFired = false;
            bool endedFired = false;
            object? endingNewValue = null;

            spread.CellEditStarting += (s, e) =>
            {
                startingFired = true;
                Assert.That(e.Row, Is.EqualTo(0));
                Assert.That(e.Column, Is.EqualTo(1));
            };

            spread.CellEditEnding += (s, e) =>
            {
                endingFired = true;
                endingNewValue = e.NewValue;
            };

            spread.CellEditEnded += (s, e) =>
            {
                endedFired = true;
                Assert.That(e.WasCommitted, Is.True);
            };

            var element = cellType.GetElements(sheetView, 0, 1).First();
            element.OnClick(sheetView, 0, 1);

            Assert.That(startingFired, Is.True);
            Assert.That(endingFired, Is.True);
            Assert.That(endedFired, Is.True);
            Assert.That(endingNewValue, Is.EqualTo(true));
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(true));
        }

        [Test]
        public void CheckBoxCellType_CellEditEnding_CanCancelToggle()
        {
            var spread = new Spread();
            var sheetView = spread.Sheets.ActiveSheet;
            var worksheet = (Worksheet)sheetView.WorkSheet;

            var cellType = new CheckBoxCellType();
            worksheet.Columns[1].CellType = cellType;
            worksheet.SetValue(0, 1, false);

            bool endedFired = false;
            bool endedWasCommitted = true;

            spread.CellEditEnding += (s, e) =>
            {
                // Cancel the toggle
                e.Cancel = true;
            };

            spread.CellEditEnded += (s, e) =>
            {
                endedFired = true;
                endedWasCommitted = e.WasCommitted;
            };

            var element = cellType.GetElements(sheetView, 0, 1).First();
            element.OnClick(sheetView, 0, 1);

            // Value must not change
            Assert.That(worksheet.GetValue(0, 1), Is.EqualTo(false));
            Assert.That(endedFired, Is.True);
            Assert.That(endedWasCommitted, Is.False);
        }
    }
}
