using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheKeysFilter<TKey>
    {
        private static readonly ArrayPool<TKey> ArrayPool = ArrayPool<TKey>.Shared;
        
        public static ArraySegment<TKey> Filter(
            IReadOnlyCollection<TKey> keys,
            Func<TKey, bool> keysToSkipPredicate,
            out TKey[] pooledArray)
        {
            pooledArray = ArrayPool.Rent(keys.Count);
            
            var index = 0;
            foreach (var key in keys)
            {
                if (!keysToSkipPredicate(key))
                    pooledArray[index++] = key;
            }

            return new ArraySegment<TKey>(pooledArray, 0, index);
        }

        public static void ReturnPooledArray(TKey[] pooledArray) => ArrayPool.Return(pooledArray);
    }
}