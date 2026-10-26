using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.Commands;
using NUnit.Framework;
using System;
using System.Data;
using System.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RowColumnInsertDeleteTests
    {
        private sealed class DummyFilterCondition : IFilterCondition
        {
            public bool Match(FilterContext context) => true;
        }

        [Test]
        public void Rows_Insert_ShiftsDataAndRemapsFormulas()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, 100);
            ws.SetValue(1, 0, 200);
            ws.SetFormula(2, 0, "=A1+A2"); // references row 0 (A1) and row 1 (A2)

            // Insert 1 row at index 1
            ws.Rows.Insert(1, 1);

            Assert.That(ws.GetValue(0, 0), Is.EqualTo(100));
            Assert.That(ws.GetValue(1, 0), Is.Null);
            Assert.That(ws.GetValue(2, 0), Is.EqualTo(200));
            // The formula that was at row 2 is now at row 3
            Assert.That(ws.GetFormula(3, 0), Is.EqualTo("=A1+A3"));
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(300));

            // Verify DependencyManager tracks the new coordinate (A3 is row 2, col 0)
            ws.SetValue(2, 0, 500);
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(600));
        }

        [Test]
        public void Rows_Remove_ShiftsDataAndSetsRefErrorOnDeletedReference()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, 10);
            ws.SetValue(1, 0, 20);
            ws.SetValue(2, 0, 30);
            ws.SetFormula(3, 0, "=A1+A2+A3");

            // Delete row 1 (A2)
            ws.Rows.Remove(1, 1);

            Assert.That(ws.GetValue(0, 0), Is.EqualTo(10));
            Assert.That(ws.GetValue(1, 0), Is.EqualTo(30));
            // Formula shifted to row 2, and A2 became #REF!, A3 became A2
            Assert.That(ws.GetFormula(2, 0), Is.EqualTo("=A1+#REF!+A2"));
            Assert.That(ws.GetValue(2, 0), Is.EqualTo("#REF!"));
        }

        [Test]
        public void Columns_Insert_ShiftsDataRight()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, "A");
            ws.SetValue(0, 1, "B");
            ws.SetFormula(0, 2, "=A1+B1");

            // Insert 1 column at index 1
            ws.Columns.Insert(1, 1);

            Assert.That(ws.GetValue(0, 0), Is.EqualTo("A"));
            Assert.That(ws.GetValue(0, 1), Is.Null);
            Assert.That(ws.GetValue(0, 2), Is.EqualTo("B"));
            Assert.That(ws.GetFormula(0, 3), Is.EqualTo("=A1+C1"));
        }

        [Test]
        public void Columns_Remove_ShiftsDataLeft()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, 10);
            ws.SetValue(0, 1, 20);
            ws.SetValue(0, 2, 30);
            ws.SetFormula(0, 3, "=A1+B1+C1");

            // Remove column 1 (B)
            ws.Columns.Remove(1, 1);

            Assert.That(ws.GetValue(0, 0), Is.EqualTo(10));
            Assert.That(ws.GetValue(0, 1), Is.EqualTo(30));
            Assert.That(ws.GetFormula(0, 2), Is.EqualTo("=A1+#REF!+B1"));
            Assert.That(ws.GetValue(0, 2), Is.EqualTo("#REF!"));
        }

        [Test]
        public void Rows_Insert_ThrowsWhenBound()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add("Alice");
            ws.DataSource = dt;

            Assert.That(ws.IsBound, Is.True);
            Assert.Throws<InvalidOperationException>(() => ws.Rows.Insert(0, 1));
            Assert.Throws<InvalidOperationException>(() => ws.Rows.Remove(0, 1));
        }

        [Test]
        public void InsertRowsCommand_SupportsUndoRedo()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            ws.SetValue(0, 0, 10);
            ws.SetValue(1, 0, 20);

            sheetView.SelectCell(1, 0);

            var insertCmd = new InsertRowsCommand(spread);
            Assert.That(insertCmd.CanExecute(sheetView), Is.True);
            insertCmd.Execute(sheetView);

            // Row inserted at index 1
            Assert.That(ws.GetValue(1, 0), Is.Null);
            Assert.That(ws.GetValue(2, 0), Is.EqualTo(20));

            // Undo
            spread.UndoRedoManager.Undo();
            Assert.That(ws.GetValue(1, 0), Is.EqualTo(20));

            // Redo
            spread.UndoRedoManager.Redo();
            Assert.That(ws.GetValue(1, 0), Is.Null);
            Assert.That(ws.GetValue(2, 0), Is.EqualTo(20));
        }

        [Test]
        public void DeleteRowsCommand_SupportsUndoRedo()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            ws.SetValue(0, 0, "Top");
            ws.SetValue(1, 0, "Middle");
            ws.SetValue(2, 0, "Bottom");

            sheetView.SelectCell(1, 0);

            var deleteCmd = new DeleteRowsCommand(spread);
            Assert.That(deleteCmd.CanExecute(sheetView), Is.True);
            deleteCmd.Execute(sheetView);

            Assert.That(ws.GetValue(1, 0), Is.EqualTo("Bottom"));

            // Undo restores "Middle"
            spread.UndoRedoManager.Undo();
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("Top"));
            Assert.That(ws.GetValue(1, 0), Is.EqualTo("Middle"));
            Assert.That(ws.GetValue(2, 0), Is.EqualTo("Bottom"));

            // Redo removes it again
            spread.UndoRedoManager.Redo();
            Assert.That(ws.GetValue(1, 0), Is.EqualTo("Bottom"));
        }

        [Test]
        public void AutoFilter_ShiftsOnRowAndColumnInsert()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.AutoFilter.SetRange(new CellRange(1, 1, 5, 3));
            ws.AutoFilter.SetFilter(2, new DummyFilterCondition());

            ws.Rows.Insert(0, 2);
            Assert.That(ws.AutoFilter.Range.TopRow, Is.EqualTo(3));
            Assert.That(ws.AutoFilter.Range.RowCount, Is.EqualTo(5));

            ws.Columns.Insert(1, 1);
            Assert.That(ws.AutoFilter.Range.LeftColumn, Is.EqualTo(2));
            Assert.That(ws.AutoFilter.GetColumnFilter(2), Is.Null);
            Assert.That(ws.AutoFilter.GetColumnFilter(3), Is.Not.Null);
        }
    }
}
