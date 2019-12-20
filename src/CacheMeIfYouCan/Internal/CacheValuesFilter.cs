using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheValuesFilter<TKey, TValue>
    {
        private static readonly ArrayPool<KeyValuePair<TKey, TValue>> ArrayPool = ArrayPool<KeyValuePair<TKey, TValue>>.Shared;
        
        public static ArraySegment<KeyValuePair<TKey, TValue>> Filter(
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> values,
            Func<TKey, TValue, bool> valuesToSkipPredicate,
            out KeyValuePair<TKey, TValue>[] pooledArray)
        {
            pooledArray = ArrayPool.Rent(values.Count);
            
            var index = 0;
            foreach (var kv in values)
            {
                if (!valuesToSkipPredicate(kv.Key, kv.Value))
                    pooledArray[index++] = kv;
            }
            
            return new ArraySegment<KeyValuePair<TKey, TValue>>(pooledArray, 0, index);
        }
        
        public static void ReturnPooledArray(KeyValuePair<TKey, TValue>[] pooledArray) => ArrayPool.Return(pooledArray);
    }
}