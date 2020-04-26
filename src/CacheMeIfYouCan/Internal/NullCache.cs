using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class NullCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private NullCache() { }
        
        public static readonly NullCache<TKey, TValue> Instance = new NullCache<TKey, TValue>();
        
        public bool LocalCacheEnabled { get; } = false;
        public bool DistributedCacheEnabled { get; } = false;
        public ValueTask<(bool Success, TValue Value, CacheGetStats Stats)> TryGet(TKey key) => default;
        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive) => default;
        public ValueTask<CacheGetManyStats> GetMany(IReadOnlyCollection<TKey> keys, int cacheKeysSkipped, Memory<KeyValuePair<TKey, TValue>> destination) => default;
        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive) => default;
    }
    
    internal sealed class NullCache<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private NullCache() { }
        
        public static readonly NullCache<TOuterKey, TInnerKey, TValue> Instance = new NullCache<TOuterKey, TInnerKey, TValue>();
        
        public bool LocalCacheEnabled => false;
        public bool DistributedCacheEnabled => false;
        
        public ValueTask<CacheGetManyStats> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            return default;
        }

        public ValueTask SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            return default;
        }
    }
}