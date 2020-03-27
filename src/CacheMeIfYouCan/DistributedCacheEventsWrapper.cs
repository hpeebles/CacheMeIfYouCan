using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public sealed class DistributedCacheEventsWrapper<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly Action<TKey, bool, TValue, TimeSpan> _onTryGetCompletedSuccessfully;
        private readonly Func<TKey, TimeSpan, Exception, bool> _onTryGetException;
        private readonly Action<TKey, TValue, TimeSpan, TimeSpan> _onSetCompletedSuccessfully;
        private readonly Func<TKey, TValue, TimeSpan, TimeSpan, Exception, bool> _onSetException;
        private readonly Action<IReadOnlyCollection<TKey>, ReadOnlyMemory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>, TimeSpan> _onGetManyCompletedSuccessfully;
        private readonly Func<IReadOnlyCollection<TKey>, TimeSpan, Exception, bool> _onGetManyException;
        private readonly Action<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan> _onSetManyCompletedSuccessfully;
        private readonly Func<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> _onSetManyException;
        private readonly IDistributedCache<TKey, TValue> _innerCache;

        public DistributedCacheEventsWrapper(
            DistributedCacheEventsWrapperConfig<TKey, TValue> config,
            IDistributedCache<TKey, TValue> innerCache)
        {
            _onTryGetCompletedSuccessfully = config.OnTryGetCompletedSuccessfully;
            _onTryGetException = config.OnTryGetException;
            _onSetCompletedSuccessfully = config.OnSetCompletedSuccessfully;
            _onSetException = config.OnSetException;
            _onGetManyCompletedSuccessfully = config.OnGetManyCompletedSuccessfully;
            _onGetManyException = config.OnGetManyException;
            _onSetManyCompletedSuccessfully = config.OnSetManyCompletedSuccessfully;
            _onSetManyException = config.OnSetManyException;
            _innerCache = innerCache;
        }
        
        public async Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            if (_onTryGetException is null)
            {
                if (_onTryGetCompletedSuccessfully is null)
                {
                    return await _innerCache
                        .TryGet(key)
                        .ConfigureAwait(false);
                }

                var stopwatch = Stopwatch.StartNew();

                var result = await _innerCache
                    .TryGet(key)
                    .ConfigureAwait(false);

                _onTryGetCompletedSuccessfully(key, result.Success, result.Value, stopwatch.Elapsed);

                return result;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var result = await _innerCache
                        .TryGet(key)
                        .ConfigureAwait(false);

                    _onTryGetCompletedSuccessfully?.Invoke(key, result.Success, result.Value, stopwatch.Elapsed);

                    return result;
                }
                catch (Exception ex)
                {
                    if (!_onTryGetException(key, stopwatch.Elapsed, ex))
                        throw;

                    return default;
                }
            }
        }

        public async Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            if (_onSetException is null)
            {
                if (_onSetCompletedSuccessfully is null)
                {
                    await _innerCache
                        .Set(key, value, timeToLive)
                        .ConfigureAwait(false);
                    
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                await _innerCache
                    .Set(key, value, timeToLive)
                    .ConfigureAwait(false);

                _onSetCompletedSuccessfully(key, value, timeToLive, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await _innerCache
                        .Set(key, value, timeToLive)
                        .ConfigureAwait(false);

                    _onSetCompletedSuccessfully?.Invoke(key, value, timeToLive, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetException(key, value, timeToLive, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }

        public async Task<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            if (_onGetManyException is null)
            {
                if (_onGetManyCompletedSuccessfully is null)
                {
                    return await _innerCache
                        .GetMany(keys, destination)
                        .ConfigureAwait(false);
                }

                var stopwatch = Stopwatch.StartNew();

                var countFound = await _innerCache
                    .GetMany(keys, destination)
                    .ConfigureAwait(false);

                var values = destination.Slice(0, countFound);
                
                _onGetManyCompletedSuccessfully(keys, values, stopwatch.Elapsed);

                return countFound;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var countFound = await _innerCache
                        .GetMany(keys, destination)
                        .ConfigureAwait(false);
                    
                    var values = destination.Slice(0, countFound);
                    
                    _onGetManyCompletedSuccessfully?.Invoke(keys, values, stopwatch.Elapsed);

                    return countFound;
                }
                catch (Exception ex)
                {
                    if (!_onGetManyException(keys, stopwatch.Elapsed, ex))
                        throw;

                    return 0;
                }
            }
        }

        public async Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_onSetManyException is null)
            {
                if (_onSetManyCompletedSuccessfully is null)
                {
                    await _innerCache
                        .SetMany(values, timeToLive)
                        .ConfigureAwait(false);
                    
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                await _innerCache
                    .SetMany(values, timeToLive)
                    .ConfigureAwait(false);

                _onSetManyCompletedSuccessfully(values, timeToLive, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await _innerCache
                        .SetMany(values, timeToLive)
                        .ConfigureAwait(false);
                    
                    _onSetManyCompletedSuccessfully?.Invoke(values, timeToLive, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetManyException(values, timeToLive, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }
    }
    
    public sealed class DistributedCacheEventsWrapper<TOuterKey, TInnerKey, TValue> : IDistributedCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly Action<TOuterKey, IReadOnlyCollection<TInnerKey>, ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>, TimeSpan> _onGetManyCompletedSuccessfully;
        private readonly Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan, Exception, bool> _onGetManyException;
        private readonly Action<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan> _onSetManyCompletedSuccessfully;
        private readonly Func<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> _onSetManyException;
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;

        public DistributedCacheEventsWrapper(
            DistributedCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue> config,
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _onGetManyCompletedSuccessfully = config.OnGetManyCompletedSuccessfully;
            _onGetManyException = config.OnGetManyException;
            _onSetManyCompletedSuccessfully = config.OnSetManyCompletedSuccessfully;
            _onSetManyException = config.OnSetManyException;
            _innerCache = innerCache;
        }

        public async Task<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            if (_onGetManyException is null)
            {
                if (_onGetManyCompletedSuccessfully is null)
                {
                    return await _innerCache
                        .GetMany(outerKey, innerKeys, destination)
                        .ConfigureAwait(false);
                }

                var stopwatch = Stopwatch.StartNew();

                var countFound = await _innerCache
                    .GetMany(outerKey, innerKeys, destination)
                    .ConfigureAwait(false);
                
                var values = destination.Slice(0, countFound);

                _onGetManyCompletedSuccessfully(outerKey, innerKeys, values, stopwatch.Elapsed);

                return countFound;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var countFound = await _innerCache
                        .GetMany(outerKey, innerKeys, destination)
                        .ConfigureAwait(false);

                    var values = destination.Slice(0, countFound);
                    
                    _onGetManyCompletedSuccessfully?.Invoke(outerKey, innerKeys, values, stopwatch.Elapsed);

                    return countFound;
                }
                catch (Exception ex)
                {
                    if (!_onGetManyException(outerKey, innerKeys, stopwatch.Elapsed, ex))
                        throw;

                    return 0;
                }
            }
        }

        public async Task SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (_onSetManyException is null)
            {
                if (_onSetManyCompletedSuccessfully is null)
                {
                    await _innerCache
                        .SetMany(outerKey, values, timeToLive)
                        .ConfigureAwait(false);
                    
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                await _innerCache
                    .SetMany(outerKey, values, timeToLive)
                    .ConfigureAwait(false);

                _onSetManyCompletedSuccessfully(outerKey, values, timeToLive, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await _innerCache
                        .SetMany(outerKey, values, timeToLive)
                        .ConfigureAwait(false);

                    _onSetManyCompletedSuccessfully?.Invoke(outerKey, values, timeToLive, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetManyException(outerKey, values, timeToLive, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }
    }
}