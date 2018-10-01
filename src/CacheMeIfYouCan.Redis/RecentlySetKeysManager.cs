using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace CacheMeIfYouCan.Redis
{
    // This holds all of the keys that have been set recently by this client.
    // We can then use this to determine whether to remove keys or not on receipt of key event messages.
    internal class RecentlySetKeysManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, long> _dictionary = new ConcurrentDictionary<string, long>();
        private readonly Queue<KeyValuePair<string, long>> _queue = new Queue<KeyValuePair<string, long>>();
        private readonly long _recentWindow = Stopwatch.Frequency * 5; // 5 seconds
        private readonly object _lock = new object();

        // Every second we prune keys which are no longer relevant, disposing of this field is the only way to stop that process
        private readonly IDisposable _keyRemoverProcess;

        public RecentlySetKeysManager()
        {
            _keyRemoverProcess = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ => Prune());
        }

        public void Mark(string key)
        {
            var now = Stopwatch.GetTimestamp();

            if (!_dictionary.TryAdd(key, now))
                return;
            
            lock (_lock)
                _queue.Enqueue(new KeyValuePair<string, long>(key, now));
        }

        public bool IsRecentlySet(string key)
        {
            // Remove keys as they are seen since if we recieve the same key a second time it must have
            // been changed externally and so its value was no longer recently set by this local service
            return _dictionary.TryRemove(key, out var timestamp) && IsRecent(timestamp);
        }

        public void Dispose()
        {
            _keyRemoverProcess?.Dispose();
        }

        private void Prune()
        {
            lock (_lock)
            {
                while (_queue.Any())
                {
                    var next = _queue.Peek();

                    if (IsRecent(next.Value))
                        break;
                    
                    _queue.Dequeue();
                    _dictionary.TryRemove(next.Key, out _);
                }
            }
        }

        private bool IsRecent(long timestamp)
        {
            var age = Stopwatch.GetTimestamp() - timestamp;
            
            return age < _recentWindow;
        }
    }
}