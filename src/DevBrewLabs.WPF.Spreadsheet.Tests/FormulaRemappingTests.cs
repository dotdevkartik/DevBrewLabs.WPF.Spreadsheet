using DevBrewLabs.Spreadsheet.CalcEngine.Utils;
using NUnit.Framework;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    public class FormulaRemappingTests
    {
        [Test]
        public void ShiftFormula_RelativeReference_ShiftsRowAndColumn()
        {
            string formula = "=A1 + 10";
            string shifted = FormulaRemapper.ShiftFormula(formula, 2, 3);
            Assert.That(shifted, Is.EqualTo("=D3 + 10"));
        }

        [Test]
        public void ShiftFormula_AbsoluteReferences_PreservesDollarSigns()
        {
            string formula = "=$A$1 + $B$2";
            string shifted = FormulaRemapper.ShiftFormula(formula, 5, 5);
            Assert.That(shifted, Is.EqualTo("=$A$1 + $B$2"));
        }

        [Test]
        public void ShiftFormula_MixedReferences_OnlyShiftsRelativePart()
        {
            string formula = "=$A1 + B$2";
            string shifted = FormulaRemapper.ShiftFormula(formula, 2, 3);
            Assert.That(shifted, Is.EqualTo("=$A3 + E$2"));
        }

        [Test]
        public void ShiftFormula_QuotedSheetName_PreservesSheetAndShiftsCell()
        {
            string formula = "='My Sheet'!A1 + 5";
            string shifted = FormulaRemapper.ShiftFormula(formula, 1, 1);
            Assert.That(shifted, Is.EqualTo("='My Sheet'!B2 + 5"));
        }

        [Test]
        public void ShiftFormula_RangeReference_ShiftsBothEnds()
        {
            string formula = "=SUM(A1:B5)";
            string shifted = FormulaRemapper.ShiftFormula(formula, 2, 1);
            Assert.That(shifted, Is.EqualTo("=SUM(B3:C7)"));
        }

        [Test]
        public void RemapOnRowChange_Insert_ShiftsRowsDown()
        {
            string formula = "=A5 + B10";
            // Insert 2 rows at row index 2 (1-based row 3).
            // A5 is 0-based row 4 -> shifts down to 6 (1-based 7).
            // B10 is 0-based row 9 -> shifts down to 11 (1-based 12).
            string remapped = FormulaRemapper.RemapOnRowChange(formula, 2, 2, true);
            Assert.That(remapped, Is.EqualTo("=A7 + B12"));
        }

        [Test]
        public void RemapOnRowChange_Delete_ReplacesDeletedRowWithRefError()
        {
            string formula = "=A3 + B10";
            // Delete row index 2 (which is 1-based row 3).
            // A3 is deleted -> becomes #REF!
            // B10 is 0-based row 9 -> shifts up to 8 (1-based 9).
            string remapped = FormulaRemapper.RemapOnRowChange(formula, 2, 1, false);
            Assert.That(remapped, Is.EqualTo("=#REF! + B9"));
        }

        [Test]
        public void RemapOnColumnChange_Insert_ShiftsColumnsRight()
        {
            string formula = "=C1 + D5";
            // Insert 1 column at index 1 (Col B)
            // Col C (0-based 2) shifts to D, Col D (0-based 3) shifts to E
            string remapped = FormulaRemapper.RemapOnColumnChange(formula, 1, 1, true);
            Assert.That(remapped, Is.EqualTo("=D1 + E5"));
        }

        [Test]
        public void RemapOnColumnChange_Delete_ReplacesDeletedColumnWithRefError()
        {
            string formula = "=B1 + D5";
            // Delete column 1 (Col B).
            string remapped = FormulaRemapper.RemapOnColumnChange(formula, 1, 1, false);
            Assert.That(remapped, Is.EqualTo("=#REF! + C5"));
        }
    }
}
