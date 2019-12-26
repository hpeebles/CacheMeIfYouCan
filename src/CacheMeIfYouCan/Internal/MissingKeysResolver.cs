using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class MissingKeysResolver<TKey, TValue>
    {
        public static IReadOnlyCollection<TKey> GetMissingKeys(
            IReadOnlyCollection<TKey> keys,
            Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary.Count == 0)
                return keys;
                
            List<TKey> missingKeysList;
            if (dictionary.Count < keys.Count)
            {
                missingKeysList = new List<TKey>(keys.Count - dictionary.Count);
                foreach (var key in keys)
                {
                    if (dictionary.ContainsKey(key))
                        continue;
             
                    missingKeysList.Add(key);
                }
                
                return missingKeysList;
            }

            missingKeysList = null;
            foreach (var key in keys)
            {
                if (dictionary.ContainsKey(key))
                    continue;
                
                if (missingKeysList is null)
                    missingKeysList = new List<TKey>();

                missingKeysList.Add(key);
            }
            
            return missingKeysList;
        }
    }
}