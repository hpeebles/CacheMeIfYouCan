using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents a distributed cache
    /// </summary>
    /// <typeparam name="TK">The type of the cache keys (the underlying cache may convert this to string before use)</typeparam>
    /// <typeparam name="TV">The type of the cache values</typeparam>
    public interface IDistributedCache<TK, TV> : IDisposable
    {
        /// <summary>
        /// The name used to identify this cache
        /// </summary>
        string CacheName { get; }
        
        /// <summary>
        /// The name used to identify the type of this cache
        /// </summary>
        string CacheType { get; }
        
        /// <summary>
        /// Gets a single value from the cache
        /// </summary>
        /// <param name="key">The key to lookup in the cache</param>
        /// <returns>The cached value along with meta data (if not found the Success property will be false)</returns>
        Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key);
        
        /// <summary>
        /// Sets a single value in the cache
        /// </summary>
        /// <param name="key">The key at which to store the <paramref name="value"/></param>
        /// <param name="value">The value to store in the cache</param>
        /// <param name="timeToLive">The time to live for the newly stored <paramref name="key"/></param>
        Task Set(Key<TK> key, TV value, TimeSpan timeToLive);
        
        /// <summary>
        /// Gets a collection of values from the cache
        /// </summary>
        /// <param name="keys">The keys to lookup in the cache</param>
        /// <returns>The items found in the cache along with their meta data</returns>
        /// <remarks>When implementing this, only return those items that are found in the cache, if none are found, return an empty list</remarks>
        Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys);
        
        /// <summary>
        /// Sets a collection of values in the cache
        /// </summary>
        /// <param name="values">The collection of key/value pairs to store in the cache</param>
        /// <param name="timeToLive">The time to live for each newly stored key</param>
        Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);

        /// <summary>
        /// Removes a single key from the cache
        /// </summary>
        /// <param name="key">The key to remove from the cache</param>
        /// <returns>True if the key was removed, False otherwise</returns>
        Task<bool> Remove(Key<TK> key);
    }
}