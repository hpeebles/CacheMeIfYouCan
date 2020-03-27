using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CacheMeIfYouCan
{
    public sealed class LocalCacheEventsWrapper<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private readonly Action<TKey, bool, TValue, TimeSpan> _onTryGetCompletedSuccessfully;
        private readonly Func<TKey, TimeSpan, Exception, bool> _onTryGetException;
        private readonly Action<TKey, TValue, TimeSpan, TimeSpan> _onSetCompletedSuccessfully;
        private readonly Func<TKey, TValue, TimeSpan, TimeSpan, Exception, bool> _onSetException;
        private readonly Action<IReadOnlyCollection<TKey>, ReadOnlyMemory<KeyValuePair<TKey, TValue>>, TimeSpan> _onGetManyCompletedSuccessfully;
        private readonly Func<IReadOnlyCollection<TKey>, TimeSpan, Exception, bool> _onGetManyException;
        private readonly Action<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan> _onSetManyCompletedSuccessfully;
        private readonly Func<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> _onSetManyException;
        private readonly Action<TKey, bool, TValue, TimeSpan> _onTryRemoveCompletedSuccessfully;
        private readonly Func<TKey, TimeSpan, Exception, bool> _onTryRemoveException;
        private readonly ILocalCache<TKey, TValue> _innerCache;

        public LocalCacheEventsWrapper(
            LocalCacheEventsWrapperConfig<TKey, TValue> config,
            ILocalCache<TKey, TValue> innerCache)
        {
            _onTryGetCompletedSuccessfully = config.OnTryGetCompletedSuccessfully;
            _onTryGetException = config.OnTryGetException;
            _onSetCompletedSuccessfully = config.OnSetCompletedSuccessfully;
            _onSetException = config.OnSetException;
            _onGetManyCompletedSuccessfully = config.OnGetManyCompletedSuccessfully;
            _onGetManyException = config.OnGetManyException;
            _onSetManyCompletedSuccessfully = config.OnSetManyCompletedSuccessfully;
            _onSetManyException = config.OnSetManyException;
            _onTryRemoveCompletedSuccessfully = config.OnTryRemoveCompletedSuccessfully;
            _onTryRemoveException = config.OnTryRemoveException;
            _innerCache = innerCache;
        }
        
        public bool TryGet(TKey key, out TValue value)
        {
            if (_onTryGetException is null)
            {
                if (_onTryGetCompletedSuccessfully is null)
                    return _innerCache.TryGet(key, out value);

                var stopwatch = Stopwatch.StartNew();

                var found = _innerCache.TryGet(key, out value);

                _onTryGetCompletedSuccessfully(key, found, value, stopwatch.Elapsed);

                return found;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var found = _innerCache.TryGet(key, out value);

                    _onTryGetCompletedSuccessfully?.Invoke(key, found, value, stopwatch.Elapsed);

                    return found;
                }
                catch (Exception ex)
                {
                    if (!_onTryGetException(key, stopwatch.Elapsed, ex))
                        throw;

                    value = default;
                    return false;
                }
            }
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            if (_onSetException is null)
            {
                if (_onSetCompletedSuccessfully is null)
                {
                    _innerCache.Set(key, value, timeToLive);
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                _innerCache.Set(key, value, timeToLive);

                _onSetCompletedSuccessfully(key, value, timeToLive, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    _innerCache.Set(key, value, timeToLive);

                    _onSetCompletedSuccessfully?.Invoke(key, value, timeToLive, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetException(key, value, timeToLive, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }

        public int GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            if (_onGetManyException is null)
            {
                if (_onGetManyCompletedSuccessfully is null)
                    return _innerCache.GetMany(keys, destination);

                var stopwatch = Stopwatch.StartNew();

                var countFound = _innerCache.GetMany(keys, destination);

                var values = destination.Slice(0, countFound);
                
                _onGetManyCompletedSuccessfully(keys, values, stopwatch.Elapsed);

                return countFound;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var countFound = _innerCache.GetMany(keys, destination);
                    
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

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_onSetManyException is null)
            {
                if (_onSetManyCompletedSuccessfully is null)
                {
                    _innerCache.SetMany(values, timeToLive);
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                _innerCache.SetMany(values, timeToLive);

                _onSetManyCompletedSuccessfully(values, timeToLive, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    _innerCache.SetMany(values, timeToLive);
                    
                    _onSetManyCompletedSuccessfully?.Invoke(values, timeToLive, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetManyException(values, timeToLive, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (_onTryRemoveException is null)
            {
                if (_onTryRemoveCompletedSuccessfully is null)
                    return _innerCache.TryRemove(key, out value);

                var stopwatch = Stopwatch.StartNew();

                var removed = _innerCache.TryRemove(key, out value);

                _onTryRemoveCompletedSuccessfully(key, removed, value, stopwatch.Elapsed);

                return removed;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var removed = _innerCache.TryRemove(key, out value);

                    _onTryRemoveCompletedSuccessfully?.Invoke(key, removed, value, stopwatch.Elapsed);

                    return removed;
                }
                catch (Exception ex)
                {
                    if (!_onTryRemoveException(key, stopwatch.Elapsed, ex))
                        throw;

                    value = default;
                    return false;
                }
            }
        }
    }
    
    public sealed class LocalCacheEventsWrapper<TOuterKey, TInnerKey, TValue> : ILocalCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly Action<TOuterKey, IReadOnlyCollection<TInnerKey>, ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>>, TimeSpan> _onGetManyCompletedSuccessfully;
        private readonly Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan, Exception, bool> _onGetManyException;
        private readonly Action<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan> _onSetManyCompletedSuccessfully;
        private readonly Func<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> _onSetManyException;
        private readonly Action<TOuterKey, ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>, TimeSpan> _onSetManyWithVaryingTimesToLiveCompletedSuccessfully;
        private readonly Func<TOuterKey, ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>, TimeSpan, Exception, bool> _onSetManyWithVaryingTimesToLiveException;
        private readonly Action<TOuterKey, TInnerKey, bool, TValue, TimeSpan> _onTryRemoveCompletedSuccessfully;
        private readonly Func<TOuterKey, TInnerKey, TimeSpan, Exception, bool> _onTryRemoveException;
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache;

        public LocalCacheEventsWrapper(
            LocalCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue> config,
            ILocalCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _onGetManyCompletedSuccessfully = config.OnGetManyCompletedSuccessfully;
            _onGetManyException = config.OnGetManyException;
            _onSetManyCompletedSuccessfully = config.OnSetManyCompletedSuccessfully;
            _onSetManyException = config.OnSetManyException;
            _onSetManyWithVaryingTimesToLiveCompletedSuccessfully = config.OnSetManyWithVaryingTimesToLiveCompletedSuccessfully;
            _onSetManyWithVaryingTimesToLiveException = config.OnSetManyWithVaryingTimesToLiveException;
            _onTryRemoveCompletedSuccessfully = config.OnTryRemoveCompletedSuccessfully;
            _onTryRemoveException = config.OnTryRemoveException;
            _innerCache = innerCache;
        }

        public int GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            if (_onGetManyException is null)
            {
                if (_onGetManyCompletedSuccessfully is null)
                    return _innerCache.GetMany(outerKey, innerKeys, destination);

                var stopwatch = Stopwatch.StartNew();

                var countFound = _innerCache.GetMany(outerKey, innerKeys, destination);
                
                var values = destination.Slice(0, countFound);

                _onGetManyCompletedSuccessfully(outerKey, innerKeys, values, stopwatch.Elapsed);

                return countFound;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var countFound = _innerCache.GetMany(outerKey, innerKeys, destination);

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

        public void SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (_onSetManyException is null)
            {
                if (_onSetManyCompletedSuccessfully is null)
                {
                    _innerCache.SetMany(outerKey, values, timeToLive);
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                _innerCache.SetMany(outerKey, values, timeToLive);

                _onSetManyCompletedSuccessfully(outerKey, values, timeToLive, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    _innerCache.SetMany(outerKey, values, timeToLive);

                    _onSetManyCompletedSuccessfully?.Invoke(outerKey, values, timeToLive, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetManyException(outerKey, values, timeToLive, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }

        public void SetManyWithVaryingTimesToLive(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            if (_onSetManyWithVaryingTimesToLiveException is null)
            {
                if (_onSetManyWithVaryingTimesToLiveCompletedSuccessfully is null)
                {
                    _innerCache.SetManyWithVaryingTimesToLive(outerKey, values);
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                
                _innerCache.SetManyWithVaryingTimesToLive(outerKey, values);

                _onSetManyWithVaryingTimesToLiveCompletedSuccessfully(outerKey, values, stopwatch.Elapsed);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    _innerCache.SetManyWithVaryingTimesToLive(outerKey, values);

                    _onSetManyWithVaryingTimesToLiveCompletedSuccessfully?.Invoke(outerKey, values, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    if (!_onSetManyWithVaryingTimesToLiveException(outerKey, values, stopwatch.Elapsed, ex))
                        throw;
                }
            }
        }

        public bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value)
        {
            if (_onTryRemoveException is null)
            {
                if (_onTryRemoveCompletedSuccessfully is null)
                    return _innerCache.TryRemove(outerKey, innerKey, out value);

                var stopwatch = Stopwatch.StartNew();

                var removed = _innerCache.TryRemove(outerKey, innerKey, out value);

                _onTryRemoveCompletedSuccessfully(outerKey, innerKey, removed, value, stopwatch.Elapsed);

                return removed;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var removed = _innerCache.TryRemove(outerKey, innerKey, out value);

                    _onTryRemoveCompletedSuccessfully?.Invoke(outerKey, innerKey, removed, value, stopwatch.Elapsed);

                    return removed;
                }
                catch (Exception ex)
                {
                    if (!_onTryRemoveException(outerKey, innerKey, stopwatch.Elapsed, ex))
                        throw;

                    value = default;
                    return false;
                }
            }
        }
    }
}