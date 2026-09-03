using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Enums;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Cell type for dropdown selection supporting object lists, custom display/value mappings, 
    /// editable/non-editable modes, and in-place dropdown editor popup.
    /// </summary>
    public class ComboBoxCellType : BaseCellType
    {
        private ComboBoxDropDownButton _dropDownButton;

        public IEnumerable ItemsSource { get; set; }
        public string DisplayMemberPath { get; set; }
        public string SelectedValuePath { get; set; }
        public string SearchMemberPath { get; set; }
        public bool ShowSuggestions { get; set; } = true;
        public bool IsEditable { get; set; } = false;
        public bool ShowDropDownButton { get; set; } = true;
        public virtual double DropDownButtonWidth => 18.0;
        public double MaxDropDownHeight { get; set; } = 220;
        public double? MaxDropDownWidth { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        public Brush ArrowBrush { get; set; }
        public Brush HoverArrowBrush { get; set; }
        public Brush ButtonHoverBackground { get; set; }

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (ShowDropDownButton)
            {
                yield return _dropDownButton ?? (_dropDownButton = new ComboBoxDropDownButton(this));
            }
        }

        public override Rect GetContentRect(ISheetView view, int row, int col, Rect cellRect, double zoom)
        {
            var rect = base.GetContentRect(view, row, col, cellRect, zoom);
            if (ShowDropDownButton)
            {
                double width = DropDownButtonWidth * zoom;
                return new Rect(rect.X, rect.Y, Math.Max(0, rect.Width - width), rect.Height);
            }

            return rect;
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            if (value == null)
                return;

            var contentRect = GetContentRect(renderContext.SheetView, -1, -1, cellRect, renderContext.ZoomFactor);

            var align = style.HorizontalAlignment;
            if (align == CellHorizontalAlignment.Auto)
                align = CellHorizontalAlignment.Left;

            string textToDraw = ResolveDisplayText(value);
            if (formatter != null)
            {
                textToDraw = formatter.Format(textToDraw);
            }

            renderContext.DrawText(
                textToDraw,
                contentRect,
                style.FontFamily,
                style.FontSize,
                style.FontWeight,
                style.FontStyle,
                style.ForeColor,
                align,
                style.VerticalAlignment,
                style.TextTrimming,
                style.AllowMultiLineText);
        }

        public string ResolveDisplayText(object value)
        {
            if (value == null) return string.Empty;

            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    if (item == null) continue;

                    if (!string.IsNullOrEmpty(SelectedValuePath))
                    {
                        var prop = item.GetType().GetProperty(SelectedValuePath, BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null)
                        {
                            var propVal = prop.GetValue(item, null);
                            if (Equals(propVal, value) || (propVal != null && propVal.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase)))
                            {
                                return GetItemDisplayString(item);
                            }
                        }
                    }
                    else if (Equals(item, value) || item.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return GetItemDisplayString(item);
                    }
                }
            }

            return value.ToString();
        }

        private string GetItemDisplayString(object item)
        {
            if (item == null) return string.Empty;

            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                var prop = item.GetType().GetProperty(DisplayMemberPath, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    var val = prop.GetValue(item, null);
                    return val?.ToString() ?? string.Empty;
                }
            }

            return item.ToString();
        }

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new ComboBoxCellEditor
            {
                ItemsSource = ItemsSource,
                DisplayMemberPath = DisplayMemberPath,
                SelectedValuePath = SelectedValuePath,
                SearchMemberPath = SearchMemberPath,
                ShowSuggestions = ShowSuggestions,
                IsEditable = IsEditable,
                MaxDropDownHeight = MaxDropDownHeight,
                MaxDropDownWidth = MaxDropDownWidth,
                ItemTemplate = ItemTemplate
            };
        }
    }

    #region Elements

    /// <summary>
    /// Interactive dropdown button displayed on ComboBox cells.
    /// </summary>
    public class ComboBoxDropDownButton : CellElement
    {
        private static readonly Geometry _chevronGeometry = CreateChevronGeometry();
        private readonly ComboBoxCellType _cellType;

        public ComboBoxDropDownButton(ComboBoxCellType cellType)
        {
            _cellType = cellType ?? throw new ArgumentNullException(nameof(cellType));
        }

        private static Geometry CreateChevronGeometry()
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(1.5, 3.5), true, true);
                ctx.LineTo(new Point(8.5, 3.5), true, true);
                ctx.LineTo(new Point(5.0, 7.5), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        public override Rect GetBounds(Rect cellRect, double zoom)
        {
            double buttonWidth = (_cellType?.DropDownButtonWidth ?? 18) * zoom;
            double buttonHeight = Math.Min(cellRect.Height, 20 * zoom);
            double y = cellRect.Y + (cellRect.Height - buttonHeight) / 2;
            return new Rect(cellRect.Right - buttonWidth, y, buttonWidth, buttonHeight);
        }

        public override void Draw(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            var spread = context.SheetView?.Spread;
            if (spread == null) return;

            bool isHovered = (state == CellElementState.Hover || state == CellElementState.Pressed);
            var hoverBg = _cellType?.ButtonHoverBackground ?? spread.HoverFilterButtonBackground ?? SheetUtils.ComboBoxDropDownHoverBackground;

            if (isHovered && hoverBg != null)
            {
                var hoverBgRect = new Rect(bounds.X + 1, bounds.Y + 1, Math.Max(0, bounds.Width - 2), Math.Max(0, bounds.Height - 2));
                context.DrawRoundedRectangle(hoverBg, null, hoverBgRect, 2, 2);
            }

            double iconWidth = 8 * context.ZoomFactor;
            double iconHeight = 8 * context.ZoomFactor;
            double x = bounds.X + (bounds.Width - iconWidth) / 2;
            double y = bounds.Y + (bounds.Height - iconHeight) / 2;

            Brush arrowBrush;
            if (isHovered)
            {
                arrowBrush = _cellType?.HoverArrowBrush ?? SheetUtils.ComboBoxDropDownHoverArrowBrush;
            }
            else
            {
                arrowBrush = _cellType?.ArrowBrush ?? SheetUtils.ComboBoxDropDownArrowBrush;
            }

            if (arrowBrush != null)
            {
                double scale = iconWidth / 10.0;
                context.PushTransform(new MatrixTransform(scale, 0, 0, scale, x, y));
                context.DrawGeometry(arrowBrush, null, _chevronGeometry);
                context.Pop();
            }
        }

        public override void OnClick(ISheetView view, int row, int col)
        {
            var sheetView = view as SheetView;
            if (sheetView == null) return;

            var editingManager = sheetView.Spread?.EditingManager;
            if (editingManager == null) return;

            if (editingManager.IsEditing)
            {
                if (!editingManager.EndEdit(true))
                    return;
            }

            editingManager.BeginEdit(sheetView, row, col, EditTrigger.DropdownClick);
        }
    }

    #endregion
}
