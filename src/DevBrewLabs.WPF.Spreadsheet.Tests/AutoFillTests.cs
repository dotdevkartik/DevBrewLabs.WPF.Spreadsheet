using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Core;
using DevBrewLabs.WPF.Spreadsheet;
using NUnit.Framework;
using System.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class AutoFillTests
    {
        [Test]
        public void AutoFill_SingleNumber_NoCtrl_CopiesValue()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetValue(0, 0, 42);

            var src = new CellRange(0, 0, 1, 1);
            var tgt = new CellRange(0, 0, 4, 1);

            ws.AutoFill(src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetValue(1, 0), Is.EqualTo(42));
            Assert.That(ws.GetValue(2, 0), Is.EqualTo(42));
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(42));
        }

        [Test]
        public void AutoFill_SingleNumber_WithCtrl_IncrementsValue()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetValue(0, 0, 10);

            var src = new CellRange(0, 0, 1, 1);
            var tgt = new CellRange(0, 0, 4, 1);

            ws.AutoFill(src, tgt, isCtrlPressed: true);

            Assert.That(ws.GetValue(1, 0), Is.EqualTo(11));
            Assert.That(ws.GetValue(2, 0), Is.EqualTo(12));
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(13));
        }

        [Test]
        public void AutoFill_MultiNumberLinearSeries_ExtrapolatesCorrectly()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetValue(0, 0, 2);
            ws.SetValue(1, 0, 4);

            var src = new CellRange(0, 0, 2, 1);
            var tgt = new CellRange(0, 0, 5, 1);

            ws.AutoFill(src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetValue(2, 0), Is.EqualTo(6));
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(8));
            Assert.That(ws.GetValue(4, 0), Is.EqualTo(10));
        }

        [Test]
        public void AutoFill_HorizontalLinearSeries_ExtrapolatesRight()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetValue(0, 0, 10);
            ws.SetValue(0, 1, 20);

            var src = new CellRange(0, 0, 1, 2);
            var tgt = new CellRange(0, 0, 1, 4);

            ws.AutoFill(src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetValue(0, 2), Is.EqualTo(30));
            Assert.That(ws.GetValue(0, 3), Is.EqualTo(40));
        }

        [Test]
        public void AutoFill_Formula_ShiftsReferencesRelatively()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetFormula(0, 1, "=A1*2");

            var src = new CellRange(0, 1, 1, 1);
            var tgt = new CellRange(0, 1, 4, 1);

            ws.AutoFill(src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetFormula(1, 1), Is.EqualTo("=A2*2"));
            Assert.That(ws.GetFormula(2, 1), Is.EqualTo("=A3*2"));
            Assert.That(ws.GetFormula(3, 1), Is.EqualTo("=A4*2"));
        }

        [Test]
        public void AutoFill_AlphaNumeric_IncrementsTrailingNumber()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetValue(0, 0, "Item 01");

            var src = new CellRange(0, 0, 1, 1);
            var tgt = new CellRange(0, 0, 3, 1);

            ws.AutoFill(src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetValue(1, 0), Is.EqualTo("Item 02"));
            Assert.That(ws.GetValue(2, 0), Is.EqualTo("Item 03"));
        }

        [Test]
        public void AutoFill_DayOfWeek_CyclesDays()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;
            ws.SetValue(0, 0, "Friday");

            var src = new CellRange(0, 0, 1, 1);
            var tgt = new CellRange(0, 0, 3, 1);

            ws.AutoFill(src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetValue(1, 0), Is.EqualTo("Saturday"));
            Assert.That(ws.GetValue(2, 0), Is.EqualTo("Sunday"));
        }

        [Test]
        public void AutoFill_ExecuteAutoFill_SupportsUndoRedo()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            ws.SetValue(0, 0, 1);
            ws.SetValue(1, 0, 2);

            var src = new CellRange(0, 0, 2, 1);
            var tgt = new CellRange(0, 0, 4, 1);

            spread.AutoFillManager.ExecuteAutoFill(sheetView, src, tgt, isCtrlPressed: false);

            Assert.That(ws.GetValue(2, 0), Is.EqualTo(3));
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(4));

            // Undo
            spread.UndoRedoManager.Undo();
            Assert.That(ws.GetValue(2, 0), Is.Null);
            Assert.That(ws.GetValue(3, 0), Is.Null);

            // Redo
            spread.UndoRedoManager.Redo();
            Assert.That(ws.GetValue(2, 0), Is.EqualTo(3));
            Assert.That(ws.GetValue(3, 0), Is.EqualTo(4));
        }
    }
}
