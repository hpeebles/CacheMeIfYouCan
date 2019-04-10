using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal static class RedisConnectionContainer
    {
        private static readonly object Lock = new object();
        private static IReadOnlyDictionary<string, RedisConnection> _connections = new Dictionary<string, RedisConnection>();
        
        public static IRedisConnection GetOrAdd(ConfigurationOptions configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            var connectionString = configuration.ToString();
            
            if (_connections.TryGetValue(connectionString, out var connection))
                return connection;

            lock (Lock)
            {
                if (_connections.TryGetValue(connectionString, out connection))
                    return connection;
                
                connection = new RedisConnection(configuration);
                connection.Connect();
                
                var updated = new ReadOnlyDictionary<string, RedisConnection>(_connections
                    .Select(kv => kv)
                    .Concat(new[] { new KeyValuePair<string, RedisConnection>(connectionString, connection) })
                    .ToDictionary(kv => kv.Key, kv => kv.Value));

                Interlocked.Exchange(ref _connections, updated);

                return connection;
            }
        }
    }
}