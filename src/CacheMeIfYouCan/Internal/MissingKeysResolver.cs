using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class MissingKeysResolver<TKey>
    {
        public static ReadOnlyMemory<TKey> GetMissingKeys(
            ReadOnlyMemory<TKey> inputKeys,
            HashSet<TKey> keysReturned,
            out TKey[] pooledArray)
        {
            if (keysReturned.Count == 0)
            {
                pooledArray = null;
                return inputKeys;
            }

            var span = inputKeys.Span;
            var missingKeys = keysReturned.Count < inputKeys.Length
                ? ArrayPool<TKey>.Shared.Rent(inputKeys.Length - keysReturned.Count)
                : null;

            var keysFound = 0;
            var missingKeyIndex = 0;
            foreach (var key in span)
            {
                if (keysReturned.Contains(key))
                {
                    keysFound++;
                    continue;
                }

                // These conditions will only be true if 'keysReturned' contains keys that are not in 'inputKeys'
                if (missingKeys is null)
                    missingKeys = ArrayPool<TKey>.Shared.Rent(16);
                else if (missingKeyIndex == missingKeys.Length)
                    Utilities.GrowPooledArray(ref missingKeys, inputKeys.Length - keysFound);

                missingKeys[missingKeyIndex++] = key;
            }

            pooledArray = missingKeys;
            return new ReadOnlyMemory<TKey>(pooledArray, 0, missingKeyIndex);
        }
    }

    internal static class MissingKeysResolver<TKey, TValue>
    {
        public static ReadOnlyMemory<TKey> GetMissingKeys(
            ReadOnlyMemory<TKey> inputKeys,
            Dictionary<TKey, TValue> dictionaryReturned,
            out TKey[] pooledArray)
        {
            if (dictionaryReturned is null || dictionaryReturned.Count == 0)
            {
                pooledArray = null;
                return inputKeys;
            }

            var span = inputKeys.Span;
            var missingKeys = dictionaryReturned.Count < inputKeys.Length
                ? ArrayPool<TKey>.Shared.Rent(inputKeys.Length - dictionaryReturned.Count)
                : null;

            var keysFound = 0;
            var missingKeyIndex = 0;
            foreach (var key in span)
            {
                if (dictionaryReturned.ContainsKey(key))
                {
                    keysFound++;
                    continue;
                }

                // These conditions will only be true if 'dictionaryReturned' contains keys that are not in 'inputKeys'
                if (missingKeys is null)
                    missingKeys = ArrayPool<TKey>.Shared.Rent(16);
                else if (missingKeyIndex == missingKeys.Length)
                    Utilities.GrowPooledArray(ref missingKeys, inputKeys.Length - keysFound);

                missingKeys[missingKeyIndex++] = key;
            }

            pooledArray = missingKeys;
            return new ReadOnlyMemory<TKey>(pooledArray, 0, missingKeyIndex);
        }
    }
}