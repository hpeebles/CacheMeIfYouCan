using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class Generic
    {
        private readonly CacheSetupLock _setupLock;

        public Generic(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task KeyIsSerializedCorrectly()
        {
            var results1 = new List<FunctionCacheGetResult<List<int>, int>>();
            var results2 = new List<FunctionCacheGetResult<List<int>, int>>();
            
            Func<List<int>, Task<int>> func = x => Task.FromResult(x.Sum());
            Func<List<int>, Task<int>> cachedFuncWithConstantSerializer;
            Func<List<int>, Task<int>> cachedFuncWithSerializer;
            using (_setupLock.Enter())
            {
                cachedFuncWithConstantSerializer = func
                    .Cached()
                    .WithKeySerializer(x => "test")
                    .OnResult(results1.Add)
                    .Build();

                cachedFuncWithSerializer = func
                    .Cached()
                    .WithKeySerializer(x => String.Join(",", x))
                    .OnResult(results2.Add)
                    .Build();
            }

            var key1 = new List<int> { 1 };
            var key2 = new List<int> { 2, 3 };
            var key3 = new List<int> { 2, 3 };
            
            await cachedFuncWithConstantSerializer(key1);
            await cachedFuncWithConstantSerializer(key2);
            await cachedFuncWithSerializer(key1);
            await cachedFuncWithSerializer(key2);
            await cachedFuncWithSerializer(key3);
            
            results1[0].Results.Single().Value.Should().Be(results1[1].Results.Single().Value);
            results1[1].Results.Single().Outcome.Should().Be(Outcome.FromCache);
            
            results2[1].Results.Single().Outcome.Should().Be(Outcome.Fetch);
            results2[2].Results.Single().Outcome.Should().Be(Outcome.FromCache);
            results2[1].Results.Single().Value.Should().Be(results2[2].Results.Single().Value);
        }
    }
}