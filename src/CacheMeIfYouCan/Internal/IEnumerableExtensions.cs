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
    }
}