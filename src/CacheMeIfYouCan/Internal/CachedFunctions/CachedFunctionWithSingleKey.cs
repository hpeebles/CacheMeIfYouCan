using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithSingleKey<TParams, TKey, TValue>
    {
        private readonly Func<TParams, CancellationToken, ValueTask<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _keySelector;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TParams, TimeSpan> _timeToLiveFactory;
        private readonly Func<TParams, bool> _skipCacheGetPredicate;
        private readonly Func<TParams, TValue, bool> _skipCacheSetPredicate;
        private readonly ICache<TKey, TValue> _cache;
        private readonly Action<SuccessfulRequestEvent<TParams, TKey, TValue>> _onSuccessAction;
        private readonly Action<ExceptionEvent<TParams, TKey>> _onExceptionAction;
        private readonly CacheGetStats _cacheStatsIfSkipped;
        private readonly bool _cacheEnabled;
        private readonly bool _measurementsEnabled;

        public CachedFunctionWithSingleKey(
            Func<TParams, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParams, TKey> keySelector,
            CachedFunctionWithSingleKeyConfiguration<TParams, TKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
            _onSuccessAction = config.OnSuccessAction;
            _onExceptionAction = config.OnExceptionAction;

            if (config.DisableCaching)
            {
                _cache = NullCache<TKey, TValue>.Instance;
            }
            else
            {
                if (config.TimeToLive.HasValue)
                    _timeToLive = config.TimeToLive.Value;
                else
                    _timeToLiveFactory = config.TimeToLiveFactory;
                
                _cache = CacheBuilder.Build(config);
                _skipCacheGetPredicate = config.SkipCacheGetPredicate;
                _skipCacheSetPredicate = config.SkipCacheSetPredicate;
            }

            _cacheStatsIfSkipped = GetCacheStatsIfSkipped();
            _cacheEnabled = _cache.LocalCacheEnabled || _cache.DistributedCacheEnabled;
            _measurementsEnabled = _onSuccessAction != null || _onExceptionAction != null;
        }

        public async ValueTask<TValue> Get(TParams parameters, CancellationToken cancellationToken)
        {
            var (start, timer) = GetDateAndTimer();
            TKey key = default;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                key = _keySelector(parameters);

                CacheGetStats cacheStats;
                if (_cacheEnabled && (_skipCacheGetPredicate is null || !_skipCacheGetPredicate(parameters)))
                {
                    var tryGetTask = _cache.TryGet(key);

                    var (success, valueFromCache, cacheStatsInner) = tryGetTask.IsCompleted
                        ? tryGetTask.Result
                        : await tryGetTask.ConfigureAwait(false);

                    cacheStats = cacheStatsInner;
                    
                    if (success)
                    {
                        _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TKey, TValue>(
                            parameters,
                            key,
                            valueFromCache,
                            start,
                            timer.Elapsed,
                            cacheStats));

                        return valueFromCache;
                    }
                }
                else
                {
                    cacheStats = _cacheStatsIfSkipped;
                }

                var getFromFuncTask = _originalFunction(parameters, cancellationToken);

                var value = getFromFuncTask.IsCompleted
                    ? getFromFuncTask.Result
                    : await getFromFuncTask.ConfigureAwait(false);

                if (_cacheEnabled && (_skipCacheSetPredicate is null || !_skipCacheSetPredicate(parameters, value)))
                {
                    var timeToLive = _timeToLiveFactory?.Invoke(parameters) ?? _timeToLive;

                    if (timeToLive > TimeSpan.Zero)
                    {
                        var setTask = _cache.Set(key, value, timeToLive);

                        if (!setTask.IsCompleted)
                            await setTask.ConfigureAwait(false);
                    }
                }
                
                _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TKey, TValue>(
                    parameters,
                    key,
                    value,
                    start,
                    timer.Elapsed,
                    cacheStats));

                return value;
            }
            catch (Exception ex) when (!(_onExceptionAction is null))
            {
                _onExceptionAction(new ExceptionEvent<TParams, TKey>(
                    parameters,
                    key,
                    start,
                    timer.Elapsed,
                    ex));

                throw;
            }
        }

        private CacheGetStats GetCacheStatsIfSkipped()
        {
            CacheGetFlags flags = default;
            if (_cache.LocalCacheEnabled)
                flags |= CacheGetFlags.LocalCache_Enabled | CacheGetFlags.LocalCache_Skipped;

            if (_cache.DistributedCacheEnabled)
                flags |= CacheGetFlags.DistributedCache_Enabled | CacheGetFlags.DistributedCache_Skipped;

            return flags.ToStats();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (DateTime Start, StopwatchStruct Timer) GetDateAndTimer()
        {
            return _measurementsEnabled
                ? (DateTime.UtcNow, StopwatchStruct.StartNew())
                : (default, default);
        }
    }
}