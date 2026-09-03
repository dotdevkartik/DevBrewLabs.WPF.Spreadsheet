using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HyperlinkCellTypeTests
    {
        private class TestWpfCommand : ICommand
        {
            public bool Executed { get; private set; }
            public object? ExecutedParameter { get; private set; }
            public bool CanExecuteReturn { get; set; } = true;

            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter) => CanExecuteReturn;

            public void Execute(object? parameter)
            {
                Executed = true;
                ExecutedParameter = parameter;
            }
        }

        [Test]
        public void HyperlinkCellType_DefaultProperties()
        {
            var cellType = new HyperlinkCellType();

            Assert.That(cellType.UnderlineMode, Is.EqualTo(HyperlinkUnderlineMode.Always));
            Assert.That(cellType.OpenUrlOnClick, Is.True);
            Assert.That(cellType.TrackVisited, Is.True);
            Assert.That(cellType.SupportsEditing, Is.True);
            Assert.That(cellType.CreateEditor(null), Is.TypeOf<TextCellEditor>());
        }

        [Test]
        public void HyperlinkCellType_GetElements_ReturnsHyperlinkElement()
        {
            var cellType = new HyperlinkCellType();
            var elements = cellType.GetElements(null, 0, 0).ToList();

            Assert.That(elements.Count, Is.EqualTo(1));
            Assert.That(elements[0], Is.TypeOf<HyperlinkElement>());
            Assert.That(cellType.GetElements(null, 0, 0).First(), Is.SameAs(elements[0]));
        }

        [Test]
        public void HyperlinkCellType_DistinctInstances_DoNotShareElements()
        {
            var cellType1 = new HyperlinkCellType();
            var cellType2 = new HyperlinkCellType();

            var element1 = cellType1.GetElements(null, 0, 0).First();
            var element2 = cellType2.GetElements(null, 0, 0).First();

            Assert.That(element1, Is.Not.SameAs(element2));
        }

        [Test]
        public void HyperlinkElement_Cursor_IsHand()
        {
            var cellType = new HyperlinkCellType();
            var element = cellType.GetElements(null, 0, 0).First();

            Assert.That(element.Cursor, Is.EqualTo(Cursors.Hand));
        }

        [Test]
        public void HyperlinkElement_GetBounds_MatchesCellRect()
        {
            var cellType = new HyperlinkCellType();
            var element = cellType.GetElements(null, 0, 0).First();
            var cellRect = new Rect(10, 20, 150, 30);

            var bounds = element.GetBounds(cellRect, 1.0);
            Assert.That(bounds, Is.EqualTo(cellRect));
        }

        [Test]
        public void HyperlinkCellType_ResolveTargetUrl_HandlesVariousFormats()
        {
            var cellType = new HyperlinkCellType();

            // Direct URL string
            Assert.That(cellType.ResolveTargetUrl("https://devbrewlabs.com"), Is.EqualTo("https://devbrewlabs.com"));

            // www. auto-prepends https://
            Assert.That(cellType.ResolveTargetUrl("www.github.com"), Is.EqualTo("https://www.github.com"));

            // Uri object
            var uri = new Uri("https://microsoft.com");
            Assert.That(cellType.ResolveTargetUrl(uri), Is.EqualTo("https://microsoft.com"));

            // Explicit LinkAddress overrides cell value
            cellType.LinkAddress = "https://custom.target.com";
            Assert.That(cellType.ResolveTargetUrl("Some Display Text"), Is.EqualTo("https://custom.target.com"));

            // Null returns empty
            cellType.LinkAddress = null;
            Assert.That(cellType.ResolveTargetUrl(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void HyperlinkCellType_ResolveDisplayText_PrioritizesExplicitText()
        {
            var cellType = new HyperlinkCellType();

            // Value used as display text
            Assert.That(cellType.ResolveDisplayText("https://devbrewlabs.com"), Is.EqualTo("https://devbrewlabs.com"));

            // Explicit Text overrides value
            cellType.Text = "Visit Website";
            Assert.That(cellType.ResolveDisplayText("https://devbrewlabs.com"), Is.EqualTo("Visit Website"));

            // Falls back to LinkAddress if value is null
            cellType.Text = null;
            cellType.LinkAddress = "https://fallback.com";
            Assert.That(cellType.ResolveDisplayText(null), Is.EqualTo("https://fallback.com"));
        }

        [Test]
        public void HyperlinkCellType_OnClick_FiresClickAndNavigateEvents()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];
            var cellType = new HyperlinkCellType { OpenUrlOnClick = false }; // Suppress actual browser launch
            sheet.SetCellType(0, 0, cellType);
            sheet.SetValue(0, 0, "https://github.com/dotdevkartik");

            bool clickFired = false;
            bool navigateFired = false;
            string? capturedUrl = null;

            cellType.Click += (s, e) =>
            {
                clickFired = true;
                capturedUrl = e.Url;
            };

            cellType.RequestNavigate += (s, e) =>
            {
                navigateFired = true;
            };

            cellType.OnClick(spread.Sheets.ActiveSheet, 0, 0);

            Assert.That(clickFired, Is.True);
            Assert.That(navigateFired, Is.True);
            Assert.That(capturedUrl, Is.EqualTo("https://github.com/dotdevkartik"));
        }

        [Test]
        public void HyperlinkCellType_OnClick_ExecutesCommand()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];
            var command = new TestWpfCommand();
            var cellType = new HyperlinkCellType
            {
                Command = command,
                OpenUrlOnClick = false
            };
            sheet.SetCellType(1, 1, cellType);
            sheet.SetValue(1, 1, "https://devbrewlabs.com");

            cellType.OnClick(spread.Sheets.ActiveSheet, 1, 1);

            Assert.That(command.Executed, Is.True);
            Assert.That(command.ExecutedParameter, Is.TypeOf<CellHyperlinkClickedEventArgs>());
            var args = (CellHyperlinkClickedEventArgs)command.ExecutedParameter!;
            Assert.That(args.Row, Is.EqualTo(1));
            Assert.That(args.Column, Is.EqualTo(1));
            Assert.That(args.Url, Is.EqualTo("https://devbrewlabs.com"));
        }

        [Test]
        public void HyperlinkCellType_TrackVisited_MaintainsVisitedState()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];
            var cellType = new HyperlinkCellType { OpenUrlOnClick = false, TrackVisited = true };
            sheet.SetCellType(0, 0, cellType);
            sheet.SetValue(0, 0, "https://docs.devbrewlabs.com");

            Assert.That(cellType.IsVisited("https://docs.devbrewlabs.com"), Is.False);

            cellType.OnClick(spread.Sheets.ActiveSheet, 0, 0);

            Assert.That(cellType.IsVisited("https://docs.devbrewlabs.com"), Is.True);

            cellType.ClearVisited();
            Assert.That(cellType.IsVisited("https://docs.devbrewlabs.com"), Is.False);
        }

        [Test]
        public void HyperlinkCellType_DefaultBrushes_MatchExcelStandard()
        {
            var defaultColor = ((SolidColorBrush)SheetUtils.HyperlinkBrush).Color;
            Assert.That(defaultColor.A, Is.EqualTo(255));
            Assert.That(defaultColor.R, Is.EqualTo(5));
            Assert.That(defaultColor.G, Is.EqualTo(99));
            Assert.That(defaultColor.B, Is.EqualTo(193));

            var visitedColor = ((SolidColorBrush)SheetUtils.HyperlinkVisitedBrush).Color;
            Assert.That(visitedColor.A, Is.EqualTo(255));
            Assert.That(visitedColor.R, Is.EqualTo(149));
            Assert.That(visitedColor.G, Is.EqualTo(79));
            Assert.That(visitedColor.B, Is.EqualTo(114));

            var hoverColor = ((SolidColorBrush)SheetUtils.HyperlinkHoverBrush).Color;
            Assert.That(hoverColor.A, Is.EqualTo(255));
            Assert.That(hoverColor.R, Is.EqualTo(0));
            Assert.That(hoverColor.G, Is.EqualTo(32));
            Assert.That(hoverColor.B, Is.EqualTo(96));

            var visitedHoverColor = ((SolidColorBrush)SheetUtils.HyperlinkVisitedHoverBrush).Color;
            Assert.That(visitedHoverColor.A, Is.EqualTo(255));
            Assert.That(visitedHoverColor.R, Is.EqualTo(90));
            Assert.That(visitedHoverColor.G, Is.EqualTo(36));
            Assert.That(visitedHoverColor.B, Is.EqualTo(63));
        }

        [Test]
        public void HyperlinkElement_GetBounds_ConstrainedToText_NotFullCell()
        {
            var spread = new Spread();
            var sheet = (Worksheet)spread.WorkBook.WorkSheets[0];
            var cellType = new HyperlinkCellType { OpenUrlOnClick = false };
            sheet.SetCellType(0, 0, cellType);
            sheet.SetValue(0, 0, "Link");

            var element = (HyperlinkElement)cellType.GetElements(spread.Sheets.ActiveSheet, 0, 0).First();
            var cellRect = new Rect(0, 0, 300, 25);

            var bounds = element.GetBounds(cellRect, 1.0);

            // Bounds must be non-empty and contained within the cell rectangle
            Assert.That(bounds.IsEmpty, Is.False);
            Assert.That(bounds.Left, Is.GreaterThanOrEqualTo(cellRect.Left));
            Assert.That(bounds.Right, Is.LessThanOrEqualTo(cellRect.Right));

            // Bounds width must be constrained to the short text "Link", significantly smaller than the 300px cell width
            Assert.That(bounds.Width, Is.LessThan(100.0));
            Assert.That(bounds.Width, Is.GreaterThan(10.0));

            // Clicking inside the text bounds triggers hit
            Assert.That(bounds.Contains(new Point(bounds.Left + 5, bounds.Top + 5)), Is.True);

            // Clicking in the empty whitespace to the right (x=250) is OUTSIDE text bounds
            Assert.That(bounds.Contains(new Point(250, 12)), Is.False);
        }
    }
}
