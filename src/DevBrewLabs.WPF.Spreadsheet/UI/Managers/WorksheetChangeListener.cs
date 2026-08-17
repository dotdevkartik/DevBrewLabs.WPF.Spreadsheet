using DevBrewLabs.Spreadsheet;
using System;
using System.Windows.Threading;

namespace DevBrewLabs.WPF.Spreadsheet.UI.Managers
{
    internal sealed class WorksheetChangeListener : IChangeListener
    {
        private Spread _spread;
        private bool _suspendUpdates;

        public WorksheetChangeListener(Spread spread)
        {
            _spread = spread;
        }

        public bool SuspendUpdates
        {
            get
            {
                return _suspendUpdates;
            }
            set
            {
                _suspendUpdates = value;

                if (CanInvalidate())
                {
                    _spread.Invalidate();
                }
            }
        }

        public void CellChanged(CellChangedEventArgs args)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var sheetView = (SheetView)_spread.SheetViews.GetSheetView(args.WorkSheet);

                if (!sheetView.ViewPort.ViewRange.ContainsCell(args.Row, args.Column))
                    return;

                switch (args.ChangeType)
                {
                    case CellChangeType.Value:
                    case CellChangeType.Formula:
                        if (sheetView.AutoSizeRows)
                            sheetView.AutoSizeRow(args.Row);
                        if (sheetView.AutoSizeColumns)
                            sheetView.AutoSizeColumn(args.Column);
                        break;
                }

                if (!CanInvalidate())
                {
                    return;
                }

                _spread.Invalidate();
            }));
        }

        public void RangeChanged(RangeChangedEventArgs args)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var sheetView = (SheetView)_spread.SheetViews.GetSheetView(args.WorkSheet);

                if (!sheetView.ViewPort.ViewRange.Intersects(args.Range))
                {
                    return;
                }

                if (!CanInvalidate())
                {
                    return;
                }

                _spread.Invalidate(true, false, true, false);
            }));
        }

        public void ColumnChanged(ColumnChangedEventArgs args)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var sheetView = (SheetView)_spread.SheetViews.GetSheetView(args.WorkSheet);

                switch (args.Region)
                {
                    case SheetRegion.RowHeader:
                        switch (args.ChangeType)
                        {
                            case ColumnChangeType.Width:
                                sheetView.ViewPort.UpdateHeaderColumnLocation(args.Index + 1, (int)args.NewValue - (int)args.OldValue);
                                break;
                        }
                        break;

                    case SheetRegion.Cells:
                        switch (args.ChangeType)
                        {
                            case ColumnChangeType.Width:
                                sheetView.ViewPort.UpdateColumnLocation(args.Index + 1, (int)args.NewValue - (int)args.OldValue);
                                break;
                        }
                        break;

                }

                sheetView.ViewPort.CalculateVisibleRange();

                if (!sheetView.ViewPort.ViewRange.ContainsColumn(args.Index))
                {
                    return;
                }

                if (!CanInvalidate())
                {
                    return;
                }

                _spread.SheetTabControl.UpdateScrollbars();
                _spread.Invalidate(false, true, true, false);
            }));
        }

        public void RowChanged(RowChangedEventArgs args)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var sheetView = (SheetView)_spread.SheetViews.GetSheetView(args.WorkSheet);

                switch (args.Region)
                {
                    case SheetRegion.ColumnHeader:
                        switch (args.ChangeType)
                        {
                            case RowChangeType.Height:
                                sheetView.ViewPort.UpdateHeaderRowLocation(args.Index + 1, (int)args.NewValue - (int)args.OldValue);
                                break;
                        }
                        break;

                    case SheetRegion.Cells:
                        switch (args.ChangeType)
                        {
                            case RowChangeType.Height:
                                sheetView.ViewPort.UpdateRowLocation(args.Index + 1, (int)args.NewValue - (int)args.OldValue);
                                break;
                        }
                        break;

                }

                sheetView.ViewPort.CalculateVisibleRange();
                if (!sheetView.ViewPort.ViewRange.ContainsRow(args.Index))
                {
                    return;
                }

                if (!CanInvalidate())
                {
                    return;
                }

                _spread.SheetTabControl.UpdateScrollbars();
                _spread.Invalidate(true, false, true, false);
            }));
        }

        private bool CanInvalidate()
        {
            return _spread.IsLoaded && !_suspendUpdates;
        }
    }
}
