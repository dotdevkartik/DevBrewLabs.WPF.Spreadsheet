using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal class WorkSheets : IWorkSheets
    {
        private HashSet<IWorkSheet> _sheets;
        private IWorkSheet _activeSheet;
        private WorkBook _workBook;

        public int Count => _sheets.Count;
        public IWorkBook WorkBook => _workBook;

        public IWorkSheet this[string sheetName]
        {
            get
            {
                return GetSheet(sheetName);
            }
        }

        public IWorkSheet this[int index]
        {
            get
            {
                return GetSheet(index);
            }
        }

        public IWorkSheet ActiveSheet
        {
            get
            {
                return _activeSheet;
            }
            set
            {
                SetActiveSheet(value);
            }
        }

        public int ActiveSheetIndex
        {
            get
            {
                int index = 0;
                foreach(var sheet in _sheets)
                {
                    if (sheet == _activeSheet)
                        return index;
                    index++;
                }
                return -1;
            }
            set
            {
                var sheet = GetSheet(value);
                SetActiveSheet(sheet);
            }
        }

        public event EventHandler<SheetChangedEventArgs> SheetAdded;
        public event EventHandler<SheetChangedEventArgs> SheetRemoved;
        public event EventHandler<SheetChangedEventArgs> ActiveSheetChanged;

        internal WorkSheets(WorkBook workBook)
        {
            _workBook = workBook;
            _sheets = new HashSet<IWorkSheet>();
        }

        public IWorkSheet AddSheet(string name)
        {
            VerifySheetName(name);
            var workSheet = new WorkSheet(_workBook, name);
            _sheets.Add(workSheet);
            SheetAdded?.Invoke(this, new SheetChangedEventArgs(workSheet));
            return workSheet;
        }

        /// <summary>
        /// Verifies if a sheet is already present with the same name.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException"></exception>
        internal void VerifySheetName(string name, IWorkSheet currentSheet = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Sheet name cannot be null or empty.");
            }

            if (_sheets.Any(s => s != currentSheet && s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Sheet with name '{name}' already exists.");
        }

        public IWorkSheet GetSheet(string sheetName)
        {
            sheetName = sheetName.ToLowerInvariant();

            if (!_sheets.Any(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Sheet with name '{sheetName}' does not exist.");

            var sheet = _sheets.First(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
            return sheet;
        }

        public IWorkSheet GetSheet(int index)
        {
            if (_sheets.Count <= index || index < 0)
                throw new IndexOutOfRangeException("Sheet index is out of range.");

            var sheet = _sheets.ElementAt(index);
            return sheet;
        }

        private void SetActiveSheet(IWorkSheet sheet)
        {
            if (!_sheets.Contains(sheet))
            {
                throw new InvalidOperationException("Provided sheet doesn't belong to this workbook");
            }

            _activeSheet = sheet;
            OnActiveSheetChanged(sheet);
        }

        public void RemoveSheet(string name)
        {
            var sheet = GetSheet(name);
            _sheets.Remove(sheet);
            if (_activeSheet == sheet)
                _activeSheet = null;
            sheet.Dispose();
            SheetRemoved?.Invoke(this, new SheetChangedEventArgs(sheet));
        }

        public void RemoveSheet(int index)
        {
            var sheet = GetSheet(index);
            _sheets.Remove(sheet);
            if (_activeSheet == sheet)
                _activeSheet = null;
            sheet.Dispose();
            SheetRemoved?.Invoke(this, new SheetChangedEventArgs(sheet));
        }

        public void Clear()
        {
            foreach(var sheet in _sheets.ToList())
            {
                RemoveSheet(sheet.Name);
            }
            _activeSheet = null;
        }

        protected virtual void OnActiveSheetChanged(IWorkSheet sheet)
        {
            ActiveSheetChanged?.Invoke(this, new SheetChangedEventArgs(sheet));
        }

        public void Dispose()
        {
            Clear();
            _sheets = null;
            _workBook = null;
            _activeSheet = null;
        }

        public IEnumerator<IWorkSheet> GetEnumerator()
        {
            return _sheets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
