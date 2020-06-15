using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Redis.Internal
{
    // This holds all of the keys that have been set or removed recently by this client.
    // We can then use this to determine whether to remove keys or not on receipt of key event messages.
    internal sealed class RecentlySetOrRemovedKeysManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, long> _dictionary = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentQueue<KeyValuePair<string, long>> _queue = new ConcurrentQueue<KeyValuePair<string, long>>();
        private readonly long _recentWindow = (long)TimeSpan.FromSeconds(5).TotalMilliseconds;

        // Every second we prune keys which are no longer relevant, disposing of this field is the only way to stop that process
        private readonly IDisposable _keyRemoverProcess;

        public RecentlySetOrRemovedKeysManager()
        {
            _keyRemoverProcess = new Timer(_ => Prune(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void Mark(string key)
        {
            var now = TicksHelper.GetTicks64();

            if (!_dictionary.TryAdd(key, now))
                return;
            
            _queue.Enqueue(new KeyValuePair<string, long>(key, now));
        }

        public void HandleKeyChangedNotification(string key, out bool wasLocalChange)
        {
            wasLocalChange = _dictionary.TryRemove(key, out var timestamp) && IsRecent(timestamp);
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
            var age = TicksHelper.GetTicks64() - timestamp;
            
            return age < _recentWindow;
        }
    }
}