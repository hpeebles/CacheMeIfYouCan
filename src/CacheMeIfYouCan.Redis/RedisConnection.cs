using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public class RedisConnection : IRedisConnection, IRedisSubscriber
    {
        private readonly ConfigurationOptions _configuration;
        private readonly object _lock = new Object();
        private IConnectionMultiplexer _multiplexer;
        private Dictionary<(int, KeyEventType), List<(string KeyPrefix, Action<string, KeyEventType> Action)>> _onKeyChangedActions;
        private bool _disposed;
        private const string MessagePrefix = "__keyevent@";

        public RedisConnection(string connectionString)
            : this(ConfigurationOptions.Parse(connectionString))
        { }
        
        public RedisConnection(ConfigurationOptions configuration)
        {
            _configuration = configuration;
        }

        public bool Connect()
        {
            CheckDisposed();
            
            return _multiplexer?.IsConnected ?? Reset();
        }

        public IConnectionMultiplexer Get()
        {
            if (_multiplexer == null)
                Connect();
            
            return _multiplexer;
        }

        public bool Reset(bool onlyIfDisconnected = true)
        {
            if (onlyIfDisconnected && (_multiplexer?.IsConnected ?? false))
                return true;

            lock (_lock)
            {
                CheckDisposed();

                if (onlyIfDisconnected && (_multiplexer?.IsConnected ?? false))
                    return true;

                var oldConnection = _multiplexer;
                var newConnection = ConnectionMultiplexer.Connect(_configuration);
                
                if (newConnection.IsConnected)
                    RestoreSubscriptions(newConnection);
                else
                    newConnection.ConnectionRestored += (sender, args) => RestoreSubscriptions(newConnection);

                Interlocked.Exchange(ref _multiplexer, newConnection);

                oldConnection?.Dispose();
                
                return _multiplexer.IsConnected;
            }
        }

        public void SubscribeToKeyChanges(
            int dbIndex,
            KeyEventType keyEventTypes,
            Action<string, KeyEventType> onKeyChangedAction,
            string keyPrefix = null)
        {
            lock (_lock)
            {
                if (!Connect())
                    throw new Exception("Unable to connect to Redis");

                foreach (var keyEventType in Enum
                    .GetValues(typeof(KeyEventType))
                    .Cast<KeyEventType>()
                    .Where(k => k != KeyEventType.None && k != KeyEventType.All)
                    .Where(k => keyEventTypes.HasFlag(k)))
                {
                    var clone =
                        _onKeyChangedActions?.ToDictionary(kv => kv.Key, kv => kv.Value.ToList()) ??
                        new Dictionary<(int, KeyEventType), List<(string, Action<string, KeyEventType>)>>();

                    var startSubscriber = false;
                    if (!clone.TryGetValue((dbIndex, keyEventType), out var list))
                    {
                        list = new List<(string, Action<string, KeyEventType>)>();
                        clone.Add((dbIndex, keyEventType), list);
                        startSubscriber = true;
                    }

                    list.Add((keyPrefix, onKeyChangedAction));

                    Interlocked.Exchange(ref _onKeyChangedActions, clone);

                    if (startSubscriber)
                        StartSubscriber_WithinLock(_multiplexer, dbIndex, keyEventType);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _multiplexer?.Dispose();
                _multiplexer = null;
            }
        }

        private void RestoreSubscriptions(IConnectionMultiplexer multiplexer)
        {
            if (_onKeyChangedActions == null)
                return;

            lock (_lock)
            {
                CheckDisposed();
                
                foreach (var group in _onKeyChangedActions)
                    StartSubscriber_WithinLock(multiplexer, group.Key.Item1, group.Key.Item2);
            }
        }

        private void StartSubscriber_WithinLock(IConnectionMultiplexer multiplexer, int dbIndex, KeyEventType keyEventType)
        {
            // All Redis instances must have keyevent notifications enabled (eg. 'notify-keyspace-events AE')
            var subscriber = multiplexer.GetSubscriber();

            subscriber.Subscribe($"{MessagePrefix}{dbIndex}__:{keyEventType.ToString().ToLower()}", OnKeyChanged);
        }

        private void OnKeyChanged(RedisChannel channel, RedisValue key)
        {
            if (!TryParseChannel(channel, out var dbIndex, out var keyEventType))
                return;

            foreach (var (keyPrefix, action) in _onKeyChangedActions[(dbIndex, keyEventType)])
            {
                RedisValue keyWithoutPrefix;
                if (keyPrefix is null)
                    keyWithoutPrefix = key;
                else if (key.StartsWith(keyPrefix))
                    keyWithoutPrefix = ((string)key).Substring(keyPrefix.Length);
                else
                    continue;
                    
                action(keyWithoutPrefix, keyEventType);

            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().ToString());
        }

        private static bool TryParseChannel(string channel, out int dbIndex, out KeyEventType keyEventType)
        {
            dbIndex = 0;
            keyEventType = KeyEventType.None;
            
            if (!channel.StartsWith(MessagePrefix))
                return false;

            var span = channel.AsSpan().Slice(MessagePrefix.Length);

            var index = span.IndexOf('_');
            var dbIndexSpan = span.Slice(0, index);
            var keyEventSpan = span.Slice(index + 3);

            return
                Int32.TryParse(dbIndexSpan.ToString(), out dbIndex) &&
                Enum.TryParse(keyEventSpan.ToString(), true, out keyEventType);
        }
    }
}