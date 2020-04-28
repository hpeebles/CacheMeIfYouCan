using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class MissingKeysResolver<TKey, TValue>
    {
        public static IReadOnlyCollection<TKey> GetMissingKeys(
            IReadOnlyCollection<TKey> keys,
            Dictionary<TKey, TValue> dictionary,
            out TKey[] pooledArray)
        {
            if (dictionary.Count == 0)
            {
                pooledArray = null;
                return keys;
            }

            if (dictionary.Count < keys.Count)
            {
                var missingKeys = ArrayPool<TKey>.Shared.Rent(keys.Count - dictionary.Count);
                var keysFound = 0;
                var missingKeyIndex = 0;
                foreach (var key in keys)
                {
                    if (dictionary.ContainsKey(key))
                    {
                        keysFound++;
                        continue;
                    }

                    if (missingKeyIndex == missingKeys.Length)
                    {
                        // This will only happen if 'dictionary' contains keys that are not also contained in 'keys'  
                        var newArrayLength = Math.Min(missingKeys.Length * 2, keys.Count - keysFound);
                        var newArray = ArrayPool<TKey>.Shared.Rent(newArrayLength);
                        Array.Copy(missingKeys, newArray, missingKeys.Length);
                        ArrayPool<TKey>.Shared.Return(missingKeys);
                        missingKeys = newArray;
                    }
                    missingKeys[missingKeyIndex++] = key;
                }
                
                pooledArray = missingKeys;
                return new ArraySegment<TKey>(pooledArray, 0, missingKeyIndex);
            }

            List<TKey> missingKeysList = null;
            foreach (var key in keys)
            {
                if (dictionary.ContainsKey(key))
                    continue;
                
                // This will only happen if 'dictionary' contains keys that are not also contained in 'keys'
                if (missingKeysList is null)
                    missingKeysList = new List<TKey>();

                missingKeysList.Add(key);
            }

            pooledArray = null;
            return missingKeysList;
        }
    }
}