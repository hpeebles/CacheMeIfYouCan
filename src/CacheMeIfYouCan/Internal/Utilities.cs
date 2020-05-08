using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public static class Utilities
    {
        public static void GrowPooledArray<TKey>(ref TKey[] array, int maxSize)
        {
            var newArrayLength = Math.Min(array.Length * 2, maxSize);
            var newArray = ArrayPool<TKey>.Shared.Rent(newArrayLength);
            Array.Copy(array, newArray, array.Length);
            ArrayPool<TKey>.Shared.Return(array);
            array = newArray;
        }

        public static void AddValuesToDictionary<TKey, TValue>(
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            Dictionary<TKey, TValue> dictionary)
        {
            foreach (var kv in values)
                dictionary[kv.Key] = kv.Value;
        }
    }
}