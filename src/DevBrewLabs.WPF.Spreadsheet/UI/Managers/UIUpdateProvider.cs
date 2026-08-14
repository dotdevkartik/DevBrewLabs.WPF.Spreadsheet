using DevBrewLabs.Spreadsheet;
using System;
using System.Windows.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal sealed class UIUpdateProvider : IUpdateProvider
    {
        private Spread _spread;
        private bool _suspendUpdates;

        public UIUpdateProvider(Spread spread)
        {
            _spread = spread;
        }

        bool IUpdateProvider.SuspendUpdates
        {
            get
            {
                return _suspendUpdates;
            }
            set
            {
                _suspendUpdates = value;

                if (!_suspendUpdates && _spread.IsLoaded)
                {
                    _spread.Invalidate();
                }
            }
        }

        void IUpdateProvider.CellChanged(WorkSheet worksheet, int row, int column, object oldValue, object newValue, SheetRegion region, CellChangeType changeType)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                if (!_spread.IsLoaded)
                    return;

                var sheetView = _spread.SheetViews.GetSheetView(worksheet);

                if (!sheetView.ViewPort.ViewRange.ContainsCell(row, column))
                    return;

                switch (changeType)
                {
                    case CellChangeType.Value:
                    case CellChangeType.Formula:
                        if (sheetView.AutoSizeRows)
                            sheetView.AutoSizeRow(row);
                        if (sheetView.AutoSizeColumns)
                            sheetView.AutoSizeColumn(column);
                        break;
                }

                _spread.Invalidate();
            }));
        }

        void IUpdateProvider.ColumnsChanged(WorkSheet worksheet, int index, int count, SheetRegion region, ColumnChangeType changeType)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                if (!_spread.IsLoaded)
                    return;

                var sheetView = _spread.SheetViews.GetSheetView(worksheet);
                sheetView.ViewPort.As<ViewPort>().CalculateVisibleRange();

                if (!sheetView.ViewPort.ViewRange.ContainsColumn(index))
                {
                    return;
                }

                _spread.SheetTabControl.UpdateScrollbars();
                _spread.Invalidate(false, true, true, false);
            }));
        }

        void IUpdateProvider.RangeChanged(WorkSheet worksheet, CellRange range, SheetRegion region, RangeChangeType changeType)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                if (!_spread.IsLoaded)
                    return;

                var sheetView = _spread.SheetViews.GetSheetView(worksheet);

                if (!sheetView.ViewPort.ViewRange.Intersects(range))
                {
                    return;
                }

                _spread.Invalidate(true, false, true, false);
            }));
        }

        void IUpdateProvider.RowsChanged(WorkSheet worksheet, int index, int count, SheetRegion region, RowChangeType changeType)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                if (!_spread.IsLoaded)
                    return;

                var sheetView = _spread.SheetViews.GetSheetView(worksheet);
                sheetView.ViewPort.As<ViewPort>().CalculateVisibleRange();
                if (!sheetView.ViewPort.ViewRange.ContainsRow(index))
                {
                    return;
                }

                _spread.SheetTabControl.UpdateScrollbars();
                _spread.Invalidate(true, false, true, false);
            }));
        }
    }
}
