using System.Threading;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;
using DevBrewLabs.WPF.Spreadsheet;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)] // Ensures tests run on an STA thread, required for WPF components
    public class SpreadTests
    {
        [Test]
        public void Spread_CanBeInstantiated()
        {
            var spread = new Spread();
            Assert.That(spread, Is.Not.Null);
        }

        [Test]
        public void Spread_ZoomFactor_DefaultsToOne()
        {
            var spread = new Spread();
            Assert.That(spread.ZoomFactor, Is.EqualTo(1.0));
        }

        [Test]
        public void Spread_HasExpectedDefaultPropertyValues()
        {
            var spread = new Spread();

            Assert.That(spread.AllowZooming, Is.True);
            Assert.That(spread.AllowRowResize, Is.True);
            Assert.That(spread.AllowColumnResize, Is.True);
            Assert.That(spread.ShowTabStrip, Is.True);
            Assert.That(spread.ShowAddNewSheet, Is.True);
            Assert.That(spread.IsSelectionAnimationEnabled, Is.False);
            Assert.That(spread.ShowFormulaSuggestions, Is.True);
            Assert.That(spread.ScrollMode, Is.EqualTo(SheetScrollMode.Item));
        }

        [Test]
        public void Spread_MeasureAndArrange_DoesNotThrow()
        {
            var spread = new Spread();
            
            // Simulating layout pass which often triggers internal calculation logic
            spread.Measure(new Size(800, 600));
            spread.Arrange(new Rect(0, 0, 800, 600));

            Assert.That(spread.DesiredSize.Width, Is.GreaterThanOrEqualTo(0));
            Assert.That(spread.DesiredSize.Height, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Spread_WorkBook_IsInitializedWithDefaultName()
        {
            var spread = new Spread();
            Assert.That(spread.WorkBook, Is.Not.Null);
            Assert.That(spread.WorkBook.Name, Is.EqualTo("Book1"));
        }

        [Test]
        public void Spread_SheetViews_IsInitialized()
        {
            var spread = new Spread();
            Assert.That(spread.Sheets, Is.Not.Null);
        }

        [Test]
        public void Spread_ZoomFactor_CanBeUpdated()
        {
            var spread = new Spread();
            spread.ZoomFactor = 1.5;
            Assert.That(spread.ZoomFactor, Is.EqualTo(1.5));
        }

        [Test]
        public void Spread_AllowRowResize_CanBeToggled()
        {
            var spread = new Spread();
            spread.AllowRowResize = false;
            Assert.That(spread.AllowRowResize, Is.False);
        }

        [Test]
        public void Spread_AllowColumnResize_CanBeToggled()
        {
            var spread = new Spread();
            spread.AllowColumnResize = false;
            Assert.That(spread.AllowColumnResize, Is.False);
        }

        [Test]
        public void Spread_SuspendUpdates_CanBeToggled()
        {
            var spread = new Spread();
            
            Assert.That(spread.SuspendUpdates, Is.False, "Default should be false");
            
            spread.SuspendUpdates = true;
            Assert.That(spread.SuspendUpdates, Is.True);
            
            spread.SuspendUpdates = false;
            Assert.That(spread.SuspendUpdates, Is.False);
        }

        [Test]
        public void Spread_TextFormattingOptions_AreSetCorrectly()
        {
            var spread = new Spread();
            
            var textFormattingMode = TextOptions.GetTextFormattingMode(spread);
            var textRenderingMode = TextOptions.GetTextRenderingMode(spread);

            Assert.That(textFormattingMode, Is.EqualTo(TextFormattingMode.Display));
            Assert.That(textRenderingMode, Is.EqualTo(TextRenderingMode.ClearType));
        }
        
        [Test]
        public void Spread_SelectionBackground_CanBeUpdated()
        {
            var spread = new Spread();
            var newBrush = new SolidColorBrush(Colors.Red);
            
            spread.SelectionBackground = newBrush;
            
            Assert.That(spread.SelectionBackground, Is.EqualTo(newBrush));
        }
        [Test]
        public void Spread_SelectCell_UpdatesActiveSheetViewSelection()
        {
            var spread = new Spread();
            spread.SelectCell(5, 10);
            
            Assert.That(spread.Sheets.ActiveSheet.ActiveRow, Is.EqualTo(5));
            Assert.That(spread.Sheets.ActiveSheet.ActiveColumn, Is.EqualTo(10));
        }

        [Test]
        public void Spread_SelectRow_UpdatesActiveSheetViewSelection()
        {
            var spread = new Spread();
            spread.SelectRow(3);
            
            Assert.That(spread.Sheets.ActiveSheet.Selection.TopRow, Is.EqualTo(3));
            Assert.That(spread.Sheets.ActiveSheet.Selection.RowCount, Is.EqualTo(1));
        }

        [Test]
        public void Spread_SelectColumn_UpdatesActiveSheetViewSelection()
        {
            var spread = new Spread();
            spread.SelectColumn(4);
            
            Assert.That(spread.Sheets.ActiveSheet.Selection.LeftColumn, Is.EqualTo(4));
            Assert.That(spread.Sheets.ActiveSheet.Selection.ColumnCount, Is.EqualTo(1));
        }

        [Test]
        public void Spread_SelectRange_UpdatesActiveSheetViewSelection()
        {
            var spread = new Spread();
            spread.SelectRange(1, 2, 3, 4);
            
            Assert.That(spread.Sheets.ActiveSheet.Selection.TopRow, Is.EqualTo(1));
            Assert.That(spread.Sheets.ActiveSheet.Selection.LeftColumn, Is.EqualTo(2));
            Assert.That(spread.Sheets.ActiveSheet.Selection.RowCount, Is.EqualTo(3));
            Assert.That(spread.Sheets.ActiveSheet.Selection.ColumnCount, Is.EqualTo(4));
        }

        [Test]
        public void Spread_UndoRedo_CanBeCalledWithoutException()
        {
            var spread = new Spread();
            // Should not throw even if the undo stack is empty initially
            Assert.DoesNotThrow(() => spread.Undo());
            Assert.DoesNotThrow(() => spread.Redo());
        }

        [Test]
        public void Spread_ActiveSheetView_HasExpectedDefaultSettings()
        {
            var spread = new Spread();
            var view = spread.Sheets.ActiveSheet;
            
            Assert.That(view, Is.Not.Null);
            Assert.That(view.ZoomFactor, Is.EqualTo(1.0));
            Assert.That(view.ActiveRow, Is.EqualTo(0));
            Assert.That(view.ActiveColumn, Is.EqualTo(0));
        }
    }
}
