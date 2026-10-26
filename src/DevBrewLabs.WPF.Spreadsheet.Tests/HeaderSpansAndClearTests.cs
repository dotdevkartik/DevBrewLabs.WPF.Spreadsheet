using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet;
using NUnit.Framework;
using System.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeaderSpansAndClearTests
    {
        [Test]
        public void ColumnHeaders_AddSpan_MergesHeaderCells()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.ColumnHeaders.RowCount = 2;
            ws.ColumnHeaders.AddSpan(0, 0, 1, 3); // Merges cols 0, 1, 2 in header row 0

            Assert.That(ws.ColumnHeaders.IsCovered(0, 0), Is.False); // Master cell
            Assert.That(ws.ColumnHeaders.IsCovered(0, 1), Is.True);  // Covered
            Assert.That(ws.ColumnHeaders.IsCovered(0, 2), Is.True);  // Covered
            Assert.That(ws.ColumnHeaders.IsCovered(0, 3), Is.False); // Uncovered

            var spanRange = ws.ColumnHeaders.GetSpanCellRange(0, 1);
            Assert.That(spanRange.TopRow, Is.EqualTo(0));
            Assert.That(spanRange.LeftColumn, Is.EqualTo(0));
            Assert.That(spanRange.RowCount, Is.EqualTo(1));
            Assert.That(spanRange.ColumnCount, Is.EqualTo(3));
        }

        [Test]
        public void RowHeaders_AddSpan_MergesHeaderCells()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.RowHeaders.ColumnCount = 2;
            ws.RowHeaders.AddSpan(0, 0, 3, 1); // Merges rows 0, 1, 2 in header col 0

            Assert.That(ws.RowHeaders.IsCovered(0, 0), Is.False);
            Assert.That(ws.RowHeaders.IsCovered(1, 0), Is.True);
            Assert.That(ws.RowHeaders.IsCovered(2, 0), Is.True);
            Assert.That(ws.RowHeaders.IsCovered(3, 0), Is.False);

            var spanRange = ws.RowHeaders.GetSpanCellRange(1, 0);
            Assert.That(spanRange.TopRow, Is.EqualTo(0));
            Assert.That(spanRange.LeftColumn, Is.EqualTo(0));
            Assert.That(spanRange.RowCount, Is.EqualTo(3));
            Assert.That(spanRange.ColumnCount, Is.EqualTo(1));
        }

        [Test]
        public void WorkSheet_Clear_DataMode_ClearsOnlyValues()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, 123);
            ws.SetFormula(0, 1, "=A1*2");

            ws.Clear(WorkSheetClearMode.Data);

            Assert.That(ws.GetValue(0, 0), Is.Null);
            Assert.That(ws.GetFormula(0, 1), Is.EqualTo("=A1*2"));
        }

        [Test]
        public void WorkSheet_Clear_FormulaMode_ClearsOnlyFormulas()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, 123);
            ws.SetFormula(0, 1, "=A1*2");

            ws.Clear(WorkSheetClearMode.Formula);

            Assert.That(ws.GetValue(0, 0), Is.EqualTo(123));
            Assert.That(ws.GetFormula(0, 1), Is.Null);
        }

        [Test]
        public void WorkSheet_Clear_AllMode_ClearsValuesFormulasAndSpans()
        {
            var spread = new Spread();
            var ws = spread.Sheets.ActiveSheet.WorkSheet;

            ws.SetValue(0, 0, 123);
            ws.SetFormula(0, 1, "=A1*2");
            ws.AddSpan(0, 0, 2, 2);
            Assert.That(ws.HasSpans, Is.True);

            ws.Clear(WorkSheetClearMode.All);

            Assert.That(ws.GetValue(0, 0), Is.Null);
            Assert.That(ws.GetFormula(0, 1), Is.Null);
            Assert.That(ws.HasSpans, Is.False);
        }
    }
}
