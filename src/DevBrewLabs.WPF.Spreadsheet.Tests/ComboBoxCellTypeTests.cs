using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Components;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ComboBoxCellTypeTests
    {
        public class PriorityItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public override string ToString() => Name;
        }

        [Test]
        public void ComboBoxCellType_DefaultProperties()
        {
            var cellType = new ComboBoxCellType();

            Assert.That(cellType.ShowDropDownButton, Is.True);
            Assert.That(cellType.DropDownButtonWidth, Is.EqualTo(18.0));
            Assert.That(cellType.IsEditable, Is.False);
            Assert.That(cellType.MaxDropDownHeight, Is.EqualTo(220.0));
            Assert.That(cellType.MaxDropDownWidth, Is.Null);
            Assert.That(cellType.SupportsEditing, Is.True);
        }

        [Test]
        public void ComboBoxCellType_GetElements_ReturnsComboBoxDropDownButton()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var cellType = new ComboBoxCellType();
            ws.SetCellType(0, 0, cellType);

            var elements = cellType.GetElements(spread.Sheets.ActiveSheet, 0, 0).ToList();
            Assert.That(elements.Count, Is.EqualTo(1));
            Assert.That(elements[0], Is.TypeOf<ComboBoxDropDownButton>());

            // Verify element reuse
            Assert.That(cellType.GetElements(spread.Sheets.ActiveSheet, 0, 0).First(), Is.SameAs(elements[0]));

            // When disabled, returns empty
            cellType.ShowDropDownButton = false;
            Assert.That(cellType.GetElements(spread.Sheets.ActiveSheet, 0, 0).Count(), Is.EqualTo(0));
        }

        [Test]
        public void ComboBoxDropDownButton_GetBounds_CalculatesRightAlignedRect()
        {
            var cellType = new ComboBoxCellType();
            var button = new ComboBoxDropDownButton(cellType);

            var cellRect = new Rect(100, 50, 120, 30);
            var bounds1x = button.GetBounds(cellRect, 1.0);

            Assert.That(bounds1x.Right, Is.EqualTo(cellRect.Right));
            Assert.That(bounds1x.Width, Is.EqualTo(18.0));
            Assert.That(bounds1x.Height, Is.EqualTo(20.0));

            var bounds2x = button.GetBounds(cellRect, 2.0);
            Assert.That(bounds2x.Right, Is.EqualTo(cellRect.Right));
            Assert.That(bounds2x.Width, Is.EqualTo(36.0));
        }

        [Test]
        public void ComboBoxCellType_GetContentRect_ExcludesButtonWidth()
        {
            var spread = new Spread();
            var cellType = new ComboBoxCellType { ShowDropDownButton = true };
            var cellRect = new Rect(0, 0, 100, 25);

            var contentRect = cellType.GetContentRect(spread.Sheets.ActiveSheet, 0, 0, cellRect, 1.0);
            Assert.That(contentRect.Width, Is.EqualTo(100 - 18.0));

            cellType.ShowDropDownButton = false;
            var fullRect = cellType.GetContentRect(spread.Sheets.ActiveSheet, 0, 0, cellRect, 1.0);
            Assert.That(fullRect.Width, Is.EqualTo(100));
        }

        [Test]
        public void ComboBoxCellType_ResolveDisplayText_WithStrings()
        {
            var cellType = new ComboBoxCellType
            {
                ItemsSource = new string[] { "Active", "Pending", "Completed", "Cancelled" }
            };

            Assert.That(cellType.ResolveDisplayText("Pending"), Is.EqualTo("Pending"));
            Assert.That(cellType.ResolveDisplayText("pending"), Is.EqualTo("Pending"));
            Assert.That(cellType.ResolveDisplayText("Unknown"), Is.EqualTo("Unknown"));
            Assert.That(cellType.ResolveDisplayText(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ComboBoxCellType_ResolveDisplayText_WithComplexObjects()
        {
            var items = new List<PriorityItem>
            {
                new PriorityItem { Id = 1, Name = "Low" },
                new PriorityItem { Id = 2, Name = "Medium" },
                new PriorityItem { Id = 3, Name = "High" },
                new PriorityItem { Id = 4, Name = "Critical" }
            };

            var cellType = new ComboBoxCellType
            {
                ItemsSource = items,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id"
            };

            // Resolves by SelectedValue (Id = 3 -> "High")
            Assert.That(cellType.ResolveDisplayText(3), Is.EqualTo("High"));
            Assert.That(cellType.ResolveDisplayText(1), Is.EqualTo("Low"));
        }

        [Test]
        public void ComboBoxCellType_CreatesComboBoxCellEditor()
        {
            var cellType = new ComboBoxCellType
            {
                ItemsSource = new[] { "Option A", "Option B" },
                IsEditable = true,
                MaxDropDownHeight = 180.0
            };

            var editor = cellType.CreateEditor(null) as ComboBoxCellEditor;
            Assert.That(editor, Is.Not.Null);
            Assert.That(editor.ItemsSource, Is.SameAs(cellType.ItemsSource));
            Assert.That(editor.IsEditable, Is.True);
            Assert.That(editor.MaxDropDownHeight, Is.EqualTo(180.0));
        }

        [Test]
        public void ComboBoxCellEditor_StartEdit_SyncsSelectionAndValue()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var items = new[] { "North", "South", "East", "West" };

            var cellType = new ComboBoxCellType { ItemsSource = items };
            ws.SetCellType(0, 0, cellType);
            ws.SetValue(0, 0, "East");

            spread.BeginEdit(0, 0);

            Assert.That(spread.EditingManager.IsEditing, Is.True);
            Assert.That(spread.EditingManager.ActiveEditor, Is.TypeOf<ComboBoxCellEditor>());

            var editor = (ComboBoxCellEditor)spread.EditingManager.ActiveEditor;
            Assert.That(editor.Text, Is.EqualTo("East"));
            Assert.That(editor.DropDownList.SelectedItem, Is.EqualTo("East"));
            Assert.That(editor.DropDownList.SelectedIndex, Is.EqualTo(2));
            Assert.That(editor.IsReadOnly, Is.True); // Non-editable by default

            // Select another item and commit
            editor.DropDownList.SelectedIndex = 0; // "North"
            spread.EndEdit(commitChanges: true);

            Assert.That(spread.EditingManager.IsEditing, Is.False);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("North"));
        }

        [Test]
        public void ComboBoxCellEditor_ComplexObject_CommitsSelectedValue()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var items = new List<PriorityItem>
            {
                new PriorityItem { Id = 10, Name = "Bronze" },
                new PriorityItem { Id = 20, Name = "Silver" },
                new PriorityItem { Id = 30, Name = "Gold" }
            };

            var cellType = new ComboBoxCellType
            {
                ItemsSource = items,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id"
            };

            ws.SetCellType(0, 0, cellType);
            ws.SetValue(0, 0, 20); // Silver

            spread.BeginEdit(0, 0);
            var editor = (ComboBoxCellEditor)spread.EditingManager.ActiveEditor;
            Assert.That(editor.Text, Is.EqualTo("Silver"));

            // Switch to Gold (Id = 30)
            editor.DropDownList.SelectedIndex = 2;
            spread.EndEdit(commitChanges: true);

            Assert.That(ws.GetValue(0, 0), Is.EqualTo(30));
        }

        [Test]
        public void SpreadDropDownListBox_MoveSelection_And_SelectItem()
        {
            var list = new SpreadDropDownListBox
            {
                ItemsSource = new[] { "One", "Two", "Three", "Four" }
            };

            Assert.That(list.ItemsCount, Is.EqualTo(4));

            list.SelectedIndex = 1;
            list.MoveSelection(1);
            Assert.That(list.SelectedIndex, Is.EqualTo(2));

            list.MoveSelection(-1);
            Assert.That(list.SelectedIndex, Is.EqualTo(1));

            list.MoveSelectionFirst();
            Assert.That(list.SelectedIndex, Is.EqualTo(0));

            list.MoveSelectionLast();
            Assert.That(list.SelectedIndex, Is.EqualTo(3));

            bool found = list.SelectItemByText("Two");
            Assert.That(found, Is.True);
            Assert.That(list.SelectedIndex, Is.EqualTo(1));
            Assert.That(list.SelectedItem, Is.EqualTo("Two"));
        }

        [Test]
        public void ComboBoxCellEditor_Typing_DoesNotAutoSelectOrMutateTypedText()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var items = new[] { "Banana Split", "Banana Bread", "Blueberry" };

            var cellType = new ComboBoxCellType { ItemsSource = items, IsEditable = true, ShowSuggestions = true };
            ws.SetCellType(0, 0, cellType);

            spread.BeginEdit(0, 0);
            var editor = (ComboBoxCellEditor)spread.EditingManager.ActiveEditor;

            // User types "Ban"
            editor.Text = "Ban";

            // Verify typed text is preserved and not overwritten by auto-selection
            Assert.That(editor.Text, Is.EqualTo("Ban"));

            spread.EndEdit(commitChanges: true);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("Ban"));
        }

        [Test]
        public void ComboBoxCellEditor_DirectTyping_InEditableMode_SavesCustomTypedValue()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var items = new[] { "Apple", "Banana", "Cherry" };

            var cellType = new ComboBoxCellType { ItemsSource = items, IsEditable = true };
            ws.SetCellType(0, 0, cellType);
            ws.SetValue(0, 0, "Apple");

            spread.BeginEdit(0, 0);
            var editor = (ComboBoxCellEditor)spread.EditingManager.ActiveEditor;
            Assert.That(editor.IsReadOnly, Is.False);

            // User types custom value not in list
            editor.Text = "Dragonfruit";
            spread.EndEdit(commitChanges: true);

            Assert.That(spread.EditingManager.IsEditing, Is.False);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("Dragonfruit"));
        }

        [Test]
        public void ComboBoxCellEditor_DirectTyping_InEditableMode_SavesMatchedItemValue()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var items = new List<PriorityItem>
            {
                new PriorityItem { Id = 1, Name = "Low" },
                new PriorityItem { Id = 2, Name = "Medium" },
                new PriorityItem { Id = 3, Name = "High" },
                new PriorityItem { Id = 4, Name = "Critical" }
            };

            var cellType = new ComboBoxCellType
            {
                ItemsSource = items,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                IsEditable = true
            };
            ws.SetCellType(0, 0, cellType);
            ws.SetValue(0, 0, 1); // Low

            spread.BeginEdit(0, 0);
            var editor = (ComboBoxCellEditor)spread.EditingManager.ActiveEditor;

            // User types "Critical"
            editor.Text = "Critical";
            spread.EndEdit(commitChanges: true);

            Assert.That(spread.EditingManager.IsEditing, Is.False);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo(4)); // SelectedValue of "Critical"
        }

        [Test]
        public void ComboBoxCellType_SearchMemberPath_MatchesSearchProperty()
        {
            var items = new List<DepartmentItem>
            {
                new DepartmentItem { DeptId = 101, DeptName = "Human Resources", Code = "HR" },
                new DepartmentItem { DeptId = 102, DeptName = "Information Technology", Code = "IT" },
                new DepartmentItem { DeptId = 103, DeptName = "Research & Development", Code = "RND" }
            };

            var list = new SpreadDropDownListBox
            {
                ItemsSource = items,
                DisplayMemberPath = "DeptName",
                SelectedValuePath = "DeptId",
                SearchMemberPath = "Code"
            };

            Assert.That(list.GetItemSearchText(items[1]), Is.EqualTo("IT"));
            Assert.That(list.GetItemDisplayText(items[1]), Is.EqualTo("Information Technology"));

            bool matched = list.SelectItemByText("IT", exactMatch: false);
            Assert.That(matched, Is.True);
            Assert.That(list.SelectedIndex, Is.EqualTo(1));
            Assert.That(list.SelectedValue, Is.EqualTo(102));
        }

        public class DepartmentItem
        {
            public int DeptId { get; set; }
            public string DeptName { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }

        [Test]
        public void ComboBoxDropDownButton_OnClick_BeginsEdit()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var cellType = new ComboBoxCellType { ItemsSource = new[] { "Alpha", "Beta" } };
            ws.SetCellType(0, 0, cellType);

            var button = cellType.GetElements(spread.Sheets.ActiveSheet, 0, 0).OfType<ComboBoxDropDownButton>().First();
            button.OnClick(spread.Sheets.ActiveSheet, 0, 0);

            Assert.That(spread.EditingManager.IsEditing, Is.True);
            Assert.That(spread.EditingManager.ActiveEditor, Is.TypeOf<ComboBoxCellEditor>());
        }

        [Test]
        public void ComboBoxCellEditor_NonEditable_TypeAhead_JumpsToMatchingItem_WithoutFilteringList()
        {
            var spread = new Spread();
            var ws = (Worksheet)spread.WorkBook.WorkSheets[0];
            var items = new[] { "Alpha", "Beta", "Gamma", "Delta" };

            var cellType = new ComboBoxCellType { ItemsSource = items, IsEditable = false, ShowSuggestions = true };
            ws.SetCellType(0, 0, cellType);
            ws.SetValue(0, 0, "Alpha");

            // Start edit via direct typing "G"
            spread.EditingManager.BeginEdit((SheetView)spread.Sheets.ActiveSheet, 0, 0, EditTrigger.DirectTyping, initialInput: "G");
            var editor = (ComboBoxCellEditor)spread.EditingManager.ActiveEditor;

            Assert.That(editor.IsReadOnly, Is.True);
            Assert.That(editor.DropDownList.SelectedItem, Is.EqualTo("Gamma"));
            Assert.That(editor.Text, Is.EqualTo("Gamma"));
            Assert.That(editor.DropDownList.ItemsCount, Is.EqualTo(4)); // Full list preserved

            spread.EndEdit(commitChanges: true);
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("Gamma"));
        }
    }
}
