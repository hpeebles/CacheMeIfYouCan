using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisConnection
    {
        private readonly string _connectionString;
        private readonly object _lock = new object();
        private IConnectionMultiplexer _multiplexer;
        private ILookup<int, Action<string>> _onKeyChangedActions;
        private const string MessagePrefix = "__keyevent@";

        public RedisConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Connect()
        {
            _multiplexer = ConnectionMultiplexer.Connect(_connectionString);
        }

        public IConnectionMultiplexer Get()
        {
            return _multiplexer;
        }

        public void SubscribeToKeyChanges(int dbIndex, Action<string> onKeyChangedAction)
        {
            lock (_lock)
            {
                if (_onKeyChangedActions == null || !_onKeyChangedActions.Contains(dbIndex))
                    StartSubscriber(dbIndex);
            
                var updated = new List<KeyValuePair<int, Action<string>>>
                {
                    new KeyValuePair<int, Action<string>>(dbIndex, onKeyChangedAction)
                };

                if (_onKeyChangedActions != null)
                    updated.AddRange(_onKeyChangedActions.SelectMany(x => x, (g, x) => new KeyValuePair<int, Action<string>>(g.Key, x)));
                
                Interlocked.Exchange(ref _onKeyChangedActions, updated.ToLookup(kv => kv.Key, kv => kv.Value));
            }
        }

        private void StartSubscriber(int dbIndex)
        {
            // All Redis instances must have keyevent notifications enabled (eg. 'notify-keyspace-events AE')
            var subscriber = _multiplexer.GetSubscriber();

            var keyEvents = new[]
            {
                "set",
                "del",
                "expired",
                "evicted"
            };

            foreach (var keyEvent in keyEvents)
                subscriber.Subscribe($"{MessagePrefix}{dbIndex}__:{keyEvent}", OnKeyChanged);
        }

        private void OnKeyChanged(RedisChannel channel, RedisValue key)
        {
            if (!TryGetDbIndexFromChannel(channel, out var dbIndex))
                return;

            foreach (var action in _onKeyChangedActions[dbIndex])
                action(key);
        }

        private static bool TryGetDbIndexFromChannel(string channel, out int dbIndex)
        {
            if (!channel.StartsWith(MessagePrefix))
            {
                dbIndex = 0;
                return false;
            }

            var trimmed = channel.Substring(MessagePrefix.Length);

            return Int32.TryParse(trimmed.Substring(0, trimmed.IndexOf('_')), out dbIndex);
        }
    }
}