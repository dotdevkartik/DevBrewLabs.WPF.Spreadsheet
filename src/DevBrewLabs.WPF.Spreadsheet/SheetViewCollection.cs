using DevBrewLabs.Spreadsheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace DevBrewLabs.WPF.Spreadsheet
{
    public class SheetViewCollection : IEnumerable<ISheetView>, INotifyCollectionChanged
    {
        private Spread _spread;
        private Dictionary<IWorksheet, ISheetView> _sheetViewStore;

        public ISheetView ActiveSheet { get; private set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler<SheetViewEventArgs> ActiveSheetChanged;

        public SheetViewCollection(Spread spread)
        {
            _spread = spread;
            _sheetViewStore = new Dictionary<IWorksheet, ISheetView>();
            var workSheets = _spread.WorkBook.WorkSheets;
            WeakEventManager<IWorksheets, WorksheetAddedEventArgs>.AddHandler(workSheets, "SheetAdded", OnSheetAdded);
            WeakEventManager<IWorksheets, WorksheetRemovedEventArgs>.AddHandler(workSheets, "SheetRemoved", OnSheetRemoved);
            WeakEventManager<IWorksheets, WorksheetEventArgs>.AddHandler(workSheets, "ActiveSheetChanged", OnActiveSheetChanged);
        }

        ~SheetViewCollection()
        {
            var workSheets = _spread.WorkBook.WorkSheets;
            WeakEventManager<IWorksheets, WorksheetAddedEventArgs>.RemoveHandler(workSheets, "SheetAdded", OnSheetAdded);
            WeakEventManager<IWorksheets, WorksheetRemovedEventArgs>.RemoveHandler(workSheets, "SheetRemoved", OnSheetRemoved);
            WeakEventManager<IWorksheets, WorksheetEventArgs>.RemoveHandler(workSheets, "ActiveSheetChanged", OnActiveSheetChanged);
        }

        private void OnSheetAdded(object sender, WorksheetAddedEventArgs e)
        {
            var sheetView = new SheetView(_spread, e.AddedSheet);
            _sheetViewStore.Add(e.AddedSheet, sheetView);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sheetView));
        }

        private void OnSheetRemoved(object sender, WorksheetRemovedEventArgs e)
        {
            var sheetView = _sheetViewStore[e.RemovedSheet];
            _sheetViewStore.Remove(e.RemovedSheet);

            if (_spread.WorkBook.WorkSheets.Count == 0)
            {
                ActiveSheet = null;
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, sheetView));
        }

        private void OnActiveSheetChanged(object sender, WorksheetEventArgs e)
        {
            var args = new SheetViewEventArgs() { OldSheetView = ActiveSheet };
            ActiveSheet = _sheetViewStore[e.Worksheet];
            args.NewSheetView = ActiveSheet;
            ActiveSheetChanged?.Invoke(this, args);
        }

        public IEnumerator<ISheetView> GetEnumerator()
        {
            return _sheetViewStore.Values.GetEnumerator();
        }

        public ISheetView GetSheetView(IWorksheet workSheet)
        {
            _sheetViewStore.TryGetValue(workSheet, out var sheetView);
            return sheetView;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
