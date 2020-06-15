using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Tests.Generators;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="DistributedCacheEventsWrapper{TOuterKey,TInnerKey,TValue}"/>
    /// </summary>
    public class DistributedCacheEventsWrapperTests2
    {        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task GetMany_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new DistributedCacheEventsWrapperConfig<int, int, int>();

            var successfulResults = new List<(int, IReadOnlyCollection<int>, IReadOnlyCollection<KeyValuePair<int, ValueAndTimeToLive<int>>>, TimeSpan)>();
            var failedResults = new List<(int, IReadOnlyCollection<int>, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnGetManyCompletedSuccessfully = (outerKey, innerKeys, values, duration) =>
                {
                    successfulResults.Add((outerKey, innerKeys, values, duration));
                };
            }

            if (flag2)
            {
                config.OnGetManyException = (outerKey, innerKeys, duration, exception) =>
                {
                    failedResults.Add((outerKey, innerKeys, duration, exception));
                    return innerKeys.Contains(4);
                };
            }
            
            var innerCache = new MockDistributedCache<int, int, int>();
            var cache = new DistributedCacheEventsWrapper<int, int, int>(config, innerCache);

            var innerKeys = new[] { 2, 3 };
            
            await cache.SetMany(1, new[] { new KeyValuePair<int, int>(2, 3) }, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            await cache.GetMany(1, innerKeys).ConfigureAwait(false);
            
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().BeEquivalentTo(innerKeys);
                successfulResults.Last().Item3.ToArray().Select(kv => kv.Key).Should().BeEquivalentTo(2);
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Func<Task> action = () => cache.GetMany(1, innerKeys);
            await action.Should().ThrowAsync<Exception>().ConfigureAwait(false);
            if (flag2)
            {
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(innerKeys);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                failedResults.Should().BeEmpty();
            }

            innerKeys[1] = 4;
            
            innerCache.ThrowExceptionOnNextAction();
            action = () => cache.GetMany(1, innerKeys);
            if (flag2)
            {
                await action().ConfigureAwait(false);
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(innerKeys);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                await action.Should().ThrowAsync<Exception>().ConfigureAwait(false);
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task SetMany_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new DistributedCacheEventsWrapperConfig<int, int, int>();

            var successfulResults = new List<(int, IReadOnlyCollection<KeyValuePair<int, int>>, TimeSpan, TimeSpan)>();
            var failedResults = new List<(int, IReadOnlyCollection<KeyValuePair<int, int>>, TimeSpan, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnSetManyCompletedSuccessfully = (outerKey, values, timeToLive, duration) =>
                {
                    successfulResults.Add((outerKey, values, timeToLive, duration));
                };
            }

            if (flag2)
            {
                config.OnSetManyException = (outerKey, values, timeToLive, duration, exception) =>
                {
                    failedResults.Add((outerKey, values, timeToLive, duration, exception));
                    return values.Any(kv => kv.Key == 6);
                };
            }
            
            var innerCache = new MockDistributedCache<int, int, int>();
            var cache = new DistributedCacheEventsWrapper<int, int, int>(config, innerCache);

            var values = new[] { new KeyValuePair<int, int>(2, 3), new KeyValuePair<int, int>(4, 5) };
            
            await cache.SetMany(1, values, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().BeEquivalentTo(values);
                successfulResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Func<Task> action = () => cache.SetMany(1, values, TimeSpan.FromSeconds(1));
            await action.Should().ThrowAsync<Exception>();
            if (flag2)
            {
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(values);
                failedResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                failedResults.Should().BeEmpty();
            }
            
            values[1] = new KeyValuePair<int, int>(6, 7);
            
            innerCache.ThrowExceptionOnNextAction();
            action = () => cache.SetMany(1, values, TimeSpan.FromSeconds(1));
            if (flag2)
            {
                await action().ConfigureAwait(false);
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(values);
                failedResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                await action.Should().ThrowAsync<Exception>().ConfigureAwait(false);
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task TryRemove_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new DistributedCacheEventsWrapperConfig<int, int, int>();

            var successfulResults = new List<(int, int, bool, TimeSpan)>();
            var failedResults = new List<(int, int, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnTryRemoveCompletedSuccessfully = (outerKey, innerKey, wasRemoved, duration) =>
                {
                    successfulResults.Add((outerKey, innerKey, wasRemoved, duration));
                };
            }

            if (flag2)
            {
                config.OnTryRemoveException = (outerKey, innerKey, duration, exception) =>
                {
                    failedResults.Add((outerKey, innerKey, duration, exception));
                    return innerKey == 3;
                };
            }
            
            var innerCache = new MockDistributedCache<int, int, int>();
            var cache = new DistributedCacheEventsWrapper<int, int, int>(config, innerCache);

            await cache.TryRemove(1, 1).ConfigureAwait(false);

            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().Be(1);
                successfulResults.Last().Item3.Should().BeFalse();
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            await cache.Set(1, 2, 3, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            await cache.TryRemove(1, 2).ConfigureAwait(false);
            
            if (flag1)
            {
                successfulResults.Should().HaveCount(2);
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().Be(2);
                successfulResults.Last().Item3.Should().BeTrue();
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Func<Task> action = () => cache.TryRemove(1, 3);
            if (flag2)
            {
                await action().ConfigureAwait(false);
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().Be(3);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                await action.Should().ThrowAsync<Exception>().ConfigureAwait(false);
                failedResults.Should().BeEmpty();
            }
        }
    }
}