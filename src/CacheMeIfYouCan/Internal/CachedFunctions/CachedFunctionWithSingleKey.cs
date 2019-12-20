﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithSingleKey<TKey, TValue>
    {
        private readonly Func<TKey, CancellationToken, Task<TValue>> _originalFunction;
        private readonly Func<TKey, TimeSpan> _timeToLiveFactory;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;
        private readonly ICache<TKey, TValue> _cache;

        public CachedFunctionWithSingleKey(CachedFunctionConfiguration<TKey, TValue> config)
        {
            _originalFunction = config.OriginalFunction;

            if (config.DisableCaching)
            {
                _timeToLiveFactory = key => TimeSpan.Zero;
                _cache = NullCache<TKey, TValue>.Instance;
            }
            else
            {
                _timeToLiveFactory = config.TimeToLiveFactory;
                _skipCacheGetPredicate = config.SkipCacheGetPredicate;
                _skipCacheSetPredicate = config.SkipCacheSetPredicate;
                _cache = CacheBuilder.Build(config);
            }
        }

        public async Task<TValue> Get(TKey key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (_skipCacheGetPredicate is null || !_skipCacheGetPredicate(key))
            {
                var tryGetTask = _cache.TryGet(key);

                if (!tryGetTask.IsCompleted)
                    await tryGetTask.ConfigureAwait(false);

                if (tryGetTask.Result.Success)
                    return tryGetTask.Result.Value;
            }

            var value = await _originalFunction(key, cancellationToken).ConfigureAwait(false);

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