using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    public sealed class DistributedCachePollyWrapper<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;
        private readonly IAsyncPolicy _tryGetPolicy;
        private readonly IAsyncPolicy _setPolicy;
        private readonly IAsyncPolicy _getManyPolicy;
        private readonly IAsyncPolicy _setManyPolicy;
        private readonly IAsyncPolicy _tryRemovePolicy;

        public DistributedCachePollyWrapper(IDistributedCache<TKey, TValue> innerCache, IAsyncPolicy policy)
            : this(innerCache, policy, policy, policy, policy, policy)
        { }

        public DistributedCachePollyWrapper(
            IDistributedCache<TKey, TValue> innerCache,
            IAsyncPolicy tryGetPolicy = null,
            IAsyncPolicy setPolicy = null,
            IAsyncPolicy getManyPolicy = null,
            IAsyncPolicy setManyPolicy = null,
            IAsyncPolicy tryRemovePolicy = null)
        {
            _innerCache = innerCache;
            _tryGetPolicy = tryGetPolicy;
            _setPolicy = setPolicy;
            _getManyPolicy = getManyPolicy;
            _setManyPolicy = setManyPolicy;
            _tryRemovePolicy = tryRemovePolicy;
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

        public Task<int> GetMany(ReadOnlyMemory<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            return _getManyPolicy is null
                ? _innerCache.GetMany(keys, destination)
                : _getManyPolicy.ExecuteAsync(() => _innerCache.GetMany(keys, destination));
        }

        public Task SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            return _setManyPolicy is null
                ? _innerCache.SetMany(values, timeToLive)
                : _setManyPolicy.ExecuteAsync(() => _innerCache.SetMany(values, timeToLive));
        }

        public Task<bool> TryRemove(TKey key)
        {
            return _tryRemovePolicy is null
                ? _innerCache.TryRemove(key)
                : _tryRemovePolicy.ExecuteAsync(() => _innerCache.TryRemove(key));
        }
    }

    public sealed class DistributedCachePollyWrapper<TOuterKey, TInnerKey, TValue> :
        IDistributedCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private readonly AsyncPolicy _getManyPolicy;
        private readonly AsyncPolicy _setManyPolicy;
        private readonly AsyncPolicy _tryRemovePolicy;

        public DistributedCachePollyWrapper(
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
            AsyncPolicy policy)
            : this(innerCache, policy, policy, policy)
        { }

        public DistributedCachePollyWrapper(
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
            AsyncPolicy getManyPolicy = null,
            AsyncPolicy setManyPolicy = null,
            AsyncPolicy tryRemovePolicy = null)
        {
            _innerCache = innerCache;
            _getManyPolicy = getManyPolicy;
            _setManyPolicy = setManyPolicy;
            _tryRemovePolicy = tryRemovePolicy;
        }

        public Task<int> GetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            return _getManyPolicy is null
                ? _innerCache.GetMany(outerKey, innerKeys, destination)
                : _getManyPolicy.ExecuteAsync(() => _innerCache.GetMany(outerKey, innerKeys, destination));
        }

        public Task SetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            return _setManyPolicy is null
                ? _innerCache.SetMany(outerKey, values, timeToLive)
                : _setManyPolicy.ExecuteAsync(() => _innerCache.SetMany(outerKey, values, timeToLive));
        }

        public Task<bool> TryRemove(TOuterKey outerKey, TInnerKey innerKey)
        {
            return _tryRemovePolicy is null
                ? _innerCache.TryRemove(outerKey, innerKey)
                : _tryRemovePolicy.ExecuteAsync(() => _innerCache.TryRemove(outerKey, innerKey));
        }
    }
}