using System.Threading;
using System.Windows;
using NUnit.Framework;
using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Filtering.Conditions;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.UI;
using DevBrewLabs.WPF.Spreadsheet.UI.Managers;
using DevBrewLabs.WPF.Spreadsheet.Rendering;

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
        public void ViewPort_GetRowLocation_CalculatesCorrectOffset_WithFilteredRows()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
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
        }

        [Test]
        public void RowResizeManager_Resize_UpdatesTargetRowHeight()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (RowResizeManager)spread.RowResizeManager;

            ws.DefaultRowHeight = 20;

            view.ViewPort.ResetRowLocations();

            // Begin resize on row 0 (located at Y = 0)
            manager.BeginResize(view, 0, 0);
            manager.Resize(view, 35); // Drag down to 35
            manager.EndResize(view);

            Assert.That(ws.Rows[0].Height, Is.EqualTo(35));
        }

        [Test]
        public void RowResizeManager_Resize_ClampsResizeLineAtZeroSizeWhenDraggedAbove()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (RowResizeManager)spread.RowResizeManager;

            ws.DefaultRowHeight = 20;

            view.ViewPort.ResetRowLocations();

            // Begin resize on row 1 (located at Y = 20)
            manager.BeginResize(view, 1, 20);
            manager.Resize(view, 5); // Drag above row top (Y = 20)

            // Resize line should not go beyond the row's top location (Y = 20)
            Assert.That(manager.ResizeLine.Y1, Is.EqualTo(20));
            Assert.That(manager.ResizeLine.Y2, Is.EqualTo(20));

            manager.EndResize(view);
            Assert.That(ws.Rows[1].Height, Is.EqualTo(0));
        }

        [Test]
        public void RowResizeManager_Resize_ClampsResizeLineAtBottomBoundWhenDraggedBeyond()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (RowResizeManager)spread.RowResizeManager;

            ws.DefaultRowHeight = 20;
            view.RowHeadersSurface.Height = 300;
            view.RowHeadersSurface.Measure(new Size(100, 300));
            view.RowHeadersSurface.Arrange(new Rect(0, 0, 100, 300));

            view.ViewPort.ResetRowLocations();

            // Begin resize on row 0 (located at Y = 0)
            manager.BeginResize(view, 0, 0);
            manager.Resize(view, 450); // Drag beyond surface height (300)

            // Resize line should not go beyond the surface's bottom bound (Y = 300)
            Assert.That(manager.ResizeLine.Y1, Is.EqualTo(300));
            Assert.That(manager.ResizeLine.Y2, Is.EqualTo(300));

            manager.EndResize(view);
            Assert.That(ws.Rows[0].Height, Is.EqualTo(300));
        }

        [Test]
        public void RowResizeManager_EndResize_UnhidesManuallyHiddenRow()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
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
        public void RowResizeManager_EndResize_UnhidesFilteredRow()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (RowResizeManager)spread.RowResizeManager;

            ws.DefaultRowHeight = 20;
            ws.Rows[1].IsFilteredOut = true;

            Assert.That(ws.Rows[1].IsFilteredOut, Is.True);
            Assert.That(ws.Rows.IsRowVisible(1), Is.False);

            view.ViewPort.ResetRowLocations();

            // Begin resize on hidden row 1 at location Y = 20
            manager.BeginResize(view, 1, 20);
            manager.Resize(view, 45); // Drag down by 25px
            manager.EndResize(view);

            Assert.That(ws.Rows[1].Height, Is.EqualTo(25));
            Assert.That(ws.Rows[1].IsFilteredOut, Is.False);
            Assert.That(ws.Rows.IsRowVisible(1), Is.True);
        }

        [Test]
        public void ColumnResizeManager_Resize_UpdatesTargetColumnWidth()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (ColumnResizeManager)spread.ColumnResizeManager;

            ws.DefaultColumnWidth = 80;

            view.ViewPort.ResetColumnLocations();

            // Begin resize on column 0 (located at X = 0)
            manager.BeginResize(view, 0, 0);
            manager.Resize(view, 120);
            manager.EndResize(view);

            Assert.That(ws.Columns[0].Width, Is.EqualTo(120));
        }

        [Test]
        public void ColumnResizeManager_Resize_ClampsResizeLineAtZeroSizeWhenDraggedLeft()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (ColumnResizeManager)spread.ColumnResizeManager;

            ws.DefaultColumnWidth = 80;

            view.ViewPort.ResetColumnLocations();

            // Begin resize on column 1 (located at X = 80)
            manager.BeginResize(view, 1, 80);
            manager.Resize(view, 25); // Drag left of column left edge (X = 80)

            // Resize line should not go beyond the column's left location (X = 80)
            Assert.That(manager.ResizeLine.X1, Is.EqualTo(80));
            Assert.That(manager.ResizeLine.X2, Is.EqualTo(80));

            manager.EndResize(view);
            Assert.That(ws.Columns[1].Width, Is.EqualTo(0));
        }

        [Test]
        public void ColumnResizeManager_Resize_ClampsResizeLineAtRightBoundWhenDraggedBeyond()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var manager = (ColumnResizeManager)spread.ColumnResizeManager;

            ws.DefaultColumnWidth = 80;
            view.ColumnHeadersSurface.Width = 400;
            view.ColumnHeadersSurface.Measure(new Size(400, 30));
            view.ColumnHeadersSurface.Arrange(new Rect(0, 0, 400, 30));

            view.ViewPort.ResetColumnLocations();

            // Begin resize on column 0 (located at X = 0)
            manager.BeginResize(view, 0, 0);
            manager.Resize(view, 600); // Drag beyond surface width (400)

            // Resize line should not go beyond the surface's right bound (X = 400)
            Assert.That(manager.ResizeLine.X1, Is.EqualTo(400));
            Assert.That(manager.ResizeLine.X2, Is.EqualTo(400));

            manager.EndResize(view);
            Assert.That(ws.Columns[0].Width, Is.EqualTo(400));
        }

        [Test]
        public void RowHeadersSurface_HitTest_ConsecutiveHiddenRows_UnhidesLastHiddenRowFirst()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var surface = new RowHeadersSurface(view);

            ws.DefaultRowHeight = 20;
            // Row 0 is visible (0..20)
            // Rows 1, 2, 3, 4 are filtered out (hidden at 20)
            // Row 5 is visible (20..40)
            ws.Rows[1].IsFilteredOut = true;
            ws.Rows[2].IsFilteredOut = true;
            ws.Rows[3].IsFilteredOut = true;
            ws.Rows[4].IsFilteredOut = true;

            view.ViewPort.ResetRowLocations();
            view.ViewPort.CalculateVisibleRange();

            // Hovering at Y = 18 (upper half of double line near bottom edge of Row 0) -> Row 0 resize bar
            var hitRow0 = surface.HitTest(new Point(10, 18));
            Assert.That(hitRow0.Element, Is.EqualTo(VisualElement.RowHeaderResizeBar));
            Assert.That(hitRow0.Row, Is.EqualTo(0));

            // Hovering at Y = 22 (lower half of double line) -> LAST hidden row in the block (Row 4)
            var hitLastHidden = surface.HitTest(new Point(10, 22));
            Assert.That(hitLastHidden.Element, Is.EqualTo(VisualElement.RowHeaderResizeBar));
            Assert.That(hitLastHidden.Row, Is.EqualTo(4), "Should unhide the last consecutive hidden row first (Row 4)");
        }

        [Test]
        public void RowHeadersSurface_HitTest_ConsecutiveHiddenRowsAtTop_UnhidesLastHiddenRowFirst()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var surface = new RowHeadersSurface(view);

            ws.DefaultRowHeight = 20;
            // Rows 0, 1, 2 are hidden at the top of the sheet
            // Row 3 is visible
            ws.Rows[0].IsHidden = true;
            ws.Rows[1].IsHidden = true;
            ws.Rows[2].IsHidden = true;

            view.ViewPort.ResetRowLocations();
            view.ViewPort.CalculateVisibleRange();

            // Hovering at Y = 2 (top of header) -> LAST hidden row in top block (Row 2)
            var hitTopBlock = surface.HitTest(new Point(10, 2));
            Assert.That(hitTopBlock.Element, Is.EqualTo(VisualElement.RowHeaderResizeBar));
            Assert.That(hitTopBlock.Row, Is.EqualTo(2), "Should unhide the last hidden row in the top block first (Row 2)");
        }

        [Test]
        public void ColumnHeadersSurface_HitTest_ConsecutiveHiddenColumns_UnhidesLastHiddenColumnFirst()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var view = (SheetView)spread.Sheets.ActiveSheet;
            var surface = new ColumnHeadersSurface(view);

            ws.DefaultColumnWidth = 80;
            // Column 0 is visible (0..80)
            // Columns 1, 2, 3 are hidden (width 0)
            // Column 4 is visible (80..160)
            ws.Columns[1].Width = 0;
            ws.Columns[2].Width = 0;
            ws.Columns[3].Width = 0;

            view.ViewPort.ResetColumnLocations();
            view.ViewPort.CalculateVisibleRange();

            // Hovering at X = 78 (left half of double line) -> Column 0 resize bar
            var hitCol0 = surface.HitTest(new Point(78, 10));
            Assert.That(hitCol0.Element, Is.EqualTo(VisualElement.ColumnHeaderResizeBar));
            Assert.That(hitCol0.Column, Is.EqualTo(0));

            // Hovering at X = 82 (right half of double line) -> LAST hidden column in the block (Column 3)
            var hitLastHidden = surface.HitTest(new Point(82, 10));
            Assert.That(hitLastHidden.Element, Is.EqualTo(VisualElement.ColumnHeaderResizeBar));
            Assert.That(hitLastHidden.Column, Is.EqualTo(3), "Should unhide the last consecutive hidden column first (Column 3)");
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
