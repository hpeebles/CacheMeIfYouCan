using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Tests;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class KeyspacePrefix
    {
        [Fact]
        public async Task KeyspacePrefixAddedSuccessfully()
        {
            Func<string, Task<string>> echo = new Echo();

            var prefix = Guid.NewGuid().ToString();
            
            var cachedEcho = echo
                .Cached()
                .WithRedis(c =>
                {
                    c.ConnectionString = TestConnectionString.Value;
                })
                .WithKeyspacePrefix(prefix)
                .Build();

            var redisClient = ConnectionMultiplexer.Connect(TestConnectionString.Value);

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            
            redisClient.GetDatabase().KeyExists(prefix + key).Should().BeTrue();
        }
    }
}