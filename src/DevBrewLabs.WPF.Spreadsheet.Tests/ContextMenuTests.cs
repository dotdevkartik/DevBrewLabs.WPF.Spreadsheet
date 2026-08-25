using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DevBrewLabs.Spreadsheet;
using NUnit.Framework;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ContextMenuTests
    {
        [Test]
        public void Spread_AllowContextMenu_DefaultsToTrue()
        {
            var spread = new Spread();
            Assert.That(spread.AllowContextMenu, Is.True);
        }

        [Test]
        public void Spread_CustomContextMenuProperties_DefaultToNull()
        {
            var spread = new Spread();
            Assert.That(spread.CellContextMenu, Is.Null);
            Assert.That(spread.RowHeaderContextMenu, Is.Null);
            Assert.That(spread.ColumnHeaderContextMenu, Is.Null);
            Assert.That(spread.SheetTabContextMenu, Is.Null);
        }

        [Test]
        public void ContextMenuManager_CreateCellContextMenu_ContainsStandardItems()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var menu = spread.ContextMenuManager.CreateCellContextMenu(sheetView);

            Assert.That(menu, Is.Not.Null);
            var menuItems = menu.Items.OfType<MenuItem>().Select(m => m.Header?.ToString()).ToList();

            Assert.That(menuItems, Does.Contain("Cut"));
            Assert.That(menuItems, Does.Contain("Copy"));
            Assert.That(menuItems, Does.Contain("Paste"));
            Assert.That(menuItems, Does.Contain("Clear Contents"));
            Assert.That(menuItems, Does.Contain("Merge Cells"));
            Assert.That(menuItems, Does.Contain("Unmerge Cells"));
            Assert.That(menuItems, Does.Contain("Sort A to Z"));
            Assert.That(menuItems, Does.Contain("Sort Z to A"));

            
        }

        

        [Test]
        public void ContextMenuManager_CreateRowHeaderContextMenu_ContainsStandardItems()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var menu = spread.ContextMenuManager.CreateRowHeaderContextMenu(sheetView);

            Assert.That(menu, Is.Not.Null);
            var menuItems = menu.Items.OfType<MenuItem>().Select(m => m.Header?.ToString()).ToList();

            Assert.That(menuItems, Does.Contain("Cut"));
            Assert.That(menuItems, Does.Contain("Copy"));
            Assert.That(menuItems, Does.Contain("Paste"));
            Assert.That(menuItems, Does.Contain("Clear Contents"));
            Assert.That(menuItems, Does.Contain("Hide Rows"));
            Assert.That(menuItems, Does.Contain("Unhide Rows"));
            Assert.That(menuItems, Does.Contain("AutoFit Row Height"));
        }

        [Test]
        public void ContextMenuManager_CreateColumnHeaderContextMenu_ContainsStandardItems()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var menu = spread.ContextMenuManager.CreateColumnHeaderContextMenu(sheetView);

            Assert.That(menu, Is.Not.Null);
            var menuItems = menu.Items.OfType<MenuItem>().Select(m => m.Header?.ToString()).ToList();

            Assert.That(menuItems, Does.Contain("Cut"));
            Assert.That(menuItems, Does.Contain("Copy"));
            Assert.That(menuItems, Does.Contain("Paste"));
            Assert.That(menuItems, Does.Contain("Clear Contents"));
            Assert.That(menuItems, Does.Contain("Hide Columns"));
            Assert.That(menuItems, Does.Contain("Unhide Columns"));
            Assert.That(menuItems, Does.Contain("AutoFit Column Width"));
            Assert.That(menuItems, Does.Contain("Sort A to Z"));
            Assert.That(menuItems, Does.Contain("Sort Z to A"));
        }

        [Test]
        public void ContextMenuManager_CreateSheetTabContextMenu_ContainsStandardItems()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var menu = spread.ContextMenuManager.CreateSheetTabContextMenu(sheetView, 0);

            Assert.That(menu, Is.Not.Null);
            var menuItems = menu.Items.OfType<MenuItem>().Select(m => m.Header?.ToString()).ToList();

            Assert.That(menuItems, Does.Contain("Insert Sheet"));
            Assert.That(menuItems, Does.Contain("Delete Sheet"));
            Assert.That(menuItems, Does.Contain("Duplicate Sheet"));
        }

        [Test]
        public void Spread_Cut_CopiesAndClearsCellValues_WithUndoRedo()
        {
            var spread = new Spread();
            var ws = spread.WorkBook.WorkSheets.ActiveSheet;
            ws.SetValue(0, 0, "TestCut");
            ws.SetValue(0, 1, 123);

            spread.SelectRange(0, 0, 1, 2);
            spread.Cut();

            // Values in selection should be cleared
            Assert.That(ws.GetValue(0, 0), Is.Null);
            Assert.That(ws.GetValue(0, 1), Is.Null);

            // Undo should restore values
            spread.Undo();
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("TestCut"));
            Assert.That(ws.GetValue(0, 1), Is.EqualTo(123));

            // Redo should clear again
            spread.Redo();
            Assert.That(ws.GetValue(0, 0), Is.Null);
            Assert.That(ws.GetValue(0, 1), Is.Null);
        }

        [Test]
        public void Spread_ClearContents_ClearsValues_WithUndoRedo()
        {
            var spread = new Spread();
            var ws = spread.WorkBook.WorkSheets.ActiveSheet;
            ws.SetValue(2, 3, "ClearMe");

            spread.SelectCell(2, 3);
            spread.ClearContents();

            Assert.That(ws.GetValue(2, 3), Is.Null);

            spread.Undo();
            Assert.That(ws.GetValue(2, 3), Is.EqualTo("ClearMe"));
        }

        [Test]
        public void Spread_ContextMenuOpening_CanCancelOpening()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            bool eventFired = false;

            spread.ContextMenuOpening += (s, e) =>
            {
                eventFired = true;
                Assert.That(e.Region, Is.EqualTo(SpreadContextMenuRegion.Cells));
                e.Cancel = true;
            };

            var hitTest = new SpreadHitTestResult
            {
                Sheet = sheetView,
                Row = 1,
                Column = 1,
                Element = SheetElement.Cell
            };

            spread.ContextMenuManager.ShowContextMenu(sheetView, hitTest, spread);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void Spread_ContextMenuOpening_CanModifyContextMenu()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            bool itemAdded = false;

            spread.ContextMenuOpening += (s, e) =>
            {
                e.ContextMenu.Items.Add(new MenuItem { Header = "Custom Action" });
                itemAdded = true;
            };

            var hitTest = new SpreadHitTestResult
            {
                Sheet = sheetView,
                Row = 0,
                Column = 0,
                Element = SheetElement.Cell
            };

            spread.ContextMenuManager.ShowContextMenu(sheetView, hitTest, spread);
            Assert.That(itemAdded, Is.True);
        }

        [Test]
        public void ContextMenuManager_HideAndUnhideRows_UpdatesVisibility()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var ws = (Worksheet)spread.WorkBook.WorkSheets.ActiveSheet;

            spread.SelectRow(2);
            var menu = spread.ContextMenuManager.CreateRowHeaderContextMenu(sheetView);
            var hideItem = menu.Items.OfType<MenuItem>().First(m => m.Header?.ToString() == "Hide Rows");

            hideItem.Command.Execute(hideItem.CommandParameter);
            Assert.That(ws.Rows[2].IsHidden, Is.True);

            var unhideItem = menu.Items.OfType<MenuItem>().First(m => m.Header?.ToString() == "Unhide Rows");
            unhideItem.Command.Execute(unhideItem.CommandParameter);
            Assert.That(ws.Rows[2].IsHidden, Is.False);
        }

        [Test]
        public void ContextMenuManager_HideAndUnhideColumns_UpdatesVisibility()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var ws = (Worksheet)spread.WorkBook.WorkSheets.ActiveSheet;

            spread.SelectColumn(1);
            var menu = spread.ContextMenuManager.CreateColumnHeaderContextMenu(sheetView);
            var hideItem = menu.Items.OfType<MenuItem>().First(m => m.Header?.ToString() == "Hide Columns");

            hideItem.Command.Execute(hideItem.CommandParameter);
            Assert.That(ws.Columns[1].Width, Is.EqualTo(0));

            var unhideItem = menu.Items.OfType<MenuItem>().First(m => m.Header?.ToString() == "Unhide Columns");
            unhideItem.Command.Execute(unhideItem.CommandParameter);
            Assert.That(ws.Columns[1].Width, Is.EqualTo(ws.DefaultColumnWidth));
        }

        [Test]
        public void ContextMenuManager_SheetTabOperations_AddsAndDuplicatesSheet()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet;
            var ws = spread.WorkBook.WorkSheets.ActiveSheet;
            ws.SetValue(0, 0, "DuplicateTestData");

            var menu = spread.ContextMenuManager.CreateSheetTabContextMenu(sheetView, 0);

            // Insert Sheet
            var insertItem = menu.Items.OfType<MenuItem>().First(m => m.Header?.ToString() == "Insert Sheet");
            insertItem.Command.Execute(insertItem.CommandParameter);
            Assert.That(spread.WorkBook.WorkSheets.Count, Is.EqualTo(2));

            // Duplicate Sheet
            var duplicateItem = menu.Items.OfType<MenuItem>().First(m => m.Header?.ToString() == "Duplicate Sheet");
            duplicateItem.Command.Execute(duplicateItem.CommandParameter);
            Assert.That(spread.WorkBook.WorkSheets.Count, Is.EqualTo(3));
            Assert.That(spread.WorkBook.WorkSheets.ActiveSheet.GetValue(0, 0), Is.EqualTo("DuplicateTestData"));
        }
    }
}
