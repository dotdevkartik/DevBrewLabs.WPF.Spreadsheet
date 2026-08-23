using DevBrewLabs.Spreadsheet;
using System;
using System.Windows.Threading;
using DevBrewLabs.Spreadsheet.Filtering;

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


        public void OnWorksheetChanged(WorksheetChangedEventArgs args)
        {
            if (!CanInvalidate())
            {
                return;
            }

            var sheetView = (SheetView)_spread.Sheets.GetSheetView(args.Worksheet);
            sheetView.ViewPort.CalculateVisibleRange();
            _spread.SheetTabControl.UpdateScrollbars();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _spread.Invalidate()));
        }

        public void CellChanged(CellChangedEventArgs args)
        {
            var sheetView = (SheetView)_spread.Sheets.GetSheetView(args.Worksheet);

            if (!sheetView.ViewPort.ViewRange.ContainsCell(args.Row, args.Column))
                return;

            switch (args.ChangeType)
            {
                case CellChangeType.Value:
                case CellChangeType.Formula:
                    break;
            }

            if (!CanInvalidate())
            {
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _spread.Invalidate()));
        }

        public void RangeChanged(RangeChangedEventArgs args)
        {
            var sheetView = (SheetView)_spread.Sheets.GetSheetView(args.Worksheet);

            if (!sheetView.ViewPort.ViewRange.Intersects(args.Range))
            {
                return;
            }

            if (!CanInvalidate())
            {
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _spread.Invalidate()));
        }

        public void ColumnChanged(ColumnChangedEventArgs args)
        {
            var sheetView = (SheetView)_spread.Sheets.GetSheetView(args.Worksheet);

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
                        case ColumnChangeType.Visibility:
                            sheetView.ViewPort.ResetColumnLocations();
                            break;
                    }
                    break;

            }

            sheetView.ViewPort.CalculateVisibleRange();

            if (args.ChangeType != ColumnChangeType.Visibility && !sheetView.ViewPort.ViewRange.ContainsColumn(args.Index))
            {
                return;
            }

            if (!CanInvalidate())
            {
                return;
            }

            _spread.SheetTabControl.UpdateScrollbars();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _spread.Invalidate()));
        }

        public void RowChanged(RowChangedEventArgs args)
        {
            var sheetView = (SheetView)_spread.Sheets.GetSheetView(args.Worksheet);

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
                        case RowChangeType.Visibility:
                            sheetView.ViewPort.ResetRowLocations();
                            break;
                    }
                    break;

            }

            sheetView.ViewPort.CalculateVisibleRange();
            if (args.ChangeType != RowChangeType.Visibility && !sheetView.ViewPort.ViewRange.ContainsRow(args.Index))
            {
                return;
            }

            if (!CanInvalidate())
            {
                return;
            }

            _spread.SheetTabControl.UpdateScrollbars();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _spread.Invalidate()));
        }

        private bool CanInvalidate()
        {
            return _spread.IsLoaded && !_suspendUpdates;
        }

        public void OnFilterChanged(FilterChangedEventArgs args)
        {
            var sheetView = (SheetView)_spread.Sheets.GetSheetView(args.Worksheet);
            if (sheetView == null) return;
            
            sheetView.ViewPort.ResetRowLocations();
            sheetView.ViewPort.CalculateVisibleRange();
            
            if (!CanInvalidate()) return;
            
            _spread.SheetTabControl.UpdateScrollbars();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _spread.Invalidate()));
        }
    }
}

