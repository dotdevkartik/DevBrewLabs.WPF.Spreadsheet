using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SpreadsheetSampleExplorer.Samples
{
    /// <summary>
    /// Event arguments for the <see cref="RatingCellType.RatingChanged"/> event.
    /// </summary>
    public class RatingChangedEventArgs : EventArgs
    {
        public ISheetView SheetView { get; }
        public int Row { get; }
        public int Column { get; }
        public int OldRating { get; }
        public int NewRating { get; }

        public RatingChangedEventArgs(ISheetView sheetView, int row, int column, int oldRating, int newRating)
        {
            SheetView = sheetView;
            Row = row;
            Column = column;
            OldRating = oldRating;
            NewRating = newRating;
        }
    }

    /// <summary>
    /// Custom interactive star rating cell type demonstrating custom cell rendering, hit-testing, and element click interaction.
    /// </summary>
    public class RatingCellType : BaseCellType
    {
        private static readonly Geometry _starGeometry = CreateStarGeometry();
        private StarElement[] _starElements;
        private Brush _emptyStarBorderBrush;
        private Pen _emptyStarPen;

        /// <summary>
        /// Gets or sets the maximum number of stars (default 5).
        /// </summary>
        public int MaxRating { get; set; } = 5;

        /// <summary>
        /// Gets or sets the diameter of each star in device-independent units.
        /// </summary>
        public double StarSize { get; set; } = 16.0;

        /// <summary>
        /// Gets or sets the horizontal gap between consecutive stars.
        /// </summary>
        public double StarSpacing { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the brush used to fill active/rated stars.
        /// </summary>
        public Brush FilledStarBrush { get; set; } = SheetUtils.CreateFrozenBrush("#F59E0B"); // Vibrant Amber-500

        /// <summary>
        /// Gets or sets the brush used to fill active stars when hovered.
        /// </summary>
        public Brush HoverStarBrush { get; set; } = SheetUtils.CreateFrozenBrush("#FBBF24"); // Amber-400

        /// <summary>
        /// Gets or sets the fill brush for empty/unrated stars.
        /// </summary>
        public Brush EmptyStarBrush { get; set; } = SheetUtils.CreateFrozenBrush("#F1F5F9"); // Slate-100

        /// <summary>
        /// Gets or sets the border brush for empty/unrated stars.
        /// </summary>
        public Brush EmptyStarBorderBrush
        {
            get => _emptyStarBorderBrush ?? (_emptyStarBorderBrush = SheetUtils.CreateFrozenBrush("#CBD5E1")); // Slate-300
            set
            {
                _emptyStarBorderBrush = value;
                _emptyStarPen = SheetUtils.CreateFrozenPen(value, 0.8);
            }
        }

        internal Pen EmptyStarPen => _emptyStarPen ?? (_emptyStarPen = SheetUtils.CreateFrozenPen(EmptyStarBorderBrush, 0.8));

        /// <summary>
        /// Occurs when a rating is changed via user interaction.
        /// </summary>
        public event EventHandler<RatingChangedEventArgs> RatingChanged;

        private static Geometry CreateStarGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                double cx = 5.0;
                double cy = 5.0;
                double outerR = 4.8;
                double innerR = 2.0;
                int points = 5;

                double angleStep = Math.PI / points;
                double currentAngle = -Math.PI / 2.0;

                Point first = new Point(cx + outerR * Math.Cos(currentAngle), cy + outerR * Math.Sin(currentAngle));
                ctx.BeginFigure(first, true, true);

                for (int i = 1; i < points * 2; i++)
                {
                    currentAngle += angleStep;
                    double r = (i % 2 == 0) ? outerR : innerR;
                    Point pt = new Point(cx + r * Math.Cos(currentAngle), cy + r * Math.Sin(currentAngle));
                    ctx.LineTo(pt, true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (_starElements == null || _starElements.Length != MaxRating)
            {
                _starElements = new StarElement[MaxRating];
                for (int i = 0; i < MaxRating; i++)
                {
                    _starElements[i] = new StarElement(this, i + 1);
                }
            }

            for (int i = 0; i < _starElements.Length; i++)
            {
                yield return _starElements[i];
            }
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            int rating = ParseRating(value);
            DrawStars(renderContext, cellRect, rating, false, 0);
        }

        internal void DrawStars(IRenderContext renderContext, Rect cellRect, int rating, bool isHovered, int hoverRating)
        {
            double zoom = renderContext.ZoomFactor > 0 ? renderContext.ZoomFactor : 1.0;
            double size = StarSize * zoom;
            double spacing = StarSpacing * zoom;
            double totalWidth = MaxRating * size + (MaxRating - 1) * spacing;

            if (totalWidth > cellRect.Width || size > cellRect.Height) return;

            double startX = cellRect.X + (cellRect.Width - totalWidth) / 2.0;
            double startY = cellRect.Y + (cellRect.Height - size) / 2.0;

            int displayRating = (isHovered && hoverRating > 0) ? hoverRating : rating;

            for (int i = 0; i < MaxRating; i++)
            {
                double x = startX + i * (size + spacing);
                double y = startY;

                bool isFilled = (i < displayRating);
                Brush fill = isFilled ? (isHovered ? HoverStarBrush : FilledStarBrush) : EmptyStarBrush;
                Pen pen = isFilled ? null : EmptyStarPen;

                double scale = size / 10.0;
                renderContext.PushTransform(new MatrixTransform(scale, 0, 0, scale, x, y));
                renderContext.DrawGeometry(fill, pen, _starGeometry);
                renderContext.Pop();
            }
        }

        internal int ParseRating(object value)
        {
            if (value == null) return 0;
            if (value is int intVal) return Math.Max(0, Math.Min(MaxRating, intVal));
            if (value is double dblVal) return Math.Max(0, Math.Min(MaxRating, (int)Math.Round(dblVal)));
            if (int.TryParse(value.ToString(), out int parsed)) return Math.Max(0, Math.Min(MaxRating, parsed));
            return 0;
        }

        internal void SetRating(ISheetView view, int row, int col, int newRating)
        {
            var worksheet = view?.WorkSheet;
            if (worksheet == null) return;

            int oldRating = ParseRating(worksheet.GetValue(row, col));
            worksheet.SetValue(row, col, newRating);
            RatingChanged?.Invoke(this, new RatingChangedEventArgs(view, row, col, oldRating, newRating));
        }

        public override bool SupportsEditing => false;
    }

    #region Elements

    /// <summary>
    /// Interactive element representing a single star within a rating cell.
    /// </summary>
    public class StarElement : CellElement
    {
        private readonly RatingCellType _cellType;

        /// <summary>
        /// Gets the 1-based index of this star (1 to MaxRating).
        /// </summary>
        public int StarIndex { get; }

        public StarElement(RatingCellType cellType, int starIndex)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
            StarIndex = starIndex;
        }

        public override Cursor Cursor => Cursors.Hand;

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double size = (_cellType?.StarSize ?? 16.0) * zoom;
            double spacing = (_cellType?.StarSpacing ?? 4.0) * zoom;
            int maxRating = _cellType?.MaxRating ?? 5;
            double totalWidth = maxRating * size + (maxRating - 1) * spacing;

            double startX = cellRect.X + (cellRect.Width - totalWidth) / 2.0;
            double startY = cellRect.Y + (cellRect.Height - size) / 2.0;

            double starX = startX + (StarIndex - 1) * (size + spacing);
            return new Rect(starX - 1.5 * zoom, startY - 1.5 * zoom, size + 3 * zoom, size + 3 * zoom);
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            if (state == CellElementState.Normal || _cellType == null) return;

            var ws = context.SheetView?.WorkSheet;
            var value = ws?.GetValue(row, col);
            int currentRating = _cellType.ParseRating(value);

            // Preview rating up to this hovered star index
            var cellRect = context.GetCellRect(row, col);
            _cellType.DrawStars(context, cellRect, currentRating, true, StarIndex);
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

            int currentRating = _cellType.ParseRating(worksheet.GetValue(row, col));
            // Clicking Star 1 when already 1 toggles back to 0; otherwise sets rating directly to StarIndex
            int newRating = (currentRating == StarIndex && StarIndex == 1) ? 0 : StarIndex;
            _cellType.SetRating(view, row, col, newRating);
        }
    }

    #endregion
}
