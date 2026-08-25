using DevBrewLabs.Spreadsheet;
using DevBrewLabs.WPF.Spreadsheet.CellTypes;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal class CellInteractionManager : UIManager
    {
        private (int Row, int Column, CellElement Element)? _hoveredElement;
        private (int Row, int Column, CellElement Element)? _pressedElement;

        public CellInteractionManager(Spread spread) : base(spread)
        {
        }

        /// <summary>
        /// Retrieves all interactive elements associated with a cell, merging sheet-level features and cell-type elements.
        /// </summary>
        public IEnumerable<CellElement> GetCellElements(SheetView view, int row, int col)
        {
            if (view == null) yield break;

            var workSheet = (Worksheet)view.WorkSheet;
            if (workSheet == null) yield break;

            var columns = (Columns)workSheet.Columns;
            var sheetColumn = columns?.GetItem(col);

            // 1. Sheet Features (e.g. AutoFilter header button)
            if (view.Spread.AllowFiltering &&
                workSheet.AutoFilter != null &&
                workSheet.AutoFilter.IsFilterHeaderCell(row, col) &&
                (sheetColumn == null || sheetColumn.AllowFiltering))
            {
                yield return FilterButton.Instance;
            }

            // 2. CellType elements (e.g. Spinners, Dropdowns, DatePickers)
            var cellType = (BaseCellType)(workSheet.GetCellType(row, col) ?? sheetColumn?.CellType);
            if (cellType != null)
            {
                foreach (var element in cellType.GetElements(view, row, col))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Gets the current interaction state of an element on a specific cell.
        /// </summary>
        public CellElementState GetElementState(int row, int col, CellElement element)
        {
            if (_pressedElement.HasValue &&
                _pressedElement.Value.Row == row &&
                _pressedElement.Value.Column == col &&
                _pressedElement.Value.Element == element)
            {
                return CellElementState.Pressed;
            }

            if (_hoveredElement.HasValue &&
                _hoveredElement.Value.Row == row &&
                _hoveredElement.Value.Column == col &&
                _hoveredElement.Value.Element == element)
            {
                return CellElementState.Hover;
            }

            return CellElementState.Normal;
        }

        /// <summary>
        /// Hit-tests a point against all interactive sub-elements within visible cells.
        /// </summary>
        public (CellElement Element, int Row, int Column, Rect Bounds)? HitTest(SheetView view, Point hitPoint)
        {
            if (view == null) return null;

            var workSheet = (Worksheet)view.WorkSheet;
            var viewPort = (ViewPort)view.ViewPort;
            var rows = (Rows)workSheet.Rows;
            var columns = (Columns)workSheet.Columns;
            var viewRange = viewPort.ViewRange;
            double zoom = view.ZoomFactor > 0 ? view.ZoomFactor : 1.0;

            var point = new Point(
                hitPoint.X / zoom + viewPort.LeftColumnLocation,
                hitPoint.Y / zoom + viewPort.TopRowLocation);

            for (int row = viewRange.TopRow; row <= viewRange.BottomRow; row++)
            {
                var sheetRowObj = rows.GetItem(row);
                if (sheetRowObj != null && !sheetRowObj.Visible) continue;

                var rowLocation = viewPort.GetRowLocation(row);
                double rowHeight = workSheet.Rows.GetRowHeight(row);

                if (point.Y >= rowLocation && point.Y < rowLocation + rowHeight)
                {
                    for (int col = viewRange.LeftColumn; col <= viewRange.RightColumn; col++)
                    {
                        var colLocation = viewPort.GetColumnLocation(col);
                        double columnWidth = workSheet.Columns.GetColumnWidth(col);

                        if (point.X >= colLocation && point.X < colLocation + columnWidth)
                        {
                            var anchor = workSheet.GetSpanCellRange(row, col);
                            int targetRow = anchor != default ? anchor.TopRow : row;
                            int targetCol = anchor != default ? anchor.LeftColumn : col;

                            var cellRect = viewPort.GetCellRect(targetRow, targetCol);
                            var unscaled = new Rect(
                                cellRect.X - viewPort.LeftColumnLocation,
                                cellRect.Y - viewPort.TopRowLocation,
                                cellRect.Width,
                                cellRect.Height);
                            var scaledCellRect = new Rect(
                                unscaled.X * zoom,
                                unscaled.Y * zoom,
                                unscaled.Width * zoom,
                                unscaled.Height * zoom);

                            foreach (var element in GetCellElements(view, targetRow, targetCol))
                            {
                                var elementBounds = element.GetBounds(scaledCellRect, zoom);
                                if (elementBounds.Contains(hitPoint))
                                {
                                    return (element, targetRow, targetCol, elementBounds);
                                }
                            }
                            return null;
                        }
                    }
                    break;
                }
            }

            return null;
        }

        public bool OnMouseMove(SheetView view, Point hitPoint, out Cursor cursor)
        {
            var hit = HitTest(view, hitPoint);
            if (hit != null)
            {
                UpdateHover(view, hit.Value.Row, hit.Value.Column, hit.Value.Element);
                cursor = hit.Value.Element.Cursor;
                return true;
            }

            UpdateHover(view, -1, -1, null);
            cursor = null;
            return false;
        }

        public bool OnMouseLeftButtonDown(SheetView view, Point hitPoint)
        {
            var hit = HitTest(view, hitPoint);
            if (hit != null)
            {
                _pressedElement = (hit.Value.Row, hit.Value.Column, hit.Value.Element);
                hit.Value.Element.OnMouseDown(view, hit.Value.Row, hit.Value.Column);
                InvalidateInteractionLayer(view);
                return true;
            }

            return false;
        }

        public bool OnMouseLeftButtonUp(SheetView view, Point hitPoint)
        {
            if (_pressedElement.HasValue)
            {
                var pressed = _pressedElement.Value;
                _pressedElement = null;
                InvalidateInteractionLayer(view);

                var hit = HitTest(view, hitPoint);
                if (hit != null &&
                    hit.Value.Row == pressed.Row &&
                    hit.Value.Column == pressed.Column &&
                    hit.Value.Element == pressed.Element)
                {
                    pressed.Element.OnClick(view, pressed.Row, pressed.Column);
                    return true;
                }
            }

            return false;
        }

        public void OnMouseLeave(SheetView view)
        {
            if (_hoveredElement.HasValue || _pressedElement.HasValue)
            {
                _hoveredElement = null;
                _pressedElement = null;
                InvalidateInteractionLayer(view);
            }
        }

        public void ClearState(SheetView view)
        {
            if (_hoveredElement.HasValue || _pressedElement.HasValue)
            {
                _hoveredElement = null;
                _pressedElement = null;
                InvalidateInteractionLayer(view);
            }
        }

        private void UpdateHover(SheetView view, int row, int col, CellElement element)
        {
            bool wasHovered = _hoveredElement.HasValue;
            bool isNowHovered = element != null;

            if (wasHovered != isNowHovered ||
                (wasHovered && (_hoveredElement.Value.Row != row || _hoveredElement.Value.Column != col || _hoveredElement.Value.Element != element)))
            {
                if (element != null)
                {
                    _hoveredElement = (row, col, element);
                }
                else
                {
                    _hoveredElement = null;
                }

                InvalidateInteractionLayer(view);
            }
        }

        private void InvalidateInteractionLayer(SheetView view)
        {
            view?.CellsSurface?.GetInteractionLayer()?.InvalidateVisual();
        }
    }
}
