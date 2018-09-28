using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class KeysToKeepAliveProcessor<TK> : IDisposable
    {
        private readonly TimeSpan _timeToLive;
        private readonly Func<Key<TK>, Task<TimeSpan?>> _getTimeToLiveFunc;
        private readonly Func<TK, TimeSpan?, Task> _refreshKey;
        private readonly Func<Task<IList<TK>>> _keysToKeepAliveFunc;
        private readonly Func<TK, string> _keySerializer;
        private readonly IEqualityComparer<Key<TK>> _keyComparer;
        private readonly SortedDictionary<DateTime, List<Key<TK>>> _keyExpiryDates;
        private readonly AutoResetEvent _nextKeyToExpireChanged;
        private readonly CancellationTokenSource _cts;
        private readonly object _lock;
        private readonly TimeSpan _targetRefreshTimeToLive;

        public KeysToKeepAliveProcessor(
            TimeSpan timeToLive,
            Func<Key<TK>, Task<TimeSpan?>> getTimeToLive,
            Func<TK, TimeSpan?, Task> refreshKey,
            Func<Task<IList<TK>>> keysToKeepAliveFunc,
            Func<TK, string> keySerializer,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            _timeToLive = timeToLive;
            _getTimeToLiveFunc = getTimeToLive;
            _refreshKey = refreshKey;
            _keysToKeepAliveFunc = keysToKeepAliveFunc;
            _keySerializer = keySerializer;
            _keyComparer = keyComparer;
            _keyExpiryDates = new SortedDictionary<DateTime, List<Key<TK>>>();
            _nextKeyToExpireChanged = new AutoResetEvent(false);
            _cts = new CancellationTokenSource();
            _lock = new object();
            
            // Aim to refresh keys when there is 10% of their lifetime remaining
            _targetRefreshTimeToLive = TimeSpan.FromMilliseconds(_timeToLive.TotalMilliseconds * 0.1);
        }

        public async Task Run()
        {
            if (_keysToKeepAliveFunc == null)
                return;

            Task.Run(RunKeyRefreshTask);
            
            while (!_cts.IsCancellationRequested)
            {
                var nextLoopCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_timeToLive.TotalMilliseconds * 0.9));

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, nextLoopCts.Token))
                {
                    // This token will cancel when either its time to start the next loop or the process is cancelled
                    var token = cts.Token;
                    try
                    {
                        await SetExpiryDates(token);
                    }
                    catch
                    { }

                    token.WaitHandle.WaitOne();
                }
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async Task RunKeyRefreshTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    KeyValuePair<DateTime, List<Key<TK>>>? nextKeysToExpire;
                    lock (_lock)
                    {
                        if (_keyExpiryDates.Any())
                            nextKeysToExpire = _keyExpiryDates.First();
                        else
                            nextKeysToExpire = null;
                    }
    
                    var waitInterval = nextKeysToExpire.HasValue
                        ? nextKeysToExpire.Value.Key - DateTime.UtcNow - _targetRefreshTimeToLive
                        : _timeToLive - _targetRefreshTimeToLive;
    
                    // If waitInterval < zero then the keys are due a refresh
                    if (nextKeysToExpire.HasValue && waitInterval < TimeSpan.Zero)
                    {
                        var expiry = nextKeysToExpire.Value.Key;
                        var keys = nextKeysToExpire.Value.Value;
                        
                        TimeSpan? timeToLiveRemaining = expiry - DateTime.UtcNow;
                        if (timeToLiveRemaining < TimeSpan.Zero)
                            timeToLiveRemaining = null;
                        
                        foreach (var key in keys)
                        {
                            try
                            {
                                await _refreshKey(key.AsObject, timeToLiveRemaining);
                            }
                            catch
                            { }

                            var newExpiry = DateTime.UtcNow + _timeToLive;
                            
                            lock (_lock)
                            {
                                if (_keyExpiryDates.TryGetValue(newExpiry, out var existing))
                                    existing.Add(key);
                                else
                                    _keyExpiryDates.Add(newExpiry, new List<Key<TK>> { key });
                            }
                        }

                        lock (_lock)
                            _keyExpiryDates.Remove(expiry);

                        // If we've just removed a key, continue without waiting as there may still be more that are ready to be refreshed
                        continue;
                    }
    
                    // Wait for the next keys to be due a refresh or for a key which expires sooner to be added
                    WaitHandle.WaitAny(new[] { _nextKeyToExpireChanged, _cts.Token.WaitHandle }, waitInterval);
                }
                catch (Exception ex)
                { }
            }
        }

        private async Task SetExpiryDates(CancellationToken token)
        {
            var keyObjects = await _keysToKeepAliveFunc();

            var keysSet = new HashSet<Key<TK>>(_keyComparer);
            
            var keys = keyObjects
                .Select(k => new Key<TK>(k, new Lazy<string>(() => _keySerializer(k))))
                .Where(keysSet.Add)
                .ToList();

            // Remove all keys that are no longer in the set of keys to keep alive
            lock (_lock)
            {
                var expiryDatesToRemove = new List<DateTime>();
                foreach (var kv in _keyExpiryDates)
                {
                    if (kv.Value.RemoveAll(k => !keysSet.Contains(k)) > 0 && !kv.Value.Any())
                        expiryDatesToRemove.Add(kv.Key);
                }

                foreach (var expiryDate in expiryDatesToRemove)
                    _keyExpiryDates.Remove(expiryDate);
            }

            foreach (var key in keys)
            {
                if (token.IsCancellationRequested)
                    return;
                
                var keyStart = Stopwatch.GetTimestamp();

                try
                {
                    var timeToLive = await _getTimeToLiveFunc(key);

                    if (!timeToLive.HasValue)
                    {
                        await _refreshKey(key.AsObject, null);
                        timeToLive = _timeToLive;
                    }

                    var expiry = DateTime.UtcNow + timeToLive.Value;
                    
                    lock (_lock)
                    {
                        if (_keyExpiryDates.TryGetValue(expiry, out var existing))
                        {
                            existing.Add(key);
                        }
                        else
                        {
                            _keyExpiryDates.Add(expiry, new List<Key<TK>> { key });
                            
                            // If newly added key is next to expire, signal the wait handle
                            if (_keyExpiryDates.First().Key.Equals(expiry))
                                _nextKeyToExpireChanged.Set();
                        }
                    }
                }
                catch
                { }

                var keyDuration = Stopwatch.GetTimestamp() - keyStart;

                // So that we don't hammer the cache
                await Wait(TimeSpan.FromTicks(keyDuration), token);
            }
        }

        private static async Task<bool> Wait(TimeSpan interval, CancellationToken token)
        {
            try
            {
                await Task.Delay(interval, token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}