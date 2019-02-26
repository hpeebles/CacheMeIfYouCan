using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace CacheMeIfYouCan.Redis
{
    // This holds all of the keys that have been set recently by this client.
    // We can then use this to determine whether to remove keys or not on receipt of key event messages.
    internal class RecentlySetKeysManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, long> _dictionary = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentQueue<KeyValuePair<string, long>> _queue = new ConcurrentQueue<KeyValuePair<string, long>>();
        private readonly long _recentWindow = TimeSpan.FromSeconds(5).Ticks;

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
            var now = Timestamp.Now;

            if (!_dictionary.TryAdd(key, now))
                return;
            
            _queue.Enqueue(new KeyValuePair<string, long>(key, now));
        }

        public bool IsRecentlySet(string key)
        {
            // Remove keys as they are seen since if we receive the same key a second time it must have
            // been changed externally and so its value was no longer recently set by this local service
            return _dictionary.TryRemove(key, out var timestamp) && IsRecent(timestamp);
        }

        public void Dispose()
        {
            _keyRemoverProcess?.Dispose();
        }

        private void Prune()
        {
            while (_queue.Any())
            {
                if (!_queue.TryPeek(out var next))
                    return;

                if (IsRecent(next.Value))
                    break;
                
                _queue.TryDequeue(out next);
                _dictionary.TryRemove(next.Key, out _);
            }
        }

        private bool IsRecent(long timestamp)
        {
            var age = Timestamp.Now - timestamp;
            
            return age < _recentWindow;
        }
    }
}