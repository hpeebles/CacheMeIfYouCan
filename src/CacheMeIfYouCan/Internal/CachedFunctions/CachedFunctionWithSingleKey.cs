using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithSingleKey<TParams, TKey, TValue>
    {
        private readonly Func<TParams, CancellationToken, Task<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _keySelector;
        private readonly Func<TKey, TimeSpan> _timeToLiveFactory;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;
        private readonly ICache<TKey, TValue> _cache;

        public CachedFunctionWithSingleKey(
            Func<TParams, CancellationToken, Task<TValue>> originalFunction,
            Func<TParams, TKey> keySelector,
            CachedFunctionWithSingleKeyConfiguration<TKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;

            if (config.DisableCaching)
            {
                _timeToLiveFactory = key => TimeSpan.Zero;
                _cache = NullCache<TKey, TValue>.Instance;
            }
            else
            {
                _timeToLiveFactory = config.TimeToLiveFactory;
                
                _cache = CacheBuilder.Build(
                    config,
                    out var additionalSkipCacheGetPredicate,
                    out var additionalSkipCacheSetPredicate);
                
                _skipCacheGetPredicate = config.SkipCacheGetPredicate.Or(additionalSkipCacheGetPredicate);
                _skipCacheSetPredicate = config.SkipCacheSetPredicate.Or(additionalSkipCacheSetPredicate);
            }
        }

        public async Task<TValue> Get(TParams parameters, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = _keySelector(parameters);
            
            if (_skipCacheGetPredicate is null || !_skipCacheGetPredicate(key))
            {
                var tryGetTask = _cache.TryGet(key);

                var (success, valueFromCache) = tryGetTask.IsCompleted
                    ? tryGetTask.Result
                    : await tryGetTask.ConfigureAwait(false);

                if (success)
                    return valueFromCache;
            }

            var value = await _originalFunction(parameters, cancellationToken).ConfigureAwait(false);

            if (_skipCacheSetPredicate is null || !_skipCacheSetPredicate(key, value))
            {
                var timeToLive = _timeToLiveFactory(key);

                if (timeToLive > TimeSpan.Zero)
                {
                    var setTask = _cache.Set(key, value, timeToLive);

                    if (!setTask.IsCompleted)
                        await setTask.ConfigureAwait(false);
                }
            }

            return value;
        }
    }
}