using System;
using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.Core
{
    internal class SpanManager
    {
        private readonly Dictionary<(int Row, int Col), CellRange> _anchors = new Dictionary<(int Row, int Col), CellRange>();
        private readonly Dictionary<(int Row, int Col), (int Row, int Col)> _coveredCells = new Dictionary<(int Row, int Col), (int Row, int Col)>();

        public bool HasSpans => _anchors.Count > 0;

        public void AddSpan(int row, int col, int rowCount, int colCount)
        {
            if (rowCount <= 1 && colCount <= 1)
                return;

            var newSpan = new CellRange(row, col, rowCount, colCount);

            // Check for overlaps
            for (int r = row; r < row + rowCount; r++)
            {
                for (int c = col; c < col + colCount; c++)
                {
                    if (_coveredCells.ContainsKey((r, c)) || _anchors.ContainsKey((r, c)))
                    {
                        throw new InvalidOperationException($"Cannot add span at ({row}, {col}) because it overlaps with an existing span.");
                    }
                }
            }

            _anchors[(row, col)] = newSpan;

            for (int r = row; r < row + rowCount; r++)
            {
                for (int c = col; c < col + colCount; c++)
                {
                    if (r == row && c == col) continue;
                    _coveredCells[(r, c)] = (row, col);
                }
            }
        }

        public void RemoveSpan(int row, int col)
        {
            if (_anchors.TryGetValue((row, col), out var span))
            {
                for (int r = span.TopRow; r <= span.BottomRow; r++)
                {
                    for (int c = span.LeftColumn; c <= span.RightColumn; c++)
                    {
                        if (r == row && c == col) continue;
                        _coveredCells.Remove((r, c));
                    }
                }
                _anchors.Remove((row, col));
            }
        }

        public bool IsCovered(int row, int col)
        {
            return _coveredCells.ContainsKey((row, col));
        }

        public (int Row, int Col)? GetAnchor(int row, int col)
        {
            if (_coveredCells.TryGetValue((row, col), out var anchor))
            {
                return anchor;
            }
            if (_anchors.ContainsKey((row, col)))
            {
                return (row, col);
            }
            return null;
        }

        public CellRange GetSpanRange(int row, int col)
        {
            var anchor = GetAnchor(row, col);
            if (anchor.HasValue && _anchors.TryGetValue(anchor.Value, out var range))
            {
                return range;
            }
            return default;
        }

        public IReadOnlyCollection<CellRange> GetAllSpans()
        {
            return _anchors.Values;
        }

        public IEnumerable<CellRange> GetSpansInRange(CellRange viewRange)
        {
            foreach (var span in _anchors.Values)
            {
                if (span.IntersectsWith(viewRange))
                {
                    yield return span;
                }
            }
        }

        public CellRange ExpandRange(CellRange range)
        {
            if (_anchors.Count == 0) return range;
            
            bool expanded = true;
            while (expanded)
            {
                expanded = false;
                foreach (var span in _anchors.Values)
                {
                    if (span.IntersectsWith(range) && !range.ContainsRange(span))
                    {
                        range = CellRange.Union(range, span);
                        expanded = true;
                    }
                }
            }
            return range;
        }
    }
}
