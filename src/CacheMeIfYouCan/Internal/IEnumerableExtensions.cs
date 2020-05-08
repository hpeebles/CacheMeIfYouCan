using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal
{
    internal static class IEnumerableExtensions
    {
        public static ReadOnlyMemory<T> ToReadOnlyMemory<T>(this IEnumerable<T> source, out T[] pooledArray)
        {
            if (source is null)
            {
                pooledArray = null;
                return ReadOnlyMemory<T>.Empty;
            }

            if (source is T[] array)
            {
                pooledArray = null;
                return new ReadOnlyMemory<T>(array);
            }

            if (source is IReadOnlyCollection<T> collection)
            {
                if (collection.Count == 0)
                {
                    pooledArray = null;
                    return ReadOnlyMemory<T>.Empty;
                }

                pooledArray = ArrayPool<T>.Shared.Rent(collection.Count);
                var index = 0;
                foreach (var value in collection)
                    pooledArray[index++] = value;
                
                return new ReadOnlyMemory<T>(pooledArray, 0, index);
            }

            pooledArray = null;
            return source.ToArray();
        }
        
        public static ReadOnlyMemory<KeyValuePair<TKey, TValue>> ToReadOnlyMemory<TKey, TValue>(
            this Dictionary<TKey, TValue> source, out KeyValuePair<TKey, TValue>[] pooledArray)
        {
            if (source is null || source.Count == 0)
            {
                pooledArray = null;
                return ReadOnlyMemory<KeyValuePair<TKey, TValue>>.Empty;
            }

            pooledArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(source.Count);
            var index = 0;
            foreach (var value in source)
                pooledArray[index++] = value;

            return new ReadOnlyMemory<KeyValuePair<TKey, TValue>>(pooledArray, 0, index);
        }
    }
}