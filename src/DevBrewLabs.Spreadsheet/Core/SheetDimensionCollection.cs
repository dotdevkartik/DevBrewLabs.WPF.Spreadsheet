using System;
using System.Collections.Generic;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal abstract class SheetDimensionCollection<T> : IDimensionCollection<T>, IDisposable where T : class
    {
        private SortedDictionary<int, T> _collection;

        public T this[int index]
        {
            get
            {
                return GetItem(index, true);
            }
        }

        public bool HasItems => _collection != null && _collection.Count > 0;

        internal SheetDimensionCollection()
        {
            _collection = new SortedDictionary<int, T>();
        }

        public virtual void Insert(int index, int count)
        {
            if (count <= 0 || _collection == null || _collection.Count == 0) return;

            var keysToShift = _collection.Keys.Where(k => k >= index).OrderByDescending(k => k).ToList();
            foreach (var k in keysToShift)
            {
                var item = _collection[k];
                _collection.Remove(k);
                UpdateItemIndex(item, k + count);
                _collection[k + count] = item;
            }
        }

        public virtual void Remove(int index, int count)
        {
            if (count <= 0 || _collection == null || _collection.Count == 0) return;

            var keysToRemove = _collection.Keys.Where(k => k >= index && k < index + count).ToList();
            foreach (var k in keysToRemove)
            {
                var item = _collection[k];
                _collection.Remove(k);
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }

            var keysToShift = _collection.Keys.Where(k => k >= index + count).OrderBy(k => k).ToList();
            foreach (var k in keysToShift)
            {
                var item = _collection[k];
                _collection.Remove(k);
                UpdateItemIndex(item, k - count);
                _collection[k - count] = item;
            }
        }

        protected virtual void UpdateItemIndex(T item, int newIndex)
        {
            if (item is Row r) r.Index = newIndex;
            else if (item is Column c) c.Index = newIndex;
            else if (item is ColumnHeaderRow chr) chr.Index = newIndex;
            else if (item is RowHeaderColumn rhc) rhc.Index = newIndex;
        }

        public int GetIndex(T item)
        {
            if (_collection == null) return -1;
            var result = _collection.FirstOrDefault(x => x.Value == item);
            return result.Key;
        }

        /// <summary>
        /// Gets item from collection. returns null if item doesn't exist.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetItem(int index)
        {
            return GetItem(index, false);
        }

        /// <summary>
        /// Gets the item present at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index of the item.
        /// </param>
        /// <param name="createIfNotExist">
        /// Whether to create and add the item if not exist.
        /// </param>
        /// <returns></returns>
        protected T GetItem(int index, bool createIfNotExist)
        {
            if (_collection == null) return null;

            if (_collection.TryGetValue(index, out T item))
            {
                return item;
            }
            else if (createIfNotExist)
            {
                return AddItemInternal(index);
            }
            else
                return null;
        }

        internal bool RemoveItem(int index)
        {
            return _collection != null && _collection.Remove(index);
        }
        internal void Clear()
        {
            if (_collection == null) return;
            foreach (var item in _collection.Values)
            {
                if (item is IDisposable d) d.Dispose();
            }
            _collection.Clear();
        }

        internal IEnumerable<T> GetItems()
        {
            return _collection != null ? (IEnumerable<T>)_collection.Values : Enumerable.Empty<T>();
        }


        /// <summary>
        /// Adds a new item of type T at the provided index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected T AddItemInternal(int index)
        {
            var item = CreateItem(index);
            _collection.Add(index, item);
            return item;
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateItem(int index);

        public void Dispose()
        {
            _collection?.Clear();
            _collection = null;
        }
    }
}
