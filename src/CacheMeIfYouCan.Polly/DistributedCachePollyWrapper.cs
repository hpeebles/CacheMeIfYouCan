using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    public sealed class DistributedCachePollyWrapper<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;
        private readonly AsyncPolicy _tryGetPolicy;
        private readonly AsyncPolicy _setPolicy;
        private readonly AsyncPolicy _getManyPolicy;
        private readonly AsyncPolicy _setManyPolicy;

        public DistributedCachePollyWrapper(IDistributedCache<TKey, TValue> innerCache, AsyncPolicy policy)
            : this(innerCache, policy, policy, policy, policy)
        { }

        public DistributedCachePollyWrapper(
            IDistributedCache<TKey, TValue> innerCache,
            AsyncPolicy tryGetPolicy = null,
            AsyncPolicy setPolicy = null,
            AsyncPolicy getManyPolicy = null,
            AsyncPolicy setManyPolicy = null)
        {
            _innerCache = innerCache;
            _tryGetPolicy = tryGetPolicy;
            _setPolicy = setPolicy;
            _getManyPolicy = getManyPolicy;
            _setManyPolicy = setManyPolicy;
        }
        
        public Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            return _tryGetPolicy is null
                ? _innerCache.TryGet(key)
                : _tryGetPolicy.ExecuteAsync(() => _innerCache.TryGet(key));
        }

        public Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            return _setPolicy is null
                ? _innerCache.Set(key, value, timeToLive)
                : _setPolicy.ExecuteAsync(() => _innerCache.Set(key, value, timeToLive));
        }

        public Task<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            return _getManyPolicy is null
                ? _innerCache.GetMany(keys, destination)
                : _getManyPolicy.ExecuteAsync(() => _innerCache.GetMany(keys, destination));
        }

        public Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            return _setManyPolicy is null
                ? _innerCache.SetMany(values, timeToLive)
                : _setManyPolicy.ExecuteAsync(() => _innerCache.SetMany(values, timeToLive));
        }
    }

    public sealed class DistributedCachePollyWrapper<TOuterKey, TInnerKey, TValue> :
        IDistributedCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private readonly AsyncPolicy _getManyPolicy;
        private readonly AsyncPolicy _setManyPolicy;

        public DistributedCachePollyWrapper(
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
            AsyncPolicy policy)
            : this(innerCache, policy, policy)
        { }

        public DistributedCachePollyWrapper(
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
            AsyncPolicy getManyPolicy = null,
            AsyncPolicy setManyPolicy = null)
        {
            _innerCache = innerCache;
            _getManyPolicy = getManyPolicy;
            _setManyPolicy = setManyPolicy;
        }

        public Task<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            return _getManyPolicy is null
                ? _innerCache.GetMany(outerKey, innerKeys, destination)
                : _getManyPolicy.ExecuteAsync(() => _innerCache.GetMany(outerKey, innerKeys, destination));
        }

        public Task SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            return _setManyPolicy is null
                ? _innerCache.SetMany(outerKey, values, timeToLive)
                : _setManyPolicy.ExecuteAsync(() => _innerCache.SetMany(outerKey, values, timeToLive));
        }
    }
}