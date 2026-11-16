using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Filtering;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Commands;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Interaction;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class V103GapAndPerformanceTests
    {
        private sealed class EqualsValueFilterCondition : IFilterCondition
        {
            private readonly object _expected;

            public EqualsValueFilterCondition(object expected)
            {
                _expected = expected;
            }

            public bool Match(FilterContext context) => Equals(context.Value, _expected);
        }

        private sealed class PercentFormatter : IFormatter
        {
            public string Format(object value) => value == null ? string.Empty : $"{value}%";
        }

        private static void RaiseKey(UIElement target, Key key)
        {
            var source = new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero);
            try
            {
                var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
                {
                    RoutedEvent = UIElement.PreviewKeyDownEvent
                };
                target.RaiseEvent(args);
            }
            finally
            {
                source.Dispose();
            }
        }

        private static IEnumerable<Drawing> FlattenDrawings(Drawing root)
        {
            if (root is DrawingGroup group)
            {
                foreach (var child in group.Children)
                {
                    foreach (var d in FlattenDrawings(child))
                        yield return d;
                }
            }
            else if (root != null)
            {
                yield return root;
            }
        }

        [Test]
        public void StylePalette_DeduplicatesIdenticalCellStyles()
        {
            var spread = new Spread();
            var palette = spread.WorkBook.StylePalette;

            var style1 = new CellStyle
            {
                BackColor = DrawingColor.Red,
                ForeColor = DrawingColor.White,
                FontSize = 16,
                Padding = new DrawingThickness(10, 4)
            };

            var style2 = new CellStyle
            {
                BackColor = DrawingColor.Red,
                ForeColor = DrawingColor.White,
                FontSize = 16,
                Padding = new DrawingThickness(10, 4, 4, 10)
            };

            ushort id1 = palette.GetOrAdd(style1);
            ushort id2 = palette.GetOrAdd(style2);

            Assert.That(style1.Equals(style2), Is.True);
            Assert.That(id2, Is.EqualTo(id1));
        }

        [Test]
        public void CellsRenderer_RendersEmptyCellWithCustomBackgroundStyle()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            // Cell (0, 0) has no value, no Row object, no Column object — only cell Style
            ws.Cells[0, 0].Style = new CellStyle { BackColor = DrawingColor.Red };

            spread.Measure(new Size(400, 300));
            spread.Arrange(new Rect(0, 0, 400, 300));

            var drawing = new DrawingGroup();
            var renderer = new CellsRenderer();
            using (var context = new RenderContext(drawing, sheetView))
            {
                renderer.OnRender(context, 0, 0, 2, 2);
            }

            var redGeometry = FlattenDrawings(drawing)
                .OfType<GeometryDrawing>()
                .FirstOrDefault(g => g.Brush is SolidColorBrush scb && scb.Color == Color.FromArgb(255, 255, 0, 0));

            Assert.That(redGeometry, Is.Not.Null);
        }

        [Test]
        public void TextRenderer_RespectsCustomStylePadding()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            ws.SetValue(0, 0, "DefaultPad");
            ws.Cells[0, 0].Style = new CellStyle
            {
                HorizontalAlignment = CellHorizontalAlignment.Left,
                VerticalAlignment = CellVerticalAlignment.Top,
                Padding = new DrawingThickness(5, 5, 5, 5)
            };

            ws.SetValue(1, 0, "LargePad");
            ws.Cells[1, 0].Style = new CellStyle
            {
                HorizontalAlignment = CellHorizontalAlignment.Left,
                VerticalAlignment = CellVerticalAlignment.Top,
                Padding = new DrawingThickness(24, 12, 5, 5)
            };

            ws.Rows[1].Height = 40;
            ws.Columns[0].Width = 150;

            spread.Measure(new Size(400, 300));
            spread.Arrange(new Rect(0, 0, 400, 300));

            var drawing = new DrawingGroup();
            var renderer = new CellsRenderer();
            using (var context = new RenderContext(drawing, sheetView))
            {
                renderer.OnRender(context, 0, 0, 1, 0);
            }

            var glyphDrawings = FlattenDrawings(drawing).OfType<GlyphRunDrawing>().ToList();
            Assert.That(glyphDrawings.Count, Is.EqualTo(2));

            double firstX = glyphDrawings[0].GlyphRun.BaselineOrigin.X;
            double secondX = glyphDrawings[1].GlyphRun.BaselineOrigin.X;
            double firstY = glyphDrawings[0].GlyphRun.BaselineOrigin.Y;
            double secondY = glyphDrawings[1].GlyphRun.BaselineOrigin.Y - ws.Rows[0].Height;

            Assert.That(secondX - firstX, Is.GreaterThanOrEqualTo(18.0));
            Assert.That(secondY - firstY, Is.GreaterThanOrEqualTo(6.0));
        }

        [Test]
        public void LockedCells_ArePreservedAcrossClearCutPasteAndAutoFill()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            ws.SetValue(0, 0, "Unlocked");
            ws.SetValue(0, 1, "LockedCell");
            ws.SetLocked(0, 1, true);

            // 1. ClearContents preserves locked cell
            sheetView.SelectRange(0, 0, 1, 2);
            sheetView.ClearContents();

            Assert.That(ws.GetValue(0, 0), Is.Null);
            Assert.That(ws.GetValue(0, 1), Is.EqualTo("LockedCell"));

            // Undo restores unlocked cell
            spread.Undo();
            Assert.That(ws.GetValue(0, 0), Is.EqualTo("Unlocked"));
            Assert.That(ws.GetValue(0, 1), Is.EqualTo("LockedCell"));

            // 2. Cut preserves locked cell
            sheetView.SelectRange(0, 0, 1, 2);
            sheetView.Cut();

            Assert.That(ws.GetValue(0, 0), Is.Null);
            Assert.That(ws.GetValue(0, 1), Is.EqualTo("LockedCell"));

            // 3. Paste skips locked target cell
            ws.SetValue(2, 0, "SourceA");
            ws.SetValue(2, 1, "SourceB");
            sheetView.SelectRange(2, 0, 1, 2);
            sheetView.Copy();

            sheetView.SelectCell(0, 0);
            sheetView.Paste();

            Assert.That(ws.GetValue(0, 0), Is.EqualTo("SourceA"));
            Assert.That(ws.GetValue(0, 1), Is.EqualTo("LockedCell"));

            // 4. AutoFill skips locked target cell
            ws.SetValue(3, 0, 1);
            ws.SetValue(4, 0, 2);
            ws.SetValue(5, 0, "KeepLocked");
            ws.SetLocked(5, 0, true);

            ws.AutoFill(new CellRange(3, 0, 2, 1), new CellRange(3, 0, 4, 1));
            Assert.That(ws.GetValue(5, 0), Is.EqualTo("KeepLocked"));
            Assert.That(Convert.ToDouble(ws.GetValue(6, 0)), Is.EqualTo(4.0));
        }

        [Test]
        public void KeyboardNavigation_SkipsHiddenAndFilteredRowsAndColumns()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;
            var interactionLayer = (CellsInteractionLayer)sheetView.CellsSurface.GetInteractionLayer();

            // Row 1 manually hidden, Row 2 filtered out
            ws.Rows[1].IsHidden = true;
            ws.SetValue(0, 0, "Header");
            ws.SetValue(1, 0, "A");
            ws.SetValue(2, 0, "HideMe");
            ws.SetValue(3, 0, "KeepMe");
            ws.AutoFilter.SetRange(new CellRange(0, 0, 4, 1));
            ws.AutoFilter.SetFilter(0, new EqualsValueFilterCondition("KeepMe"));

            // Column 1 hidden via width = 0
            ws.Columns[1].Width = 0;

            sheetView.SelectCell(0, 0);

            // Down from row 0 skips hidden row 1 & filtered row 2 -> lands on row 3
            RaiseKey(interactionLayer, Key.Down);
            Assert.That(sheetView.ActiveRow, Is.EqualTo(3));

            // Up from row 3 skips filtered row 2 & hidden row 1 -> lands on row 0
            RaiseKey(interactionLayer, Key.Up);
            Assert.That(sheetView.ActiveRow, Is.EqualTo(0));

            // Right from col 0 skips hidden col 1 -> lands on col 2
            RaiseKey(interactionLayer, Key.Right);
            Assert.That(sheetView.ActiveColumn, Is.EqualTo(2));

            // Left from col 2 skips hidden col 1 -> lands on col 0
            RaiseKey(interactionLayer, Key.Left);
            Assert.That(sheetView.ActiveColumn, Is.EqualTo(0));

            // ExpandSelectionDown & ExpandSelectionRight skip hidden/filtered rows & columns
            interactionLayer.ExpandSelectionDown();
            Assert.That(sheetView.Selection.BottomRow, Is.EqualTo(3));

            interactionLayer.ExpandSelectionRight();
            Assert.That(sheetView.Selection.RightColumn, Is.EqualTo(2));
        }

        [Test]
        public void DeleteRowsCommand_UndoRestoresStylesCellTypesSpansAndRefFormulas()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            var customStyle = new CellStyle { BackColor = DrawingColor.LightBlue };
            var checkBoxType = new CheckBoxCellType();
            var formatter = new PercentFormatter();

            ws.SetValue(1, 0, 50);
            ws.Cells[1, 0].Style = customStyle;
            ws.Cells[1, 0].CellType = checkBoxType;
            ws.Cells[1, 0].Formatter = formatter;
            ws.Cells[1, 0].Locked = true;
            ws.Rows[1].Height = 36;

            // Span spanning across row 1 and row 2 in col 1
            ws.SetValue(1, 1, "MergedAnchor");
            ws.AddSpan(1, 1, 2, 1);

            // Formula in row 3 referencing deleted row 1 (A2)
            ws.SetFormula(3, 0, "=A2*2");
            Assert.That(Convert.ToDouble(ws.GetValue(3, 0)), Is.EqualTo(100.0));

            // Cross-sheet formula referencing Sheet1!A2
            var sheet2 = spread.WorkBook.WorkSheets.AddSheet("Sheet2");
            sheet2.SetFormula(0, 0, "=Sheet1!A2+10");
            Assert.That(Convert.ToDouble(sheet2.GetValue(0, 0)), Is.EqualTo(60.0));

            sheetView.SelectRows(1, 1);
            var deleteCmd = new DeleteRowsCommand(spread);
            deleteCmd.Execute(sheetView);

            Assert.That(ws.GetFormula(2, 0), Is.EqualTo("=#REF!*2"));
            Assert.That(sheet2.GetFormula(0, 0), Is.EqualTo("=#REF!+10"));

            // Undo should restore style, cellType, formatter, locked, height, span, and broken formulas
            spread.Undo();

            Assert.That(ws.GetValue(1, 0), Is.EqualTo(50));
            Assert.That(ws.GetStyle(1, 0), Is.EqualTo(customStyle));
            Assert.That(ws.GetCellType(1, 0), Is.SameAs(checkBoxType));
            Assert.That(ws.GetFormatter(1, 0), Is.SameAs(formatter));
            Assert.That(ws.GetLocked(1, 0), Is.True);
            Assert.That(ws.Rows[1].Height, Is.EqualTo(36));

            var restoredSpan = ws.GetSpanCellRange(1, 1);
            Assert.That(restoredSpan, Is.EqualTo(new CellRange(1, 1, 2, 1)));
            Assert.That(ws.GetValue(1, 1), Is.EqualTo("MergedAnchor"));

            Assert.That(ws.GetFormula(3, 0), Is.EqualTo("=A2*2"));
            Assert.That(Convert.ToDouble(ws.GetValue(3, 0)), Is.EqualTo(100.0));
            Assert.That(sheet2.GetFormula(0, 0), Is.EqualTo("=Sheet1!A2+10"));
            Assert.That(Convert.ToDouble(sheet2.GetValue(0, 0)), Is.EqualTo(60.0));
        }

        [Test]
        public void DeleteColumnsCommand_SparseMillionRowSheet_IsFastAndUndoRestoresState()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            ws.RowCount = 1_000_000;
            var colStyle = new CellStyle { BackColor = DrawingColor.LightGreen };
            ws.Columns[1].Style = colStyle;
            ws.Columns[1].Width = 140;

            ws.SetValue(0, 1, "Top");
            ws.SetValue(500_000, 1, 999);
            ws.SetFormula(0, 2, "=B500001+1");

            sheetView.SelectColumns(1, 1);
            var deleteCmd = new DeleteColumnsCommand(spread);

            var sw = Stopwatch.StartNew();
            deleteCmd.Execute(sheetView);
            sw.Stop();

            // Sparse snapshot should complete quickly even with 1,000,000 rows
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000));
            Assert.That(ws.GetFormula(0, 1), Is.EqualTo("=#REF!+1"));

            spread.Undo();

            Assert.That(ws.Columns[1].Width, Is.EqualTo(140));
            Assert.That(ws.Columns[1].Style, Is.EqualTo(colStyle));
            Assert.That(ws.GetValue(0, 1), Is.EqualTo("Top"));
            Assert.That(Convert.ToDouble(ws.GetValue(500_000, 1)), Is.EqualTo(999.0));
            Assert.That(ws.GetFormula(0, 2), Is.EqualTo("=B500001+1"));
            Assert.That(Convert.ToDouble(ws.GetValue(0, 2)), Is.EqualTo(1000.0));
        }

        [Test]
        public void HideUnhideAndMergeFormula_SupportUndoRedo()
        {
            var spread = new Spread();
            var sheetView = (SheetView)spread.Sheets.ActiveSheet!;
            var ws = sheetView.WorkSheet;

            // 1. HideRows / UnhideRows Undo/Redo
            sheetView.SelectRows(2, 2);
            var hideRows = new HideRowsCommand(spread);
            hideRows.Execute(sheetView);
            Assert.That(ws.Rows.IsRowVisible(2), Is.False);
            Assert.That(ws.Rows.IsRowVisible(3), Is.False);

            spread.Undo();
            Assert.That(ws.Rows.IsRowVisible(2), Is.True);
            Assert.That(ws.Rows.IsRowVisible(3), Is.True);

            spread.Redo();
            Assert.That(ws.Rows.IsRowVisible(2), Is.False);
            Assert.That(ws.Rows.IsRowVisible(3), Is.False);

            var unhideRows = new UnhideRowsCommand(spread);
            unhideRows.Execute(sheetView);
            Assert.That(ws.Rows.IsRowVisible(2), Is.True);
            Assert.That(ws.Rows.IsRowVisible(3), Is.True);

            spread.Undo();
            Assert.That(ws.Rows.IsRowVisible(2), Is.False);
            Assert.That(ws.Rows.IsRowVisible(3), Is.False);

            // 2. HideColumns / UnhideColumns Undo/Redo
            sheetView.SelectColumns(1, 1);
            var hideCols = new HideColumnsCommand(spread);
            hideCols.Execute(sheetView);
            Assert.That(ws.Columns.IsColumnVisible(1), Is.False);

            spread.Undo();
            Assert.That(ws.Columns.IsColumnVisible(1), Is.True);

            // 3. MergeRange preserves covered formulas on Undo
            ws.SetValue(0, 0, 10);
            ws.SetFormula(0, 1, "=A1*3");
            Assert.That(Convert.ToDouble(ws.GetValue(0, 1)), Is.EqualTo(30.0));

            sheetView.MergeRange(new CellRange(0, 0, 1, 2));
            Assert.That(ws.GetFormula(0, 1), Is.Null);

            spread.Undo();
            Assert.That(ws.GetFormula(0, 1), Is.EqualTo("=A1*3"));
            Assert.That(Convert.ToDouble(ws.GetValue(0, 1)), Is.EqualTo(30.0));
        }
    }
}
