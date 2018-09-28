using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class Generic
    {
        [Fact]
        public async Task KeyIsSerializedCorrectly()
        {
            Func<List<int>, int> func = x => x.Sum();

            var results1 = new List<FunctionCacheGetResult<List<int>, int>>();
            var results2 = new List<FunctionCacheGetResult<List<int>, int>>();
            
            var cachedFuncWithConstantSerializer = func
                .Cached()
                .WithKeySerializer(x => "test")
                .OnResult(results1.Add)
                .Build();
            
            var cachedFuncWithSerializer = func
                .Cached()
                .WithKeySerializer(x => String.Join(",", x))
                .OnResult(results2.Add)
                .Build();
            
            var key1 = new List<int> { 1 };
            var key2 = new List<int> { 2, 3 };
            var key3 = new List<int> { 2, 3 };
            
            await cachedFuncWithConstantSerializer(key1);
            await cachedFuncWithConstantSerializer(key2);
            await cachedFuncWithSerializer(key1);
            await cachedFuncWithSerializer(key2);
            await cachedFuncWithSerializer(key3);
            
            Assert.Equal(results1[0].Value, results1[1].Value);
            Assert.Equal(Outcome.FromCache, results1[1].Outcome);
            
            Assert.Equal(Outcome.Fetch, results2[1].Outcome);
            Assert.Equal(Outcome.FromCache, results2[2].Outcome);
            Assert.Equal(results2[1].Value, results2[2].Value);
        }
    }
}