using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Data;
using DevBrewLabs.Spreadsheet.Sorting;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class BoundAndUnboundSortingTests
    {
        public class TestCustomer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public decimal Salary { get; set; }

            public TestCustomer(int id, string name, int age, decimal salary)
            {
                Id = id;
                Name = name;
                Age = age;
                Salary = salary;
            }
        }

        [Test]
        public void IsBound_ReturnsTrue_WhenBoundToValidList()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            Assert.That(sheet.IsBound, Is.False);

            var list = new List<TestCustomer>
            {
                new TestCustomer(1, "Alice", 30, 50000),
                new TestCustomer(2, "Bob", 25, 60000)
            };

            sheet.DataSource = list;
            Assert.That(sheet.IsBound, Is.True);

            sheet.DataSource = null;
            Assert.That(sheet.IsBound, Is.False);
        }

        [Test]
        public void BoundSorting_SortsByColumnAscending_WithoutMutatingUnderlyingObjects()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            var list = new List<TestCustomer>
            {
                new TestCustomer(1, "Charlie", 35, 80000),
                new TestCustomer(2, "Alice", 28, 60000),
                new TestCustomer(3, "Bob", 32, 75000)
            };

            sheet.DataSource = list;
            sheet.Columns[0].DataMap = new PropertyDataMap("Name");
            sheet.Columns[1].DataMap = new PropertyDataMap("Salary");

            // Sort by Name Ascending (Column 0)
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(0, true));
            sheet.Sort(options);

            // Row 0 should be Alice
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo("Alice"));
            Assert.That(sheet.GetValue(0, 1), Is.EqualTo(60000m));

            // Row 1 should be Bob
            Assert.That(sheet.GetValue(1, 0), Is.EqualTo("Bob"));
            Assert.That(sheet.GetValue(1, 1), Is.EqualTo(75000m));

            // Row 2 should be Charlie
            Assert.That(sheet.GetValue(2, 0), Is.EqualTo("Charlie"));
            Assert.That(sheet.GetValue(2, 1), Is.EqualTo(80000m));

            // Verify underlying list order was NOT modified (Object 0 is still Charlie)
            Assert.That(list[0].Name, Is.EqualTo("Charlie"));
            Assert.That(list[1].Name, Is.EqualTo("Alice"));
            Assert.That(list[2].Name, Is.EqualTo("Bob"));
        }

        [Test]
        public void MultiLevelBoundSorting_SortsCorrectly()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            var list = new List<TestCustomer>
            {
                new TestCustomer(1, "Smith", 40, 50000),
                new TestCustomer(2, "Johnson", 25, 70000),
                new TestCustomer(3, "Smith", 22, 90000),
                new TestCustomer(4, "Smith", 35, 60000)
            };

            sheet.DataSource = list;
            sheet.Columns[0].DataMap = new PropertyDataMap("Name");
            sheet.Columns[1].DataMap = new PropertyDataMap("Age");

            // Level 1: Name Ascending (Column 0)
            // Level 2: Age Ascending (Column 1)
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(0, true));
            options.SortLevels.Add(new SortInfo(1, true));

            sheet.Sort(options);

            // Row 0: Johnson, 25
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo("Johnson"));
            Assert.That(sheet.GetValue(0, 1), Is.EqualTo(25));

            // Row 1: Smith, 22
            Assert.That(sheet.GetValue(1, 0), Is.EqualTo("Smith"));
            Assert.That(sheet.GetValue(1, 1), Is.EqualTo(22));

            // Row 2: Smith, 35
            Assert.That(sheet.GetValue(2, 0), Is.EqualTo("Smith"));
            Assert.That(sheet.GetValue(2, 1), Is.EqualTo(35));

            // Row 3: Smith, 40
            Assert.That(sheet.GetValue(3, 0), Is.EqualTo("Smith"));
            Assert.That(sheet.GetValue(3, 1), Is.EqualTo(40));
        }

        [Test]
        public void HybridSorting_KeepsUnboundValuesAttachedToBoundRecords()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            var list = new List<TestCustomer>
            {
                new TestCustomer(1, "Charlie", 35, 80000),
                new TestCustomer(2, "Alice", 28, 60000)
            };

            sheet.DataSource = list;
            sheet.Columns[0].DataMap = new PropertyDataMap("Name");
            sheet.Columns[1].DataMap = new PropertyDataMap("Salary");

            // Unbound Notes Column (Column 2)
            sheet.Columns[2].DataMap = null;
            sheet.SetValue(0, 2, "Charlie's Note");
            sheet.SetValue(1, 2, "Alice's Note");

            // Sort by Name Ascending (Column 0)
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(0, true));
            sheet.Sort(options);

            // Row 0 should be Alice with Alice's Note
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo("Alice"));
            Assert.That(sheet.GetValue(0, 2), Is.EqualTo("Alice's Note"));

            // Row 1 should be Charlie with Charlie's Note
            Assert.That(sheet.GetValue(1, 0), Is.EqualTo("Charlie"));
            Assert.That(sheet.GetValue(1, 2), Is.EqualTo("Charlie's Note"));
        }

        [Test]
        public void UnboundSorting_Sorts2DArrayCorrectly()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            sheet.RowCount = 4;
            sheet.ColumnCount = 2;

            object[,] data = new object[,]
            {
                { "Orange", 30 },
                { "Apple", 50 },
                { "Banana", 20 }
            };

            sheet.Load(data);

            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(0, true)); // Sort by Fruit Name Ascending

            sheet.Sort(options);

            Assert.That(sheet.GetValue(0, 0), Is.EqualTo("Apple"));
            Assert.That(sheet.GetValue(0, 1), Is.EqualTo(50));

            Assert.That(sheet.GetValue(1, 0), Is.EqualTo("Banana"));
            Assert.That(sheet.GetValue(1, 1), Is.EqualTo(20));

            Assert.That(sheet.GetValue(2, 0), Is.EqualTo("Orange"));
            Assert.That(sheet.GetValue(2, 1), Is.EqualTo(30));
        }

        [Test]
        public void SubRangeBoundSorting_OnlySortsSelectedRowSlice()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            var list = new List<TestCustomer>
            {
                new TestCustomer(1, "Zack", 20, 10000),   // Row 0 - untouched
                new TestCustomer(2, "Charlie", 35, 80000),// Row 1 - sorted
                new TestCustomer(3, "Alice", 28, 60000),  // Row 2 - sorted
                new TestCustomer(4, "Bob", 32, 75000),    // Row 3 - sorted
                new TestCustomer(5, "Aaron", 45, 90000)   // Row 4 - untouched
            };

            sheet.DataSource = list;
            sheet.Columns[0].DataMap = new PropertyDataMap("Name");

            // Sort only rows 1 to 3 (3 rows) by Name Ascending
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(0, true));
            sheet.SortRange(new CellRange(1, 0, 3, 1), options);

            // Row 0 should still be Zack
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo("Zack"));

            // Rows 1-3 should be sorted: Alice, Bob, Charlie
            Assert.That(sheet.GetValue(1, 0), Is.EqualTo("Alice"));
            Assert.That(sheet.GetValue(2, 0), Is.EqualTo("Bob"));
            Assert.That(sheet.GetValue(3, 0), Is.EqualTo("Charlie"));

            // Row 4 should still be Aaron
            Assert.That(sheet.GetValue(4, 0), Is.EqualTo("Aaron"));
        }

        [Test]
        public void BoundaryClamping_SortRangeExceedingCollectionCount_ClampsSafely()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];

            var list = new List<TestCustomer>
            {
                new TestCustomer(1, "Charlie", 35, 80000),
                new TestCustomer(2, "Alice", 28, 60000),
                new TestCustomer(3, "Bob", 32, 75000)
            };

            sheet.DataSource = list;
            sheet.Columns[0].DataMap = new PropertyDataMap("Name");

            // Request sort on range spanning rows 0 to 100 (way beyond count of 3)
            var options = new SortOptions();
            options.SortLevels.Add(new SortInfo(0, true));
            sheet.SortRange(new CellRange(0, 0, 100, 1), options);

            // Successfully sorts within valid collection bounds without crashing
            Assert.That(sheet.GetValue(0, 0), Is.EqualTo("Alice"));
            Assert.That(sheet.GetValue(1, 0), Is.EqualTo("Bob"));
            Assert.That(sheet.GetValue(2, 0), Is.EqualTo("Charlie"));
        }
    }
}
