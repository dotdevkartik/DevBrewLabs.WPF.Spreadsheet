using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SparklineCellTypeTests
    {
        [Test]
        public void SparklineCellType_DefaultProperties()
        {
            var cellType = new SparklineCellType();

            Assert.That(cellType.Type, Is.EqualTo(SparklineType.Line));
            Assert.That(cellType.LineThickness, Is.EqualTo(1.5));
            Assert.That(cellType.MarkerSize, Is.EqualTo(3.5));
            Assert.That(cellType.Margin, Is.EqualTo(4.0));
            Assert.That(cellType.ShowMarkers, Is.False);
            Assert.That(cellType.ShowHighPoint, Is.False);
            Assert.That(cellType.ShowLowPoint, Is.False);
            Assert.That(cellType.ShowFirstPoint, Is.False);
            Assert.That(cellType.ShowLastPoint, Is.False);
            Assert.That(cellType.ShowNegativePoints, Is.False);
            Assert.That(cellType.ShowZeroAxis, Is.False);
            Assert.That(cellType.ManualMin, Is.Null);
            Assert.That(cellType.ManualMax, Is.Null);
            Assert.That(cellType.CreateEditor(null), Is.TypeOf<SparklineCellEditor>());
        }

        [Test]
        public void SparklineCellType_ParseDataPoints_DoubleArrayAndList()
        {
            var cellType = new SparklineCellType();

            double[] arr = new[] { 10.5, 20.0, -5.2, 40.8 };
            var resultArr = cellType.ParseDataPoints(arr);
            Assert.That(resultArr, Is.EqualTo(new[] { 10.5, 20.0, -5.2, 40.8 }));

            var list = new List<double> { 1.0, 2.0, 3.0 };
            var resultList = cellType.ParseDataPoints(list);
            Assert.That(resultList, Is.EqualTo(new[] { 1.0, 2.0, 3.0 }));
        }

        [Test]
        public void SparklineCellType_ParseDataPoints_FloatIntDecimalCollections()
        {
            var cellType = new SparklineCellType();

            // Floats
            float[] floats = new[] { 1.5f, 2.5f, 3.5f };
            var resultFloats = cellType.ParseDataPoints(floats);
            Assert.That(resultFloats, Is.EqualTo(new[] { 1.5, 2.5, 3.5 }));

            // Integers
            int[] ints = new[] { 10, 20, 30, -5 };
            var resultInts = cellType.ParseDataPoints(ints);
            Assert.That(resultInts, Is.EqualTo(new[] { 10.0, 20.0, 30.0, -5.0 }));

            // Decimals
            decimal[] decimals = new[] { 12.34m, 56.78m };
            var resultDecimals = cellType.ParseDataPoints(decimals);
            Assert.That(resultDecimals, Is.EqualTo(new[] { 12.34, 56.78 }));
        }

        [Test]
        public void SparklineCellType_ParseDataPoints_DelimitedStrings()
        {
            var cellType = new SparklineCellType();

            // Comma delimited
            var resComma = cellType.ParseDataPoints("10, 25.5, -8, 42");
            Assert.That(resComma, Is.EqualTo(new[] { 10.0, 25.5, -8.0, 42.0 }));

            // Semicolon delimited
            var resSemi = cellType.ParseDataPoints("10;20;30;40");
            Assert.That(resSemi, Is.EqualTo(new[] { 10.0, 20.0, 30.0, 40.0 }));

            // Space delimited
            var resSpace = cellType.ParseDataPoints("5 15 25 35");
            Assert.That(resSpace, Is.EqualTo(new[] { 5.0, 15.0, 25.0, 35.0 }));

            // Mixed whitespace and commas
            var resMixed = cellType.ParseDataPoints(" 1.1 ,  2.2 ; 3.3 \t 4.4 \n 5.5 ");
            Assert.That(resMixed, Is.EqualTo(new[] { 1.1, 2.2, 3.3, 4.4, 5.5 }));
        }

        [Test]
        public void SparklineCellType_ParseDataPoints_SingleNumberAndInvalidStrings()
        {
            var cellType = new SparklineCellType();

            // Single number
            Assert.That(cellType.ParseDataPoints(42.5), Is.EqualTo(new[] { 42.5 }));
            Assert.That(cellType.ParseDataPoints("99.9"), Is.EqualTo(new[] { 99.9 }));

            // Empty / whitespace strings
            Assert.That(cellType.ParseDataPoints(""), Is.Empty);
            Assert.That(cellType.ParseDataPoints("   "), Is.Empty);

            // Text with invalid tokens filtered out
            var mixedText = cellType.ParseDataPoints("10, abc, 20, def, 30");
            Assert.That(mixedText, Is.EqualTo(new[] { 10.0, 20.0, 30.0 }));
        }

        [Test]
        public void SparklineCellType_ParseDataPoints_NullFallbackToStaticData()
        {
            var cellType = new SparklineCellType
            {
                StaticData = new[] { 100.0, 200.0, 300.0 }
            };

            Assert.That(cellType.ParseDataPoints(null), Is.EqualTo(new[] { 100.0, 200.0, 300.0 }));
            Assert.That(cellType.ParseDataPoints(""), Is.EqualTo(new[] { 100.0, 200.0, 300.0 }));
        }

        [Test]
        public void SparklineCellType_ParseDataPoints_CustomDataSelector()
        {
            var cellType = new SparklineCellType
            {
                DataSelector = obj =>
                {
                    if (obj is Tuple<int, int, int> tuple)
                    {
                        return new double[] { tuple.Item1, tuple.Item2, tuple.Item3 };
                    }
                    return null;
                }
            };

            var data = cellType.ParseDataPoints(Tuple.Create(10, 20, 30));
            Assert.That(data, Is.EqualTo(new[] { 10.0, 20.0, 30.0 }));
        }

        [Test]
        public void SparklineCellType_CustomBrushesAndConfig()
        {
            var customSeries = new SolidColorBrush(Colors.Purple);
            var customNegative = new SolidColorBrush(Colors.DarkOrange);
            var customHigh = new SolidColorBrush(Colors.LimeGreen);

            var cellType = new SparklineCellType
            {
                Type = SparklineType.Column,
                SeriesBrush = customSeries,
                NegativeBrush = customNegative,
                HighPointBrush = customHigh,
                ShowHighPoint = true,
                ShowNegativePoints = true,
                ShowZeroAxis = true,
                LineThickness = 2.5,
                Margin = 6.0,
                ManualMin = -50.0,
                ManualMax = 150.0
            };

            Assert.That(cellType.Type, Is.EqualTo(SparklineType.Column));
            Assert.That(cellType.SeriesBrush, Is.SameAs(customSeries));
            Assert.That(cellType.NegativeBrush, Is.SameAs(customNegative));
            Assert.That(cellType.HighPointBrush, Is.SameAs(customHigh));
            Assert.That(cellType.ShowHighPoint, Is.True);
            Assert.That(cellType.ShowNegativePoints, Is.True);
            Assert.That(cellType.ShowZeroAxis, Is.True);
            Assert.That(cellType.LineThickness, Is.EqualTo(2.5));
            Assert.That(cellType.Margin, Is.EqualTo(6.0));
            Assert.That(cellType.ManualMin, Is.EqualTo(-50.0));
            Assert.That(cellType.ManualMax, Is.EqualTo(150.0));
        }

        [Test]
        public void SparklineCellType_AllTypesEnumeration()
        {
            var cellType = new SparklineCellType();

            cellType.Type = SparklineType.Line;
            Assert.That(cellType.Type, Is.EqualTo(SparklineType.Line));

            cellType.Type = SparklineType.Column;
            Assert.That(cellType.Type, Is.EqualTo(SparklineType.Column));

            cellType.Type = SparklineType.WinLoss;
            Assert.That(cellType.Type, Is.EqualTo(SparklineType.WinLoss));

            cellType.Type = SparklineType.Area;
            Assert.That(cellType.Type, Is.EqualTo(SparklineType.Area));
        }

        [Test]
        public void SparklineCellEditor_FormatsAndParsesSeries()
        {
            var cellType = new SparklineCellType();
            var editor = (SparklineCellEditor)cellType.CreateEditor(null);

            var context = new TestEditorContext
            {
                Value = new double[] { 10.5, 20.0, -5.2, 40.8 },
                Trigger = EditTrigger.DoubleClick
            };

            editor.StartEdit(context);
            Assert.That(editor.Text, Is.EqualTo("10.5, 20, -5.2, 40.8"));

            editor.Text = "1.5, 2.5, 3.5";
            var result = editor.GetValue();
            Assert.That(result, Is.InstanceOf<double[]>());
            Assert.That((double[])result, Is.EqualTo(new double[] { 1.5, 2.5, 3.5 }));
        }

        private class TestEditorContext : IEditorContext
        {
            public DevBrewLabs.WPF.Spreadsheet.ISheetView? SheetView { get; set; }
            public DevBrewLabs.Spreadsheet.IWorksheet? Worksheet { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
            public System.Windows.Rect CellBounds { get; set; }
            public double ZoomFactor { get; set; } = 1.0;
            public object? Value { get; set; }
            public string? Formula { get; set; }
            public string? FormattedText { get; set; }
            public DevBrewLabs.Spreadsheet.IStyle? Style { get; set; }
            public DevBrewLabs.Spreadsheet.Formatters.IFormatter? Formatter { get; set; }
            public EditTrigger Trigger { get; set; }
            public string? InitialInput { get; set; }
        }
    }
}
