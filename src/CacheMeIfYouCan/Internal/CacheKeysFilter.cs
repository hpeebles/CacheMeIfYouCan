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
            return keys switch
            {
                TKey[] keysArray => FilterArray(keysArray, keysToSkipPredicate, out pooledArray),
                List<TKey> keysList => FilterList(keysList, keysToSkipPredicate, out pooledArray),
                _ => FilterIReadOnlyCollection(keys, keysToSkipPredicate, out pooledArray)
            };
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
                Array.Copy(keys, pooledArray, index - 1);
                outputIndex = index - 1;
            }

            while (index < keys.Length)
            {
                var next = keys[index++];
                if (!keysToSkipPredicate(next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ArraySegment<TKey>(pooledArray, 0, outputIndex);
        }
        
        private static IReadOnlyCollection<TKey> FilterList(
            List<TKey> keys,
            Func<TKey, bool> keysToSkipPredicate,
            out TKey[] pooledArray)
        {
            var skipFirst = keysToSkipPredicate(keys[0]);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < keys.Count)
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

                var remaining = keys.Count + 1 - index;
                pooledArray = ArrayPool.Rent(remaining);
                pooledArray[0] = keys[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < keys.Count)
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

                pooledArray = ArrayPool.Rent(keys.Count - 1);
                keys.CopyTo(0, pooledArray, 0, index - 1);
                outputIndex = index - 1;
            }

            while (index < keys.Count)
            {
                var next = keys[index++];
                if (!keysToSkipPredicate(next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ArraySegment<TKey>(pooledArray, 0, outputIndex);
        }
        
        public static IReadOnlyCollection<TKey> FilterIReadOnlyCollection(
            IReadOnlyCollection<TKey> keys,
            Func<TKey, bool> keysToSkipPredicate,
            out TKey[] pooledArray)
        {
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
            return keys switch
            {
                TInnerKey[] keysArray => FilterArray(outerKey, keysArray, keysToSkipPredicate, out pooledArray),
                List<TInnerKey> keysList => FilterList(outerKey, keysList, keysToSkipPredicate, out pooledArray),
                _ => FilterIReadOnlyCollection(outerKey, keys, keysToSkipPredicate, out pooledArray)
            };
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
                Array.Copy(keys, pooledArray, index - 1);
                outputIndex = index - 1;
            }

            while (index < keys.Length)
            {
                var next = keys[index++];
                if (!keysToSkipPredicate(outerKey, next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ArraySegment<TInnerKey>(pooledArray, 0, outputIndex);
        }
        
        private static IReadOnlyCollection<TInnerKey> FilterList(
            TOuterKey outerKey,
            List<TInnerKey> keys,
            Func<TOuterKey, TInnerKey, bool> keysToSkipPredicate,
            out TInnerKey[] pooledArray)
        {
            var skipFirst = keysToSkipPredicate(outerKey, keys[0]);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < keys.Count)
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

                var remaining = keys.Count + 1 - index;
                pooledArray = ArrayPool.Rent(remaining);
                pooledArray[0] = keys[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < keys.Count)
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

                pooledArray = ArrayPool.Rent(keys.Count - 1);
                keys.CopyTo(0, pooledArray, 0, index - 1);
                outputIndex = index - 1;
            }

            while (index < keys.Count)
            {
                var next = keys[index++];
                if (!keysToSkipPredicate(outerKey, next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ArraySegment<TInnerKey>(pooledArray, 0, outputIndex);
        }
        
        private static IReadOnlyCollection<TInnerKey> FilterIReadOnlyCollection(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> keys,
            Func<TOuterKey, TInnerKey, bool> keysToSkipPredicate,
            out TInnerKey[] pooledArray)
        {
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

        public static void ReturnPooledArray(TInnerKey[] pooledArray) => ArrayPool.Return(pooledArray);
    }
}