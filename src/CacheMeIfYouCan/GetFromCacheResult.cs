using System;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the result of a call to get a single value from a cache
    /// </summary>
    /// <typeparam name="TK">The type of the cache key</typeparam>
    /// <typeparam name="TV">The type of the cache value</typeparam>
    public readonly struct GetFromCacheResult<TK, TV>
    {
        public GetFromCacheResult(Key<TK> key, TV value, TimeSpan timeToLive, string cacheType)
        {
            Key = key;
            Value = value;
            TimeToLive = timeToLive;
            CacheType = cacheType;
            Success = true;
        }
        
        /// <summary>
        /// The key which was looked up in the cache
        /// </summary>
        public Key<TK> Key { get; }
        
        /// <summary>
        /// The value found in the cache, or default if not found
        /// </summary>
        public TV Value { get; }
        
        /// <summary>
        /// The remaining time to live of the key, or default if not found
        /// </summary>
        public TimeSpan TimeToLive { get; }
        
        /// <summary>
        /// The type of the cache
        /// </summary>
        public string CacheType { get; }
        
        /// <summary>
        /// True if the key was found in the cache, otherwise False
        /// </summary>
        public bool Success { get; }

        public static implicit operator TV(GetFromCacheResult<TK, TV> result)
        {
            return result.Value;
        }
    }
}