using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheValuesFilter<TKey, TValue>
    {
        public static ReadOnlyMemory<KeyValuePair<TKey, TValue>> Filter(
            ReadOnlyMemory<KeyValuePair<TKey, TValue>> values,
            Func<TKey, TValue, bool> valuesToSkipPredicate,
            out KeyValuePair<TKey, TValue>[] pooledArray)
        {
            if (values.Length == 0)
            {
                pooledArray = null;
                return values;
            }
            
            var span = values.Span;
            var first = span[0];
            var skipFirst = valuesToSkipPredicate(first.Key, first.Value);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < span.Length)
                {
                    var kv = span[index++];
                    if (!valuesToSkipPredicate(kv.Key, kv.Value))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return ReadOnlyMemory<KeyValuePair<TKey, TValue>>.Empty;
                }

                var remaining = values.Length + 1 - index;
                pooledArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(remaining);
                pooledArray[0] = span[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < span.Length)
                {
                    var kv = span[index++];
                    if (valuesToSkipPredicate(kv.Key, kv.Value))
                    {
                        skipAny = true;
                        break;
                    }
                }

                if (!skipAny)
                {
                    pooledArray = null;
                    return values;
                }

                pooledArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(values.Length - 1);
                span.Slice(0, index - 1).CopyTo(pooledArray);
                outputIndex = index - 1;
            }

            while (index < span.Length)
            {
                var next = span[index++];
                if (!valuesToSkipPredicate(next.Key, next.Value))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ReadOnlyMemory<KeyValuePair<TKey, TValue>>(pooledArray, 0, outputIndex);
        }
    }
    
    internal static class CacheValuesFilter<TParams, TInnerKey, TValue>
    {
        public static ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> Filter(
            TParams outerParams,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            Func<TParams, TInnerKey, TValue, bool> valuesToSkipPredicate,
            out KeyValuePair<TInnerKey, TValue>[] pooledArray)
        {
            if (values.Length == 0)
            {
                pooledArray = null;
                return values;
            }
            
            var span = values.Span;
            var first = span[0];
            var skipFirst = valuesToSkipPredicate(outerParams, first.Key, first.Value);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < span.Length)
                {
                    var kv = span[index++];
                    if (!valuesToSkipPredicate(outerParams, kv.Key, kv.Value))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>>.Empty;
                }

                var remaining = values.Length + 1 - index;
                pooledArray = ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(remaining);
                pooledArray[0] = span[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < span.Length)
                {
                    var kv = span[index++];
                    if (valuesToSkipPredicate(outerParams, kv.Key, kv.Value))
                    {
                        skipAny = true;
                        break;
                    }
                }

                if (!skipAny)
                {
                    pooledArray = null;
                    return values;
                }

                pooledArray = ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(values.Length - 1);
                span.Slice(0, index - 1).CopyTo(pooledArray);
                outputIndex = index - 1;
            }

            while (index < span.Length)
            {
                var next = span[index++];
                if (!valuesToSkipPredicate(outerParams, next.Key, next.Value))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>>(pooledArray, 0, outputIndex);
        }
        
        public static ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> Filter(
            TParams outerParams,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            Func<TParams, TInnerKey, TValue, bool> valuesToSkipPredicate,
            out KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[] pooledArray)
        {
            if (values.Length == 0)
            {
                pooledArray = null;
                return values;
            }
            
            var first = values[0];
            var skipFirst = valuesToSkipPredicate(outerParams, first.Key, first.Value);

            var index = 1;
            int outputIndex;
            if (skipFirst)
            {
                var includeAny = false;
                while (index < values.Length)
                {
                    var kv = values[index++];
                    if (!valuesToSkipPredicate(outerParams, kv.Key, kv.Value))
                    {
                        includeAny = true;
                        break;
                    }
                }

                if (!includeAny)
                {
                    pooledArray = null;
                    return ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Empty;
                }

                var remaining = values.Length + 1 - index;
                pooledArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(remaining);
                pooledArray[0] = values[index - 1];
                outputIndex = 1;
            }
            else
            {
                var skipAny = false;
                while (index < values.Length)
                {
                    var kv = values[index++];
                    if (valuesToSkipPredicate(outerParams, kv.Key, kv.Value))
                    {
                        skipAny = true;
                        break;
                    }
                }

                if (!skipAny)
                {
                    pooledArray = null;
                    return values;
                }

                pooledArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(values.Length - 1);
                values.Slice(0, index - 1).CopyTo(pooledArray);
                outputIndex = index - 1;
            }

            while (index < values.Length)
            {
                var next = values[index++];
                if (!valuesToSkipPredicate(outerParams, next.Key, next.Value))
                    pooledArray[outputIndex++] = next;
            }
            
            return new ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>(pooledArray, 0, outputIndex);
        }
    }
}