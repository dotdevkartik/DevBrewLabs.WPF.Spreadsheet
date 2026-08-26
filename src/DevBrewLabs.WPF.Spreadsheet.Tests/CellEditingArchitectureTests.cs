using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using NUnit.Framework;
using System.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CellEditingArchitectureTests
    {
        [Test]
        public void BeginEdit_StartsEditing_AndCreatesTextCellEditor()
        {
            var spread = new Spread();
            var ws = spread.WorkBook.WorkSheets[0];
            ws.SetValue(0, 0, "Hello World");

            spread.BeginEdit(0, 0);

            Assert.That(spread.EditingManager.IsEditing, Is.True);
            Assert.That(spread.EditingManager.ActiveEditor, Is.TypeOf<TextCellEditor>());

            var editor = (TextCellEditor)spread.EditingManager.ActiveEditor;
            Assert.That(editor.Text, Is.EqualTo("Hello World"));
        }

        [Test]
        public void BeginEdit_WithDirectTyping_ClearsValueAndSetsInitialText()
        {
            var spread = new Spread();
            var ws = spread.WorkBook.WorkSheets[0];
            ws.SetValue(0, 0, "Existing Value");

            spread.BeginEdit(0, 0, EditTrigger.DirectTyping);

            Assert.That(spread.EditingManager.IsEditing, Is.True);
            var editor = (TextCellEditor)spread.EditingManager.ActiveEditor;
            Assert.That(editor.Text, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EndEdit_WithCommit_SavesValueToWorkSheet()
        {
            var spread = new Spread();
            var ws = spread.WorkBook.WorkSheets[0];
            ws.SetValue(0, 0, "Old");

            spread.BeginEdit(0, 0);
            var editor = (TextCellEditor)spread.EditingManager.ActiveEditor;
            editor.Text = "New Value";

            spread.EndEdit(commitChanges: true);

            Assert.That(spread.EditingManager.IsEditing, Is.False);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("New Value"));
        }

        [Test]
        public void EndEdit_WithCancel_DiscardsChanges()
        {
            var spread = new Spread();
            var ws = spread.WorkBook.WorkSheets[0];
            ws.SetValue(0, 0, "Original");

            spread.BeginEdit(0, 0);
            var editor = (TextCellEditor)spread.EditingManager.ActiveEditor;
            editor.Text = "Modified";

            spread.EndEdit(commitChanges: false);

            Assert.That(spread.EditingManager.IsEditing, Is.False);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("Original"));
        }

        [Test]
        public void CellEditStarting_CanCancel_BeginEdit()
        {
            var spread = new Spread();
            bool eventFired = false;

            spread.CellEditStarting += (s, e) =>
            {
                eventFired = true;
                e.Cancel = true;
            };

            spread.BeginEdit(0, 0);

            Assert.That(eventFired, Is.True);
            Assert.That(spread.EditingManager.IsEditing, Is.False);
        }

        [Test]
        public void CellEditEnding_CanCancel_EndEdit()
        {
            var spread = new Spread();
            bool eventFired = false;

            spread.BeginEdit(0, 0);

            spread.CellEditEnding += (s, e) =>
            {
                eventFired = true;
                e.Cancel = true;
            };

            spread.EndEdit(commitChanges: true);

            Assert.That(eventFired, Is.True);
            Assert.That(spread.EditingManager.IsEditing, Is.True); // Still editing because it was cancelled
        }

        [Test]
        public void CellEditEnded_FiresOnCommitAndCancel()
        {
            var spread = new Spread();
            bool? wasCommitted = null;

            spread.CellEditEnded += (s, e) =>
            {
                wasCommitted = e.WasCommitted;
            };

            spread.BeginEdit(0, 0);
            spread.EndEdit(commitChanges: true);

            Assert.That(wasCommitted, Is.True);

            spread.BeginEdit(0, 0);
            spread.EndEdit(commitChanges: false);

            Assert.That(wasCommitted, Is.False);
        }

        [Test]
        public void NonEditableCellTypes_DoNotStartEditing()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];

            ws.SetCellType(0, 0, new ButtonCellType());
            ws.SetCellType(0, 1, new CheckBoxCellType());

            spread.BeginEdit(0, 0);
            Assert.That(spread.EditingManager.IsEditing, Is.False);

            spread.BeginEdit(0, 1);
            Assert.That(spread.EditingManager.IsEditing, Is.False);
        }

        [Test]
        public void NumberCellType_CreatesNumericCellEditor()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            ws.SetCellType(0, 0, new NumberCellType());
            ws.SetValue(0, 0, 123.45);

            spread.BeginEdit(0, 0);

            Assert.That(spread.EditingManager.IsEditing, Is.True);
            Assert.That(spread.EditingManager.ActiveEditor, Is.TypeOf<NumericCellEditor>());
        }

        [Test]
        public void DateCellType_CreatesDateCellEditor_AndSupportsEditing()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var dateCellType = new DateCellType();
            ws.SetCellType(0, 0, dateCellType);
            ws.SetValue(0, 0, "2026-08-26");

            // Verify elements include DatePickerButton
            var elements = System.Linq.Enumerable.ToList(dateCellType.GetElements(spread.Sheets.ActiveSheet, 0, 0));
            Assert.That(elements.Count, Is.EqualTo(1));
            Assert.That(elements[0], Is.TypeOf<Elements.DatePickerButton>());

            // Begin edit
            spread.BeginEdit(0, 0);

            Assert.That(spread.EditingManager.IsEditing, Is.True);
            Assert.That(spread.EditingManager.ActiveEditor, Is.TypeOf<DateCellEditor>());

            var editor = (DateCellEditor)spread.EditingManager.ActiveEditor;
            Assert.That(editor.Text, Is.EqualTo("2026-08-26"));

            // Change text inline and commit
            editor.Text = "2026-12-31";
            spread.EndEdit(commitChanges: true);

            Assert.That(spread.EditingManager.IsEditing, Is.False);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo(new System.DateTime(2026, 12, 31)));
        }
    }
}
