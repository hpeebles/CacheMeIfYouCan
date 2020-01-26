using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Tests;
using FluentAssertions;
using Polly;
using Xunit;

namespace CacheMeIfYouCan.Polly.Tests
{
    /// <summary>
    /// Tests for <see cref="DistributedCachePollyWrapper{TKey,TValue}"/>
    /// </summary>
    public class DistributedCachePollyWrapperTests1
    {
        [Theory]
        [InlineData("tryget", true)]
        [InlineData("tryget", false)]
        [InlineData("set", true)]
        [InlineData("set", false)]
        [InlineData("getmany", true)]
        [InlineData("getmany", false)]
        [InlineData("setmany", true)]
        [InlineData("setmany", false)]
        public async Task WhenPolicyIsSet_PolicyIsAppliedCorrectly(string action, bool applyPolicy)
        {
            var policy = Policy.Handle<Exception>().RetryAsync();
            var innerCache = new MockDistributedCache<int, int>();
            var cache = applyPolicy
                ? new DistributedCachePollyWrapper<int, int>(innerCache, policy)
                : new DistributedCachePollyWrapper<int, int>(innerCache);

            var task = action switch
            {
                "tryget" => (Func<Task>)(() => cache.TryGet(1)),
                "set" => () => cache.Set(1, 1, TimeSpan.FromSeconds(1)),
                "getmany" => () => cache.GetMany(new[] { 1 }),
                "setmany" => () => cache.SetMany(new[] { new KeyValuePair<int, int>(1, 1) }, TimeSpan.FromSeconds(1)),
                _ => throw new Exception()
            };

            innerCache.ThrowExceptionOnNextAction();

            if (applyPolicy)
                await task.Should().NotThrowAsync();
            else
                await task.Should().ThrowAsync<Exception>();
        }
    }
}