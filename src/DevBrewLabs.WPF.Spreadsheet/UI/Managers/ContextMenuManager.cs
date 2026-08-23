using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Sorting;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DevBrewLabs.WPF.Spreadsheet.Commands;
using DevBrewLabs.WPF.Spreadsheet.UI.Menus;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    /// <summary>
    /// Manages context menus for Spread, including Cells, RowHeaders, ColumnHeaders, and SheetTabs.
    /// </summary>
    internal class ContextMenuManager : UIManager
    {
        

        public ContextMenuManager(Spread spread) : base(spread)
        {
        }

        #region Public Show Methods

        /// <summary>
        /// Shows the context menu for a spreadsheet region (Cells, RowHeaders, ColumnHeaders, CornerHeader).
        /// </summary>
        public void ShowContextMenu(SheetView sheetView, SpreadHitTestResult hitTest, FrameworkElement targetElement, Point? screenPosition = null)
        {
            if (Spread == null || !Spread.AllowContextMenu || sheetView == null)
                return;

            if (Spread.EditingManager != null && Spread.EditingManager.IsEditing)
            {
                if (!Spread.EditingManager.EndEdit(true))
                    return;
            }

            SpreadContextMenuRegion region = ResolveRegion(hitTest);

            // Adjust selection if right clicked outside active selection
            AdjustSelectionForRegion(sheetView, region, hitTest);

            // Resolve context menu (custom or default)
            ContextMenu menu = ResolveContextMenu(region, sheetView, hitTest, -1);

            if (menu == null)
                return;

            // Raise ContextMenuOpening event
            var args = new SpreadContextMenuOpeningEventArgs(
                sheetView,
                region,
                hitTest,
                sheetView.Selection,
                menu,
                -1);

            Spread.RaiseContextMenuOpening(args);

            if (args.Cancel || args.ContextMenu == null)
                return;

            // Update item states before opening
            UpdateMenuItemStates(args.ContextMenu, sheetView, region, -1);

            // Open context menu
            OpenContextMenu(args.ContextMenu, targetElement, screenPosition);
        }

        /// <summary>
        /// Shows the context menu for a sheet tab item.
        /// </summary>
        public void ShowSheetTabContextMenu(SheetView sheetView, int sheetIndex, FrameworkElement targetElement, Point? screenPosition = null)
        {
            if (Spread == null || !Spread.AllowContextMenu)
                return;

            if (Spread.EditingManager != null && Spread.EditingManager.IsEditing)
            {
                if (!Spread.EditingManager.EndEdit(true))
                    return;
            }

            // Set active sheet to the right clicked sheet tab
            if (sheetIndex >= 0 && sheetIndex < Spread.WorkBook.WorkSheets.Count)
            {
                var targetWorksheet = Spread.WorkBook.WorkSheets[sheetIndex];
                if (Spread.WorkBook.WorkSheets.ActiveSheet != targetWorksheet)
                {
                    Spread.WorkBook.WorkSheets.ActiveSheet = targetWorksheet;
                }
            }

            ContextMenu menu = Spread.SheetTabContextMenu ?? CreateSheetTabContextMenu(sheetView, sheetIndex);

            var args = new SpreadContextMenuOpeningEventArgs(
                sheetView,
                SpreadContextMenuRegion.SheetTab,
                null,
                sheetView?.Selection ?? default,
                menu,
                sheetIndex);

            Spread.RaiseContextMenuOpening(args);

            if (args.Cancel || args.ContextMenu == null)
                return;

            UpdateMenuItemStates(args.ContextMenu, sheetView, SpreadContextMenuRegion.SheetTab, sheetIndex);
            OpenContextMenu(args.ContextMenu, targetElement, screenPosition);
        }

        #endregion

        #region Context Menu Builders

        public ContextMenu CreateCellContextMenu(SheetView sheetView)
        {
            return new CellsContextMenu(Spread, sheetView);
        }

        public ContextMenu CreateRowHeaderContextMenu(SheetView sheetView)
        {
            return new RowHeaderContextMenu(Spread, sheetView);
        }

        public ContextMenu CreateColumnHeaderContextMenu(SheetView sheetView)
        {
            return new ColumnHeaderContextMenu(Spread, sheetView);
        }

        public ContextMenu CreateSheetTabContextMenu(SheetView sheetView, int sheetIndex)
        {
            return new SheetTabContextMenu(Spread, sheetIndex);
        }

        #endregion

        

        #region Internal Helpers

        private SpreadContextMenuRegion ResolveRegion(SpreadHitTestResult hitTest)
        {
            if (hitTest == null)
                return SpreadContextMenuRegion.Cells;

            switch (hitTest.Element)
            {
                case VisualElement.RowHeader:
                case VisualElement.RowHeaderResizeBar:
                    return SpreadContextMenuRegion.RowHeader;

                case VisualElement.ColumnHeader:
                case VisualElement.ColumnHeaderResizeBar:
                    return SpreadContextMenuRegion.ColumnHeader;

                case VisualElement.TopLeft:
                    return SpreadContextMenuRegion.CornerHeader;

                default:
                    return SpreadContextMenuRegion.Cells;
            }
        }

        private void AdjustSelectionForRegion(SheetView sheetView, SpreadContextMenuRegion region, SpreadHitTestResult hitTest)
        {
            if (hitTest == null || sheetView == null)
                return;

            switch (region)
            {
                case SpreadContextMenuRegion.Cells:
                    if (hitTest.Row >= 0 && hitTest.Column >= 0)
                    {
                        if (!sheetView.Selection.ContainsCell(hitTest.Row, hitTest.Column))
                        {
                            sheetView.SelectCell(hitTest.Row, hitTest.Column);
                        }
                    }
                    break;

                case SpreadContextMenuRegion.RowHeader:
                    if (hitTest.Row >= 0)
                    {
                        if (!sheetView.Selection.ContainsRow(hitTest.Row) || sheetView.Selection.ColumnCount < sheetView.WorkSheet.ColumnCount)
                        {
                            sheetView.SelectRow(hitTest.Row);
                        }
                    }
                    break;

                case SpreadContextMenuRegion.ColumnHeader:
                    if (hitTest.Column >= 0)
                    {
                        if (!sheetView.Selection.ContainsColumn(hitTest.Column) || sheetView.Selection.RowCount < sheetView.WorkSheet.RowCount)
                        {
                            sheetView.SelectColumn(hitTest.Column);
                        }
                    }
                    break;
            }
        }

        private ContextMenu ResolveContextMenu(SpreadContextMenuRegion region, SheetView sheetView, SpreadHitTestResult hitTest, int sheetIndex)
        {
            switch (region)
            {
                case SpreadContextMenuRegion.Cells:
                case SpreadContextMenuRegion.CornerHeader:
                    return Spread.CellContextMenu ?? CreateCellContextMenu(sheetView);

                case SpreadContextMenuRegion.RowHeader:
                    return Spread.RowHeaderContextMenu ?? CreateRowHeaderContextMenu(sheetView);

                case SpreadContextMenuRegion.ColumnHeader:
                    return Spread.ColumnHeaderContextMenu ?? CreateColumnHeaderContextMenu(sheetView);

                case SpreadContextMenuRegion.SheetTab:
                    return Spread.SheetTabContextMenu ?? CreateSheetTabContextMenu(sheetView, sheetIndex);

                default:
                    return CreateCellContextMenu(sheetView);
            }
        }

        private void UpdateMenuItemStates(ContextMenu menu, SheetView sheetView, SpreadContextMenuRegion region, int sheetIndex) { }
        

        private void OpenContextMenu(ContextMenu menu, FrameworkElement targetElement, Point? screenPosition)
        {
            menu.PlacementTarget = targetElement;
            if (screenPosition.HasValue)
            {
                menu.Placement = PlacementMode.AbsolutePoint;
                menu.HorizontalOffset = screenPosition.Value.X;
                menu.VerticalOffset = screenPosition.Value.Y;
            }
            else
            {
                menu.Placement = PlacementMode.MousePoint;
            }
            menu.IsOpen = true;
        }

        #endregion
    }
}
