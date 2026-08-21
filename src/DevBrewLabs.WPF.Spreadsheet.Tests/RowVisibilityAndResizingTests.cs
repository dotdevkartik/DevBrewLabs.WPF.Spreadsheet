using System.Threading;
using NUnit.Framework;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RowVisibilityAndResizingTests
    {
        [Test]
        public void Rows_IsRowVisible_ReflectsVisibilityCorrectly()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];

            // Default unconfigured row
            Assert.That(ws.Rows.IsRowVisible(0), Is.True);
            Assert.That(ws.Rows.IsRowVisible(5), Is.True);

            // Manually hidden row
            ws.Rows[1].IsHidden = true;
            Assert.That(ws.Rows.IsRowVisible(1), Is.False);
            Assert.That(ws.Rows[1].Visible, Is.False);

            // Filtered out row
            ws.Rows[2].IsFilteredOut = true;
            Assert.That(ws.Rows.IsRowVisible(2), Is.False);
            Assert.That(ws.Rows[2].Visible, Is.False);

            // Row with height 0
            ws.Rows[3].Height = 0;
            Assert.That(ws.Rows.IsRowVisible(3), Is.False);
            Assert.That(ws.Rows[3].Visible, Is.False);

            // Unhiding row restores visibility
            ws.Rows[1].IsHidden = false;
            Assert.That(ws.Rows.IsRowVisible(1), Is.True);
        }

        [Test]
        public void Columns_IsColumnVisible_ReflectsVisibilityCorrectly()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];

            Assert.That(ws.Columns.IsColumnVisible(0), Is.True);

            ws.Columns[1].Width = 0;
            Assert.That(ws.Columns.IsColumnVisible(1), Is.False);

            ws.Columns[1].Width = 80;
            Assert.That(ws.Columns.IsColumnVisible(1), Is.True);
        }

        [Test]
        public void ViewPort_GetTemporaryRowLocation_CalculatesCorrectOffset_WithFilteredRows()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.SheetViews.ActiveSheetView;
            var viewPort = (ViewPort)view.ViewPort;

            ws.DefaultRowHeight = 20;

            // Row 0 is visible (0..20)
            // Row 1 is filtered out (effective height 0)
            // Row 2 is filtered out (effective height 0)
            // Row 3 is visible (20..40)
            ws.Rows[1].IsFilteredOut = true;
            ws.Rows[2].IsFilteredOut = true;

            viewPort.ResetRowLocations();

            Assert.That(viewPort.GetRowLocation(0), Is.EqualTo(0));
            Assert.That(viewPort.GetRowLocation(1), Is.EqualTo(20));
            Assert.That(viewPort.GetRowLocation(2), Is.EqualTo(20));
            Assert.That(viewPort.GetRowLocation(3), Is.EqualTo(20));
            Assert.That(viewPort.GetRowLocation(4), Is.EqualTo(40));

            // Set temporary height on row 0 to 30 (+10)
            viewPort.SetTemporaryRowHeight(0, 30);
            Assert.That(viewPort.GetTemporaryRowLocation(3), Is.EqualTo(30));

            // Set temporary height on row 3 to 50
            viewPort.SetTemporaryRowHeight(3, 50);
            Assert.That(viewPort.GetTemporaryRowLocation(4), Is.EqualTo(80));

            viewPort.ClearTemporaryRowHeights();
        }

        [Test]
        public void RowResizeManager_UpwardResize_SkipsFilteredRows()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.SheetViews.ActiveSheetView;
            var manager = (RowResizeManager)spread.RowResizeManager;

            ws.DefaultRowHeight = 20;

            // Row 0: visible (0..20)
            // Row 1, 2: filtered out (size 0)
            // Row 3: visible (20..40)
            ws.Rows[1].IsFilteredOut = true;
            ws.Rows[2].IsFilteredOut = true;

            view.ViewPort.ResetRowLocations();

            // Begin resize on row 3 bottom edge (located at logical Y = 20)
            manager.BeginResize(view, 3, 20);

            // Drag upward to logical Y = 10 (inside row 0)
            manager.Resize(view, 10);

            // Row 3 should be collapsed to 0
            Assert.That(view.ViewPort.GetTemporaryRowHeight(3), Is.EqualTo(0));
            // Row 0 should be resized to height 10
            Assert.That(view.ViewPort.GetTemporaryRowHeight(0), Is.EqualTo(10));
            // Filtered rows 1 and 2 should not have active positive temporary heights
            Assert.That(view.ViewPort.GetTemporaryRowHeight(1) ?? 0, Is.EqualTo(0));
            Assert.That(view.ViewPort.GetTemporaryRowHeight(2) ?? 0, Is.EqualTo(0));

            manager.CancelResize(view);
        }

        [Test]
        public void RowResizeManager_EndResize_UnhidesManuallyHiddenRow()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.SheetViews.ActiveSheetView;
            var manager = (RowResizeManager)spread.RowResizeManager;

            ws.DefaultRowHeight = 20;
            ws.Rows[1].IsHidden = true;

            Assert.That(ws.Rows[1].IsHidden, Is.True);
            Assert.That(ws.Rows.IsRowVisible(1), Is.False);

            view.ViewPort.ResetRowLocations();

            // Begin resize to unhide row 1 (located at Y = 20)
            manager.BeginResize(view, 1, 20);
            manager.Resize(view, 45); // Drag down by 25px
            manager.EndResize(view);

            Assert.That(ws.Rows[1].Height, Is.EqualTo(25));
            Assert.That(ws.Rows[1].IsHidden, Is.False);
            Assert.That(ws.Rows.IsRowVisible(1), Is.True);
        }

        [Test]
        public void AutoFilter_AppliesAndClearsVisibility()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];

            ws.SetValue(0, 0, "Name");
            ws.SetValue(1, 0, "Alice");
            ws.SetValue(2, 0, "Bob");
            ws.SetValue(3, 0, "Charlie");

            ws.AutoFilter.SetRange(new CellRange(0, 0, 4, 1));
            ws.AutoFilter.SetFilter(0, new TextFilter(TextFilterOperator.Equals, "Bob"));

            Assert.That(ws.Rows.IsRowVisible(1), Is.False, "Alice should be filtered out");
            Assert.That(ws.Rows.IsRowVisible(2), Is.True, "Bob should be visible");
            Assert.That(ws.Rows.IsRowVisible(3), Is.False, "Charlie should be filtered out");

            ws.AutoFilter.ClearAll();

            Assert.That(ws.Rows.IsRowVisible(1), Is.True);
            Assert.That(ws.Rows.IsRowVisible(2), Is.True);
            Assert.That(ws.Rows.IsRowVisible(3), Is.True);
        }
    }
}
