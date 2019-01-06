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
        public GetFromCacheResult(Key<TK> key, TV value, TimeSpan timeToLive, string cacheType, int statusCode = 0)
        {
            Key = key;
            Value = value;
            TimeToLive = timeToLive;
            CacheType = cacheType;
            Success = true;
            StatusCode = statusCode;
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
        
        /// <summary>
        /// If you add any custom wrappers you can use this field to mark results
        /// </summary>
        /// <remarks>
        /// 0 None
        /// 11 Duplicate
        /// </remarks>
        public int StatusCode { get; }

        /// <summary>
        /// Creates a copy of the current <see cref="GetFromCacheResult{TK,TV}"/> and assigns it the specified 
        /// <paramref name="statusCode"/> value
        /// </summary>
        /// <param name="statusCode">The status code</param>
        /// <returns></returns>
        public GetFromCacheResult<TK, TV> WithStatusCode(int statusCode)
        {
            return statusCode == StatusCode
                ? this
                : new GetFromCacheResult<TK, TV>(Key, Value, TimeToLive, CacheType, statusCode);
        }
        
        public static implicit operator TV(GetFromCacheResult<TK, TV> result)
        {
            return result.Value;
        }
    }
}