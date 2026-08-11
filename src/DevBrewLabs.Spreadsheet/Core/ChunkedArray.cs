using System.Collections.Generic;

namespace DevBrewLabs.Spreadsheet.Core
{
    /// <summary>
    /// A chunked array that stores data in contiguous blocks of memory.
    /// This provides the memory efficiency of sparse matrices for largely empty spaces, 
    /// while retaining the low overhead and speed of flat arrays for dense regions.
    /// </summary>
    internal class ChunkedArray<T>
    {
        private const int ChunkSize = 1024;
        private readonly Dictionary<int, T[]> _chunks = new Dictionary<int, T[]>();

        public T GetValue(int index)
        {
            int chunkIndex = index / ChunkSize;
            if (_chunks.TryGetValue(chunkIndex, out var chunk))
            {
                return chunk[index % ChunkSize];
            }
            return default;
        }

        public void SetValue(int index, T value)
        {
            int chunkIndex = index / ChunkSize;
            
            if (!_chunks.TryGetValue(chunkIndex, out var chunk))
            {
                // If setting to default value and chunk doesn't exist, avoid allocation.
                if (EqualityComparer<T>.Default.Equals(value, default))
                    return;

                chunk = new T[ChunkSize];
                _chunks[chunkIndex] = chunk;
            }

            chunk[index % ChunkSize] = value;
        }

        public bool ContainsKey(int index)
        {
            int chunkIndex = index / ChunkSize;
            if (_chunks.TryGetValue(chunkIndex, out var chunk))
            {
                return !EqualityComparer<T>.Default.Equals(chunk[index % ChunkSize], default);
            }
            return false;
        }

        public void Remove(int index)
        {
            SetValue(index, default);
        }

        public void Clear()
        {
            _chunks.Clear();
        }
    }
}
