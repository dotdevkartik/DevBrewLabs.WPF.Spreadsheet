using DevBrewLabs.Spreadsheet.Core;
using NUnit.Framework;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    public class SpreadsheetDataParserTests
    {
        [Test]
        public void ParseTextData_StandardTsv_ParsesGrid()
        {
            string tsv = "A\tB\tC\r\n1\t2\t3\r\n";
            var data = SpreadsheetDataParser.ParseTextData(tsv, '\t');

            Assert.That(data, Is.Not.Null);
            Assert.That(data.GetLength(0), Is.EqualTo(2));
            Assert.That(data.GetLength(1), Is.EqualTo(3));
            Assert.That(data[0, 0], Is.EqualTo("A"));
            Assert.That(data[0, 1], Is.EqualTo("B"));
            Assert.That(data[0, 2], Is.EqualTo("C"));
            Assert.That(data[1, 0], Is.EqualTo("1"));
            Assert.That(data[1, 1], Is.EqualTo("2"));
            Assert.That(data[1, 2], Is.EqualTo("3"));
        }

        [Test]
        public void ParseTextData_MultilineCellInQuotes_PreservesInternalNewlines()
        {
            string tsv = "Title\t\"Line 1\nLine 2\"\r\nSecond\tValue\r\n";
            var data = SpreadsheetDataParser.ParseTextData(tsv, '\t');

            Assert.That(data, Is.Not.Null);
            Assert.That(data.GetLength(0), Is.EqualTo(2)); // Still 2 rows, not 3!
            Assert.That(data.GetLength(1), Is.EqualTo(2));
            Assert.That(data[0, 0], Is.EqualTo("Title"));
            Assert.That(data[0, 1], Is.EqualTo("Line 1\nLine 2"));
            Assert.That(data[1, 0], Is.EqualTo("Second"));
            Assert.That(data[1, 1], Is.EqualTo("Value"));
        }

        [Test]
        public void ParseTextData_EscapedQuotes_UnescapesCorrectly()
        {
            string csv = "Saying,Author\r\n\"He said \"\"Hello\"\" to me\",John\r\n";
            var data = SpreadsheetDataParser.ParseTextData(csv, ',');

            Assert.That(data, Is.Not.Null);
            Assert.That(data.GetLength(0), Is.EqualTo(2));
            Assert.That(data[1, 0], Is.EqualTo("He said \"Hello\" to me"));
            Assert.That(data[1, 1], Is.EqualTo("John"));
        }
    }
}
