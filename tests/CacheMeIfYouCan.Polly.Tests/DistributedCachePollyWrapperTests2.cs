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
    /// Tests for <see cref="DistributedCachePollyWrapper{TOuterKey,TInnerKey,TValue}"/>
    /// </summary>
    public class DistributedCachePollyWrapperTests2
    {
        [Theory]
        [InlineData("getmany", true)]
        [InlineData("getmany", false)]
        [InlineData("setmany", true)]
        [InlineData("setmany", false)]
        public async Task WhenPolicyIsSet_PolicyIsAppliedCorrectly(string action, bool applyPolicy)
        {
            var policy = Policy.Handle<Exception>().RetryAsync();
            var innerCache = new MockDistributedCache<int, int, int>();
            var cache = applyPolicy
                ? new DistributedCachePollyWrapper<int, int, int>(innerCache, policy)
                : new DistributedCachePollyWrapper<int, int, int>(innerCache);

            var task = action switch
            {
                "getmany" => (Func<Task>)(() => cache.GetMany(1, new[] { 1 })),
                "setmany" => () => cache.SetMany(1, new[] { new KeyValuePair<int, int>(1, 1) }, TimeSpan.FromSeconds(1)),
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