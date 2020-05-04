using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class MissingKeysResolver<TKey, TValue>
    {
        public static ReadOnlyMemory<TKey> GetMissingKeys(
            ReadOnlyMemory<TKey> keys,
            Dictionary<TKey, TValue> dictionary,
            out TKey[] pooledArray)
        {
            if (dictionary.Count == 0)
            {
                pooledArray = null;
                return keys;
            }

            var span = keys.Span;
            var missingKeys = dictionary.Count < keys.Length
                ? ArrayPool<TKey>.Shared.Rent(keys.Length - dictionary.Count)
                : null;

            var keysFound = 0;
            var missingKeyIndex = 0;
            foreach (var key in span)
            {
                if (dictionary.ContainsKey(key))
                {
                    keysFound++;
                    continue;
                }

                // These conditions will only be true if 'dictionary' contains keys that are not also contained in 'keys'
                if (missingKeys is null)
                    missingKeys = ArrayPool<TKey>.Shared.Rent(16);
                else if (missingKeyIndex == missingKeys.Length)
                    GrowArray(ref missingKeys, keys.Length - keysFound);

                missingKeys[missingKeyIndex++] = key;
            }

            pooledArray = missingKeys;
            return new ReadOnlyMemory<TKey>(pooledArray, 0, missingKeyIndex);
        }

        private static void GrowArray(ref TKey[] array, int maxSize)
        {
            var newArrayLength = Math.Min(array.Length * 2, maxSize);
            var newArray = ArrayPool<TKey>.Shared.Rent(newArrayLength);
            Array.Copy(array, newArray, array.Length);
            ArrayPool<TKey>.Shared.Return(array);
            array = newArray;
        }
    }
}