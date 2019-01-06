using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents a standalone cache
    /// </summary>
    /// <typeparam name="TK">The type of the cache keys</typeparam>
    /// <typeparam name="TV">The type of the cache values</typeparam>
    public interface ICache<TK, TV>
    {
        /// <summary>
        /// Gets a single value from the cache
        /// </summary>
        /// <param name="key">The key to lookup in the cache</param>
        /// <returns>The cached value (default if not found)</returns>
        Task<TV> Get(TK key);
        
        /// <summary>
        /// Sets a single value in the cache
        /// </summary>
        /// <param name="key">The key at which to store the <paramref name="value"/></param>
        /// <param name="value">The value to store in the cache</param>
        /// <param name="timeToLive">The time to live for the newly stored <paramref name="key"/></param>
        Task Set(TK key, TV value, TimeSpan timeToLive);
        
        /// <summary>
        /// Gets a collection of values from the cache
        /// </summary>
        /// <param name="keys">The keys to lookup in the cache</param>
        /// <returns>The values found in the cache</returns>
        Task<IDictionary<TK, TV>> Get(ICollection<TK> keys);
        
        /// <summary>
        /// Sets a collection of values in the cache
        /// </summary>
        /// <param name="values">The collection of key/value pairs to store in the cache</param>
        /// <param name="timeToLive">The time to live for each newly stored key</param>
        Task Set(ICollection<KeyValuePair<TK, TV>> values, TimeSpan timeToLive);
    }
}