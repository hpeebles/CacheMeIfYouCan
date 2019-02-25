using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class NullValue
    {
        [Theory]
        [InlineData(null)]
        [InlineData("null")]
        public async Task WithNullValue(string nullValue)
        {
            Func<string, Task<string>> func = k => Task.FromResult<string>(null);

            var results = new List<FunctionCacheGetResult>();

            var cachedFunc = func
                .Cached()
                .WithRedis(c =>
                {
                    c.ConnectionString = TestConnectionString.Value;
                    c.NullValue = nullValue;
                })
                .OnResult(results.Add)
                .Build();

            var key = Guid.NewGuid().ToString();

            await cachedFunc(key);
            var value = await cachedFunc(key);

            value.Should().BeNull();
            
            results[1].Results.Single().Outcome.Should().Be(nullValue == null ? Outcome.Fetch : Outcome.FromCache);
            results[1].Results.Single().CacheType.Should().Be(nullValue == null ? null : "redis");
        }
    }
}