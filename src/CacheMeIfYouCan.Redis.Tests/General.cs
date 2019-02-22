using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers.Protobuf;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class General
    {
        [Fact]
        public async Task GetFromRedisCache()
        {
            Func<string, Task<string>> echo = new Echo();

            var results = new List<FunctionCacheGetResult>();

            var cachedEcho = echo
                .Cached()
                .WithRedis(c => c.ConnectionString = TestConnectionString.Value)
                .OnResult(results.Add)
                .Build();

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            await cachedEcho(key);
            
            results[1].Results.Single().KeyString.Should().Be(key);
            results[1].Results.Single().Outcome.Should().Be(Outcome.FromCache);
            results[1].Results.Single().CacheType.Should().Be("redis");
        }
        
        [Fact]
        public async Task WithByteSerializer()
        {
            Func<string, Task<TestProtobufClass>> func = str =>
                Task.FromResult(new TestProtobufClass {Value1 = str, Value2 = str});

            var results = new List<FunctionCacheGetResult>();

            var cachedEcho = func
                .Cached()
                .WithValueSerializer(new ProtobufSerializer())
                .WithRedis(c => c.ConnectionString = TestConnectionString.Value)
                .OnResult(results.Add)
                .Build();

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            await cachedEcho(key);
            
            results[1].Results.Single().KeyString.Should().Be(key);
            results[1].Results.Single().Outcome.Should().Be(Outcome.FromCache);
            results[1].Results.Single().CacheType.Should().Be("redis");
        }

        [DataContract]
        private class TestProtobufClass
        {
            [DataMember(Order = 1)]
            public string Value1 { get; set; }
            
            [DataMember(Order = 2)]
            public string Value2 { get; set; }
        }
    }
}