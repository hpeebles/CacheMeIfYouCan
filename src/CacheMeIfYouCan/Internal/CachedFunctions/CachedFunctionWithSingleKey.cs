using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithSingleKey<TKey, TValue>
    {
        private readonly Func<TKey, Task<TValue>> _originalFunction;
        private readonly Func<TKey, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly ILocalCache<TKey, TValue> _cache;

        public CachedFunctionWithSingleKey(CachedFunctionConfiguration<TKey, TValue> config)
        {
            _originalFunction = config.OriginalFunction;
            _timeToLiveFactory = config.TimeToLiveFactory;
            _keyComparer = config.KeyComparer;
            _cache = config.LocalCache;
        }

        public async Task<TValue> Get(TKey key)
        {
            if (_cache.TryGet(key, out var value))
                return value;

            value = await _originalFunction(key).ConfigureAwait(false);

            var timeToLive = _timeToLiveFactory(key);
            
            if (timeToLive > TimeSpan.Zero)
                _cache.Set(key, value, timeToLive);

            return value;
        }
    }
}