using System;
using System.Collections.Generic;
using System.Linq;

namespace DevBrewLabs.Spreadsheet
{
    internal abstract class SheetDimensionCollection<T> : IDisposable where T : class
    {
        private SortedDictionary<int, T> _collection;

        public T this[int index]
        {
            get
            {
                return GetItem(index, true);
            }
        }

        public bool HasItems => _collection.Count > 0;

        internal SheetDimensionCollection()
        {
            _collection = new SortedDictionary<int, T>();
        }

        public abstract void Insert(int index, int count);

        public abstract void Remove(int index, int count);

        public int GetIndex(T item)
        {
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
            _collection.Clear();
            _collection = null;
        }
    }
}
