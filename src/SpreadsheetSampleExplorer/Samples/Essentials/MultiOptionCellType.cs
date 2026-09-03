using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Event arguments for the <see cref="MultiOptionCellType.SelectionChanged"/> event.
    /// </summary>
    public class MultiOptionChangedEventArgs : EventArgs
    {
        public ISheetView SheetView { get; }
        public int Row { get; }
        public int Column { get; }
        public object OldValue { get; }
        public object NewValue { get; }
        public int SelectedIndex { get; }

        public MultiOptionChangedEventArgs(ISheetView sheetView, int row, int column, object oldValue, object newValue, int selectedIndex)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            OldValue = oldValue;
            NewValue = newValue;
            SelectedIndex = selectedIndex;
        }
    }

    /// <summary>
    /// Custom cell type rendering a group of mutually exclusive, custom-styled radio buttons within a single cell.
    /// Supports independent sub-element hit-testing, hover highlighting, and click selection.
    /// </summary>
    public class MultiOptionCellType : BaseCellType
    {
        private static readonly Dictionary<string, double> _textWidthCache = new Dictionary<string, double>(StringComparer.Ordinal);
        private static readonly Typeface _labelTypeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        private RadioOptionElement[] _optionElements;
        private Pen _unselectedPen;
        private Pen _selectedPen;
        private Pen _hoverPen;

        #region Properties

        /// <summary>
        /// Gets or sets the array of option labels to display.
        /// </summary>
        public string[] Items { get; set; } = new[] { "Option 1", "Option 2" };

        /// <summary>
        /// Gets or sets the diameter of the outer radio circle in device-independent units.
        /// </summary>
        public double RadioSize { get; set; } = 14.0;

        /// <summary>
        /// Gets or sets the diameter of the inner filled dot when selected.
        /// </summary>
        public double DotSize { get; set; } = 6.0;

        /// <summary>
        /// Gets or sets the gap between the radio button circle and its text label.
        /// </summary>
        public double TextGap { get; set; } = 6.0;

        /// <summary>
        /// Gets or sets the horizontal gap between consecutive radio options.
        /// </summary>
        public double ItemSpacing { get; set; } = 16.0;

        /// <summary>
        /// Gets or sets the accent brush for selected radio buttons.
        /// </summary>
        public Brush SelectedBrush { get; set; } = SheetUtils.CreateFrozenBrush("#2563EB");

        /// <summary>
        /// Gets or sets the border brush for unselected radio buttons.
        /// </summary>
        public Brush UnselectedBorderBrush { get; set; } = SheetUtils.CreateFrozenBrush("#94A3B8");

        /// <summary>
        /// Gets or sets the border brush when hovering over a radio button.
        /// </summary>
        public Brush HoverBorderBrush { get; set; } = SheetUtils.CreateFrozenBrush("#3B82F6");

        /// <summary>
        /// Gets or sets the soft translucent halo brush drawn around a radio circle on hover.
        /// </summary>
        public Brush HoverHaloBrush { get; set; } = SheetUtils.CreateFrozenBrush(Color.FromArgb(35, 37, 99, 235));

        /// <summary>
        /// Gets or sets the subtle outline pen for the hover halo around a radio circle.
        /// </summary>
        public Pen HoverHaloPen { get; set; } = SheetUtils.CreateFrozenPen(SheetUtils.CreateFrozenBrush(Color.FromArgb(80, 59, 130, 246)), 1.0);

        /// <summary>
        /// Gets or sets the background fill of the radio circle.
        /// </summary>
        public Brush RadioBackgroundBrush { get; set; } = Brushes.White;

        /// <summary>
        /// Gets or sets the text label foreground brush.
        /// </summary>
        public Brush TextBrush { get; set; } = SheetUtils.CreateFrozenBrush("#1E293B");

        /// <summary>
        /// Gets or sets the font family for option labels.
        /// </summary>
        public DrawingFontFamily FontFamily { get; set; } = new DrawingFontFamily("Segoe UI");

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an option within this cell is clicked and selected.
        /// </summary>
        public event EventHandler<MultiOptionChangedEventArgs> SelectionChanged;

        #endregion

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            int count = Items?.Length ?? 0;
            if (count == 0) yield break;

            if (_optionElements == null || _optionElements.Length != count)
            {
                _optionElements = new RadioOptionElement[count];
                for (int i = 0; i < count; i++)
                {
                    _optionElements[i] = new RadioOptionElement(this, i);
                }
            }

            for (int i = 0; i < _optionElements.Length; i++)
            {
                yield return _optionElements[i];
            }
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (Items == null || Items.Length == 0) return;

            int selectedIndex = ResolveSelectedIndex(value);
            DrawRadioGroup(renderContext, cellRect, selectedIndex, style?.FontFamily ?? FontFamily);
        }

        internal void DrawRadioGroup(IRenderContext renderContext, Rect cellRect, int selectedIndex, DrawingFontFamily fontFamily = null)
        {
            int count = Items?.Length ?? 0;
            if (count == 0) return;

            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            double radioSize = RadioSize * zoom;
            double dotSize = DotSize * zoom;
            double textGap = TextGap * zoom;
            double itemSpacing = ItemSpacing * zoom;
            double fontSize = 11.0;
            double scaledFontSize = fontSize * zoom;

            EnsurePens();

            // Calculate total width of all radio options
            double[] itemWidths = new double[count];
            double totalWidth = 0;

            for (int i = 0; i < count; i++)
            {
                double textWidth = GetTextWidth(Items[i], scaledFontSize);
                itemWidths[i] = radioSize + textGap + textWidth;
                totalWidth += itemWidths[i];
                if (i < count - 1) totalWidth += itemSpacing;
            }

            if (totalWidth > cellRect.Width)
            {
                totalWidth = cellRect.Width - 4 * zoom;
            }

            // Center all options horizontally within cellRect
            double startX = cellRect.X + Math.Max(4 * zoom, (cellRect.Width - totalWidth) / 2.0);
            double currentX = startX;
            double centerY = cellRect.Y + cellRect.Height / 2.0;
            double textPadding = 5.0 * zoom;

            for (int i = 0; i < count; i++)
            {
                string label = Items[i];
                bool isSelected = (i == selectedIndex);

                // Radio circle position
                double circleCenterY = centerY;
                double circleCenterX = currentX + radioSize / 2.0;
                var circleCenter = new Point(circleCenterX, circleCenterY);
                double radius = radioSize / 2.0;

                // 1. Draw outer circle
                Pen circlePen = isSelected ? _selectedPen : _unselectedPen;
                renderContext.DrawEllipse(RadioBackgroundBrush, circlePen, circleCenter, radius, radius);

                // 2. Draw inner dot if selected
                if (isSelected)
                {
                    double dotRadius = dotSize / 2.0;
                    renderContext.DrawEllipse(SelectedBrush, null, circleCenter, dotRadius, dotRadius);
                }

                // 3. Draw text label
                double textLeft = circleCenterX + radius + textGap;
                double textW = itemWidths[i] - (radioSize + textGap);

                if (textW > 0 && textLeft + textW <= cellRect.Right)
                {
                    // Offset textBounds by -textPadding so that TextRenderer lands exactly at textLeft
                    var textBounds = new Rect(textLeft - textPadding, cellRect.Y, textW + (2.0 * textPadding), cellRect.Height);
                    renderContext.DrawText(
                        label,
                        textBounds,
                        fontFamily ?? FontFamily,
                        fontSize,
                        isSelected ? DrawingFontWeight.Bold : DrawingFontWeight.Normal,
                        DrawingFontStyle.Normal,
                        isSelected ? SelectedBrush : TextBrush,
                        CellHorizontalAlignment.Left,
                        CellVerticalAlignment.Center,
                        CellTextTrimming.None,
                        false);
                }

                currentX += itemWidths[i] + itemSpacing;
            }
        }

        internal Rect GetOptionBounds(Rect cellRect, double zoom, int optionIndex)
        {
            int count = Items?.Length ?? 0;
            if (count == 0 || optionIndex < 0 || optionIndex >= count) return Rect.Empty;

            double radioSize = RadioSize * zoom;
            double textGap = TextGap * zoom;
            double itemSpacing = ItemSpacing * zoom;
            double scaledFontSize = 11.0 * zoom;

            double[] itemWidths = new double[count];
            double totalWidth = 0;

            for (int i = 0; i < count; i++)
            {
                double textWidth = GetTextWidth(Items[i], scaledFontSize);
                itemWidths[i] = radioSize + textGap + textWidth;
                totalWidth += itemWidths[i];
                if (i < count - 1) totalWidth += itemSpacing;
            }

            double startX = cellRect.X + Math.Max(4 * zoom, (cellRect.Width - totalWidth) / 2.0);
            double currentX = startX;

            for (int i = 0; i < optionIndex; i++)
            {
                currentX += itemWidths[i] + itemSpacing;
            }

            return new Rect(currentX - 2 * zoom, cellRect.Y + 2 * zoom, itemWidths[optionIndex] + 4 * zoom, cellRect.Height - 4 * zoom);
        }

        internal int ResolveSelectedIndex(object value)
        {
            if (value == null || Items == null || Items.Length == 0) return -1;

            if (value is int intVal && intVal >= 0 && intVal < Items.Length)
                return intVal;

            string strVal = value.ToString().Trim();
            for (int i = 0; i < Items.Length; i++)
            {
                if (string.Equals(Items[i], strVal, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        internal void SetSelectedOption(ISheetView view, int row, int col, int newIndex)
        {
            var worksheet = view?.WorkSheet;
            if (worksheet == null || Items == null || newIndex < 0 || newIndex >= Items.Length) return;

            object oldValue = worksheet.GetValue(row, col);
            string newValue = Items[newIndex];

            worksheet.SetValue(row, col, newValue);
            SelectionChanged?.Invoke(this, new MultiOptionChangedEventArgs(view, row, col, oldValue, newValue, newIndex));
        }

        private void EnsurePens()
        {
            if (_unselectedPen == null)
            {
                _unselectedPen = SheetUtils.CreateFrozenPen(UnselectedBorderBrush, 1.2);
            }
            if (_selectedPen == null)
            {
                _selectedPen = SheetUtils.CreateFrozenPen(SelectedBrush, 1.8);
            }
            if (_hoverPen == null)
            {
                _hoverPen = SheetUtils.CreateFrozenPen(HoverBorderBrush, 1.8);
            }
        }

        private static double GetTextWidth(string text, double scaledFontSize)
        {
            string key = $"{text}_{scaledFontSize:F1}";
            if (!_textWidthCache.TryGetValue(key, out double width))
            {
#if NET472
                var ft = new FormattedText(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    _labelTypeface,
                    scaledFontSize,
                    Brushes.Black,
                    1.0);
#else
                var ft = new FormattedText(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    _labelTypeface,
                    scaledFontSize,
                    Brushes.Black,
                    SheetUtils.PixelPerDip);
#endif
                width = Math.Ceiling(ft.Width) + 2.0;
                _textWidthCache[key] = width;
            }
            return width;
        }

        public override bool SupportsEditing => false;
    }

    #region Elements

    /// <summary>
    /// Interactive sub-element representing a single radio option within a <see cref="MultiOptionCellType"/>.
    /// </summary>
    public class RadioOptionElement : CellElement
    {
        private readonly MultiOptionCellType _cellType;

        /// <summary>
        /// Gets the zero-based index of this radio option.
        /// </summary>
        public int OptionIndex { get; }

        public RadioOptionElement(MultiOptionCellType cellType, int optionIndex)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
            OptionIndex = optionIndex;
        }

        public override Cursor Cursor => Cursors.Hand;

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            return _cellType.GetOptionBounds(cellRect, zoom, OptionIndex);
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            if (state == CellElementState.Normal || _cellType == null || bounds.Width <= 0 || bounds.Height <= 0) return;

            double zoom = context.ZoomFactor > 0 ? context.ZoomFactor : 1.0;
            double radioSize = _cellType.RadioSize * zoom;
            double radius = radioSize / 2.0;

            // Option's radio circle center is offset by 2*zoom (the inset in GetOptionBounds) + radius
            double circleCenterX = bounds.Left + 2.0 * zoom + radius;
            double circleCenterY = bounds.Top + bounds.Height / 2.0;
            var center = new Point(circleCenterX, circleCenterY);

            // Draw a subtle halo around the radio circle on hover or press without touching text
            double haloRadius = radius + 3.0 * zoom;
            var haloBrush = _cellType.HoverHaloBrush;
            var haloPen = (state == CellElementState.Pressed) ? _cellType.HoverHaloPen : null;

            context.DrawEllipse(haloBrush, haloPen, center, haloRadius, haloRadius);
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            if (_cellType == null || view == null) return;

            var worksheet = view.WorkSheet;
            var sheetCol = worksheet?.Columns?.GetItem(col);
            var sheetRow = worksheet?.Rows?.GetItem(row);

            bool locked = (worksheet?.GetLocked(row, col) == true) ||
                (sheetRow != null && sheetRow.Locked) ||
                (sheetCol != null && sheetCol.Locked);

            if (locked) return;

            _cellType.SetSelectedOption(view, row, col, OptionIndex);
        }
    }

    #endregion
}
