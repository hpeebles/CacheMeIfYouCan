using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Redis
{
    // This holds all of the keys that have been set recently by this client.
    // We can then use this to determine whether to remove keys or not on receipt on key event messages.
    internal class RecentlySetKeysManager
    {
        private readonly ConcurrentDictionary<string, DateTime> _dictionary = new ConcurrentDictionary<string, DateTime>();
        private readonly Queue<KeyValuePair<string, DateTime>> _queue = new Queue<KeyValuePair<string, DateTime>>();
        private readonly TimeSpan _recentWindow = TimeSpan.FromSeconds(5);
        private readonly object _lock = new object();
        private DateTime _lastPruned;

        public void Mark(string key)
        {
            var now = DateTime.UtcNow;

            if (!_dictionary.TryAdd(key, now))
                return;
            
            lock (_lock)
                _queue.Enqueue(new KeyValuePair<string, DateTime>(key, now));
            
            if (DateTime.UtcNow - _lastPruned > TimeSpan.FromMinutes(1)) // to stop the collections from growing infinitely
                Prune();
        }

        public bool IsRecentlySet(string key)
        {
            Prune();

            return _dictionary.ContainsKey(key);
        }

        private void Prune()
        {
            lock (_lock)
            {
                while (_queue.Count > 0)
                {
                    var next = _queue.Peek();

                    if (IsRecent(next.Value))
                        break;
                    
                    _queue.Dequeue();
                    _dictionary.TryRemove(next.Key, out _);
                }
            }

            _lastPruned = DateTime.UtcNow;
        }

        private bool IsRecent(DateTime date)
        {
            return DateTime.UtcNow - date < _recentWindow;
        }
    }
}