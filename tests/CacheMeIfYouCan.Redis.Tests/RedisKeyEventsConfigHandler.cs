using StackExchange.Redis;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class RedisKeyEventsConfigHandler
    {
        [Fact(Skip = "Run this on a new Redis instance to enable keyspace events")]
        public void SetConfig()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(TestConnectionString.Value + ",allowAdmin=true");

            foreach (var endpoint in connectionMultiplexer.GetEndPoints())
            {
                var server = connectionMultiplexer.GetServer(endpoint);
                server.ConfigSet("notify-keyspace-events", "AE");
                server.ConfigRewrite();
            }
        }
    }
}