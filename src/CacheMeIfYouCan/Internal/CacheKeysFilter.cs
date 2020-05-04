using System;
using System.Buffers;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheKeysFilter<TKey>
    {
        public static ReadOnlyMemory<TKey> Filter(
            ReadOnlyMemory<TKey> keys,
            Func<TKey, bool> keysToSkipPredicate,
            out TKey[] pooledArray)
        {
            var span = keys.Span;
            var skipFirst = keysToSkipPredicate(span[0]);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < keys.Length)
                {
                    if (!keysToSkipPredicate(span[index++]))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return ReadOnlyMemory<TKey>.Empty;
                }

                var remaining = keys.Length + 1 - index;
                pooledArray = ArrayPool<TKey>.Shared.Rent(remaining);
                pooledArray[0] = span[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < keys.Length)
                {
                    if (keysToSkipPredicate(span[index++]))
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

                pooledArray = ArrayPool<TKey>.Shared.Rent(keys.Length - 1);
                span.Slice(0, index - 1).CopyTo(pooledArray);
                outputIndex = index - 1;
            }

            while (index < keys.Length)
            {
                var next = span[index++];
                if (!keysToSkipPredicate(next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ReadOnlyMemory<TKey>(pooledArray, 0, outputIndex);
        }
    }
    
    internal static class CacheKeysFilter<TOuterKey, TInnerKey>
    {
        public static ReadOnlyMemory<TInnerKey> Filter(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> keys,
            Func<TOuterKey, TInnerKey, bool> keysToSkipPredicate,
            out TInnerKey[] pooledArray)
        {
            var span = keys.Span;
            var skipFirst = keysToSkipPredicate(outerKey, span[0]);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < keys.Length)
                {
                    if (!keysToSkipPredicate(outerKey, span[index++]))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return ReadOnlyMemory<TInnerKey>.Empty;
                }

                var remaining = keys.Length + 1 - index;
                pooledArray = ArrayPool<TInnerKey>.Shared.Rent(remaining);
                pooledArray[0] = span[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < keys.Length)
                {
                    if (keysToSkipPredicate(outerKey, span[index++]))
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

                pooledArray = ArrayPool<TInnerKey>.Shared.Rent(keys.Length - 1);
                span.Slice(0, index - 1).CopyTo(pooledArray);
                outputIndex = index - 1;
            }

            while (index < keys.Length)
            {
                var next = span[index++];
                if (!keysToSkipPredicate(outerKey, next))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ReadOnlyMemory<TInnerKey>(pooledArray, 0, outputIndex);
        }
    }
}