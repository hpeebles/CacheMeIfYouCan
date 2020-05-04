using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Tests.Generators;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="LocalCacheEventsWrapper{TOuterKey,TInnerKey,TValue}"/>
    /// </summary>
    public class LocalCacheEventsWrapperTests2
    {        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void GetMany_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int, int>();

            var successfulResults = new List<(int, IReadOnlyCollection<int>, IReadOnlyCollection<KeyValuePair<int, int>>, TimeSpan)>();
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
            
            var innerCache = new MockLocalCache<int, int, int>();
            var cache = new LocalCacheEventsWrapper<int, int, int>(config, innerCache);

            var innerKeys = new[] { 2, 3 };
            
            cache.SetMany(1, new[] { new KeyValuePair<int, int>(2, 3) }, TimeSpan.FromSeconds(1));
            cache.GetMany(1, innerKeys);
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().BeEquivalentTo(innerKeys);
                successfulResults.Last().Item3.ToArray().Should().BeEquivalentTo(new KeyValuePair<int, int>(2, 3));
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.GetMany(1, innerKeys);
            action.Should().Throw<Exception>();
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
                action();
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(innerKeys);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void SetMany_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int, int>();

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
            
            var innerCache = new MockLocalCache<int, int, int>();
            var cache = new LocalCacheEventsWrapper<int, int, int>(config, innerCache);

            var values = new[] { new KeyValuePair<int, int>(2, 3), new KeyValuePair<int, int>(4, 5) };
            
            cache.SetMany(1, values, TimeSpan.FromSeconds(1));
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
            Action action = () => cache.SetMany(1, values, TimeSpan.FromSeconds(1));
            action.Should().Throw<Exception>();
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
                action();
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(values);
                failedResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void SetManyWithVaryingTimesToLive_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int, int>();

            var successfulResults = new List<(int, IReadOnlyCollection<KeyValuePair<int, ValueAndTimeToLive<int>>>, TimeSpan)>();
            var failedResults = new List<(int, IReadOnlyCollection<KeyValuePair<int, ValueAndTimeToLive<int>>>, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnSetManyWithVaryingTimesToLiveCompletedSuccessfully = (outerKey, valuesAndTimesToLive, duration) =>
                {
                    successfulResults.Add((outerKey, valuesAndTimesToLive, duration));
                };
            }

            if (flag2)
            {
                config.OnSetManyWithVaryingTimesToLiveException = (outerKey, valuesAndTimesToLive, duration, exception) =>
                {
                    failedResults.Add((outerKey, valuesAndTimesToLive, duration, exception));
                    return valuesAndTimesToLive.ToArray().Any(kv => kv.Key == 6);
                };
            }
            
            var innerCache = new MockLocalCache<int, int, int>();
            var cache = new LocalCacheEventsWrapper<int, int, int>(config, innerCache);

            var valuesAndTimesToLive = new[]
            {
                new KeyValuePair<int, ValueAndTimeToLive<int>>(2, new ValueAndTimeToLive<int>(3, TimeSpan.FromMinutes(1))),
                new KeyValuePair<int, ValueAndTimeToLive<int>>(4, new ValueAndTimeToLive<int>(5, TimeSpan.FromMinutes(2)))
            };
            
            cache.SetManyWithVaryingTimesToLive(1, valuesAndTimesToLive);
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().BeEquivalentTo(valuesAndTimesToLive);
                successfulResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.SetManyWithVaryingTimesToLive(1, valuesAndTimesToLive);
            action.Should().Throw<Exception>();
            if (flag2)
            {
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(valuesAndTimesToLive);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                failedResults.Should().BeEmpty();
            }
            
            valuesAndTimesToLive[1] = new KeyValuePair<int, ValueAndTimeToLive<int>>(6, new ValueAndTimeToLive<int>(7, TimeSpan.FromMinutes(3)));
            
            innerCache.ThrowExceptionOnNextAction();
            action = () => cache.SetManyWithVaryingTimesToLive(1, valuesAndTimesToLive);
            if (flag2)
            {
                action();
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().BeEquivalentTo(valuesAndTimesToLive);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void TryRemove_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int, int>();

            var successfulResults = new List<(int, int, bool, int, TimeSpan)>();
            var failedResults = new List<(int, int, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnTryRemoveCompletedSuccessfully = (outerKey, innerKey, found, value, duration) =>
                {
                    successfulResults.Add((outerKey, innerKey, found, value, duration));
                };
            }

            if (flag2)
            {
                config.OnTryRemoveException = (outerKey, innerKey, duration, exception) =>
                {
                    failedResults.Add((outerKey, innerKey, duration, exception));
                    return innerKey == 5;
                };
            }
            
            var innerCache = new MockLocalCache<int, int, int>();
            var cache = new LocalCacheEventsWrapper<int, int, int>(config, innerCache);

            cache.TryRemove(1, 2, out _).Should().BeFalse();
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().Be(2);
                successfulResults.Last().Item3.Should().BeFalse();
                successfulResults.Last().Item4.Should().Be(0);
                successfulResults.Last().Item5.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            cache.SetMany(1, new[] { new KeyValuePair<int, int>(3, 4) }, TimeSpan.FromSeconds(1));
            cache.TryRemove(1, 3, out _).Should().BeTrue();
            if (flag1)
            {
                successfulResults.Should().HaveCount(2);
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().Be(3);
                successfulResults.Last().Item3.Should().BeTrue();
                successfulResults.Last().Item4.Should().Be(4);
                successfulResults.Last().Item5.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.TryRemove(1, 5, out _);
            if (flag2)
            {
                action();
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(1);
                failedResults.Last().Item2.Should().Be(5);
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
    }
}