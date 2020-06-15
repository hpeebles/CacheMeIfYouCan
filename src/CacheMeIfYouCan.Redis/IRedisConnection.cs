using System;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public interface IRedisConnection : IDisposable
    {
        IConnectionMultiplexer Get();
    }
}