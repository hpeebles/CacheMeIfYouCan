using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheKeysFilter<TKey>
    {
        private static readonly ArrayPool<TKey> ArrayPool = ArrayPool<TKey>.Shared;
        
        public static IReadOnlyCollection<TKey> Filter(
            IReadOnlyCollection<TKey> keys,
            Func<TKey, bool> keysToSkipPredicate,
            out TKey[] pooledArray)
        {
            if (keys is TKey[] keysArray)
                return FilterArray(keysArray, keysToSkipPredicate, out pooledArray);

            pooledArray = null;
            
            var countIncluded = 0;
            var index = 0;
            foreach (var key in keys)
            {
                if (!keysToSkipPredicate(key))
                {
                    if (pooledArray is null)
                        pooledArray = ArrayPool.Rent(keys.Count - index);

                    pooledArray[countIncluded++] = key;
                }

                index++;
            }

            if (pooledArray is null)
                return Array.Empty<TKey>();

            return new ArraySegment<TKey>(pooledArray, 0, countIncluded);
        }

        private static IReadOnlyCollection<TKey> FilterArray(
            TKey[] keys,
            Func<TKey, bool> keysToSkipPredicate,
            out TKey[] pooledArray)
        {
            var skipFirst = keysToSkipPredicate(keys[0]);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < keys.Length)
                {
                    if (!keysToSkipPredicate(keys[index++]))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return Array.Empty<TKey>();
                }

                var remaining = keys.Length + 1 - index;
                pooledArray = ArrayPool.Rent(remaining);
                pooledArray[0] = keys[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < keys.Length)
                {
                    if (keysToSkipPredicate(keys[index++]))
                    {
                        skipAny = true;
                        break;
                    }
                }

                if (!skipAny)
                {
                    pooledArray = null;
                    return keys;
                }

                pooledArray = ArrayPool.Rent(keys.Length - 1);
                outputIndex = 0;
                while (outputIndex < index - 1)
                {
                    pooledArray[outputIndex] = keys[outputIndex];
                    outputIndex++;
                }
            }

            while (index < keys.Length)
            {
                var next = keys[index++];
                if (!keysToSkipPredicate(next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ArraySegment<TKey>(pooledArray, 0, outputIndex);
        }

        public static void ReturnPooledArray(TKey[] pooledArray) => ArrayPool.Return(pooledArray);
    }
    
    internal static class CacheKeysFilter<TOuterKey, TInnerKey>
    {
        private static readonly ArrayPool<TInnerKey> ArrayPool = ArrayPool<TInnerKey>.Shared;
        
        public static IReadOnlyCollection<TInnerKey> Filter(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> keys,
            Func<TOuterKey, TInnerKey, bool> keysToSkipPredicate,
            out TInnerKey[] pooledArray)
        {
            if (keys is TInnerKey[] keysArray)
                return FilterArray(outerKey, keysArray, keysToSkipPredicate, out pooledArray);
            
            pooledArray = null;
            
            var countIncluded = 0;
            var index = 0;
            foreach (var key in keys)
            {
                if (!keysToSkipPredicate(outerKey, key))
                {
                    if (pooledArray is null)
                        pooledArray = ArrayPool.Rent(keys.Count - index);

                    pooledArray[countIncluded++] = key;
                }

                index++;
            }

            if (pooledArray is null)
                return Array.Empty<TInnerKey>();

            return new ArraySegment<TInnerKey>(pooledArray, 0, countIncluded);
        }
        
        private static IReadOnlyCollection<TInnerKey> FilterArray(
            TOuterKey outerKey,
            TInnerKey[] keys,
            Func<TOuterKey, TInnerKey, bool> keysToSkipPredicate,
            out TInnerKey[] pooledArray)
        {
            var skipFirst = keysToSkipPredicate(outerKey, keys[0]);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < keys.Length)
                {
                    if (!keysToSkipPredicate(outerKey, keys[index++]))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return Array.Empty<TInnerKey>();
                }

                var remaining = keys.Length + 1 - index;
                pooledArray = ArrayPool.Rent(remaining);
                pooledArray[0] = keys[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < keys.Length)
                {
                    if (keysToSkipPredicate(outerKey, keys[index++]))
                    {
                        skipAny = true;
                        break;
                    }
                }

                if (!skipAny)
                {
                    pooledArray = null;
                    return keys;
                }

                pooledArray = ArrayPool.Rent(keys.Length - 1);
                outputIndex = 0;
                while (outputIndex < index - 1)
                {
                    pooledArray[outputIndex] = keys[outputIndex];
                    outputIndex++;
                }
            }

            while (index < keys.Length)
            {
                var next = keys[index++];
                if (!keysToSkipPredicate(outerKey, next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ArraySegment<TInnerKey>(pooledArray, 0, outputIndex);
        }

        public static void ReturnPooledArray(TInnerKey[] pooledArray) => ArrayPool.Return(pooledArray);
    }
}