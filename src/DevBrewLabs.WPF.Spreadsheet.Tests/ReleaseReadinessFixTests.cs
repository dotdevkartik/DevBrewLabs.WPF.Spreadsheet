using System;
using System.Threading;
using NUnit.Framework;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ReleaseReadinessFixTests
    {
        [Test]
        public void AutoSizeColumn_IndexGreaterThanZero_DoesNotThrow()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];
            sheet.SetValue(0, 1, "Testing Long Column Data");
            sheet.SetValue(1, 1, "Another Row Data");

            var sheetView = (SheetView)spread.Sheets.GetSheetView(sheet);
            Assert.DoesNotThrow(() => sheetView.AutoSizeColumn(1));
            Assert.That(sheet.Columns.GetColumnWidth(1), Is.GreaterThan(0));
        }

        [Test]
        public void FormulaError_CircularReference_DoesNotThrow()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];
            sheet.SetFormula(0, 0, "=A1");

            object value = null;
            Assert.DoesNotThrow(() => { value = sheet.GetValue(0, 0); });
            Assert.That(value, Is.Not.Null);
            Assert.That(value.ToString(), Does.StartWith("#"));
        }

        [Test]
        public void FormulaError_DivideByZero_ReturnsDiv0()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];
            sheet.SetFormula(0, 0, "=1/0");

            object value = null;
            Assert.DoesNotThrow(() => { value = sheet.GetValue(0, 0); });
            Assert.That(value, Is.EqualTo("#DIV/0!"));
        }

        [Test]
        public void SpanManager_SequentialRowSpanAndColumnSpan_DoesNotThrow()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];

            Assert.DoesNotThrow(() =>
            {
                var cell = sheet.Cells[0, 0];
                cell.RowSpan = 2;
                cell.ColumnSpan = 2;
            });

            Assert.That(sheet.GetRowSpan(0, 0), Is.EqualTo(2));
            Assert.That(sheet.GetColumnSpan(0, 0), Is.EqualTo(2));
        }

        [Test]
        public void CellRange_Intersects_EvaluatesAABBCorrectly()
        {
            var r1 = new CellRange(0, 0, 2, 2);
            var r2 = new CellRange(5, 5, 2, 2);
            var r3 = new CellRange(1, 1, 2, 2);

            Assert.That(r1.Intersects(r2), Is.False, "Disjoint ranges must not intersect.");
            Assert.That(r1.Intersects(r3), Is.True, "Overlapping ranges must intersect.");
        }

        [Test]
        public void Dimension_InsertAndRemove_ThrowsNotImplementedException()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];

            Assert.Throws<NotImplementedException>(() => sheet.Rows.Insert(0, 1));
            Assert.Throws<NotImplementedException>(() => sheet.Rows.Remove(0, 1));
            Assert.Throws<NotImplementedException>(() => sheet.Columns.Insert(0, 1));
            Assert.Throws<NotImplementedException>(() => sheet.Columns.Remove(0, 1));
        }

        [Test]
        public void LocationCache_UniformRows_ReturnsAccurateLocationInO1()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];
            sheet.RowCount = 100000;

            var sheetView = (SheetView)spread.Sheets.GetSheetView(sheet);
            double expectedLoc = 99999.0 * sheet.DefaultRowHeight;
            double actualLoc = sheetView.ViewPort.GetRowLocation(99999);

            Assert.That(actualLoc, Is.EqualTo(expectedLoc));
        }

        [Test]
        public void AutoFilter_ClearAll_PrunesDefaultRows()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];
            sheet.SetValue(0, 0, "Header");
            sheet.SetValue(1, 0, "Apple");
            sheet.SetValue(2, 0, "Banana");

            sheet.AutoFilter.SetRange(new CellRange(0, 0, 3, 1));
            sheet.AutoFilter.SetFilter(0, new TextFilter(TextFilterOperator.Equals, "Apple"));

            var rowsDimension = (IDimensionCollection<IRow>)sheet.Rows;
            // Row 2 is filtered out and was instantiated
            Assert.That(rowsDimension.HasItems, Is.True);

            // Clearing all filters must unfilter and prune Row 2 from the dictionary
            sheet.AutoFilter.ClearAll();
            Assert.That(rowsDimension.HasItems, Is.False, "Default rows should be pruned from dictionary upon filter clear.");
        }

        [Test]
        public void CellEditing_Undo_PreservesFormula()
        {
            var spread = new Spread();
            var sheet = spread.WorkBook.WorkSheets[0];
            var sheetView = (SheetView)spread.Sheets.GetSheetView(sheet);
            
            // Set cell A1 with a formula
            sheet.SetValue(0, 1, 10);
            sheet.SetValue(0, 2, 20);
            sheet.SetFormula(0, 0, "=B1+C1");
            Assert.That(sheet.GetFormula(0, 0), Is.EqualTo("=B1+C1"));

            // Begin edit and commit a new literal value
            spread.BeginEdit(0, 0);
            var editor = (TextCellEditor)spread.EditingManager.ActiveEditor;
            editor.Text = "999";
            spread.EndEdit(commitChanges: true);

            Assert.That(sheet.GetFormula(0, 0), Is.Null);
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo(999.0));

            // Undo the edit
            spread.Undo();

            // Assert formula is restored!
            Assert.That(sheet.GetFormula(0, 0), Is.EqualTo("=B1+C1"), "Undo must restore original formula!");
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo(30.0), "Undo must recalculate restored formula!");
        }
    }
}
