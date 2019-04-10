using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public interface IRedisConnection
    {
        IConnectionMultiplexer Get();
    }
}