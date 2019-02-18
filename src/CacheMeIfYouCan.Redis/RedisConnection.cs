using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisConnection
    {
        private readonly ConfigurationOptions _configuration;
        private readonly object _lock = new object();
        private IConnectionMultiplexer _multiplexer;
        private ILookup<(int, KeyEvents), Action<string, KeyEvents>> _onKeyChangedActions;
        private const string MessagePrefix = "__keyevent@";

        public RedisConnection(ConfigurationOptions configuration)
        {
            _configuration = configuration;
        }

        public void Connect()
        {
            _multiplexer = ConnectionMultiplexer.Connect(_configuration);
        }

        public IConnectionMultiplexer Get()
        {
            return _multiplexer;
        }

        public void SubscribeToKeyChanges(int dbIndex, KeyEvents keyEvents, Action<string, KeyEvents> onKeyChangedAction)
        {
            lock (_lock)
            {
                foreach (var keyEvent in Enum
                    .GetValues(typeof(KeyEvents))
                    .Cast<KeyEvents>()
                    .Where(k => k != KeyEvents.None && k != KeyEvents.All)
                    .Where(k => keyEvents.HasFlag(k)))
                {
                    var startSubscriber =
                        _onKeyChangedActions == null ||
                        !_onKeyChangedActions.Contains((dbIndex, keyEvent));

                    var updated = new List<KeyValuePair<(int, KeyEvents), Action<string, KeyEvents>>>
                    {
                        new KeyValuePair<(int, KeyEvents), Action<string, KeyEvents>>((dbIndex, keyEvent), onKeyChangedAction)
                    };

                    if (_onKeyChangedActions != null)
                    {
                        updated.AddRange(_onKeyChangedActions.SelectMany(
                            x => x,
                            (g, x) => new KeyValuePair<(int, KeyEvents), Action<string, KeyEvents>>(g.Key, x)));
                    }

                    Interlocked.Exchange(ref _onKeyChangedActions, updated.ToLookup(kv => kv.Key, kv => kv.Value));

                    if (startSubscriber)
                        StartSubscriber(dbIndex, keyEvent);
                }
            }
        }

        private void StartSubscriber(int dbIndex, KeyEvents keyEvent)
        {
            // All Redis instances must have keyevent notifications enabled (eg. 'notify-keyspace-events AE')
            var subscriber = _multiplexer.GetSubscriber();

            subscriber.Subscribe($"{MessagePrefix}{dbIndex}__:{keyEvent.ToString().ToLower()}", OnKeyChanged);
        }

        private void OnKeyChanged(RedisChannel channel, RedisValue key)
        {
            if (!TryParseChannel(channel, out var dbIndex, out var keyEvent))
                return;

            foreach (var action in _onKeyChangedActions[(dbIndex, keyEvent)])
                action(key, keyEvent);
        }

        private static bool TryParseChannel(string channel, out int dbIndex, out KeyEvents keyEvent)
        {
            dbIndex = 0;
            keyEvent = KeyEvents.None;
            
            if (!channel.StartsWith(MessagePrefix))
                return false;

            var parts = channel
                .Substring(MessagePrefix.Length)
                .Split(new[] { '_', ':' }, StringSplitOptions.RemoveEmptyEntries);

            return
                Int32.TryParse(parts[0], out dbIndex) &&
                Enum.TryParse(parts[1], true, out keyEvent);
        }
    }
}