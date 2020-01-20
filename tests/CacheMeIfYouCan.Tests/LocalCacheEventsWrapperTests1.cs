using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.LocalCaches;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="LocalCacheEventsWrapper{TKey,TValue}"/>
    /// </summary>
    public class LocalCacheEventsWrapperTests1
    {
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void TryGet_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int>();

            var successfulResults = new List<(int, bool, int, TimeSpan)>();
            var failedResults = new List<(int, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnTryGetCompletedSuccessfully = (key, found, value, duration) =>
                {
                    successfulResults.Add((key, found, value, duration));
                };
            }

            if (flag2)
            {
                config.OnTryGetException = (key, duration, exception) =>
                {
                    failedResults.Add((key, duration, exception));
                    return key == 3;
                };
            }
            
            var innerCache = new MockLocalCache<int, int>();
            var cache = new LocalCacheEventsWrapper<int, int>(config, innerCache);

            cache.TryGet(1, out _).Should().BeFalse();
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().BeFalse();
                successfulResults.Last().Item3.Should().Be(0);
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            cache.Set(2, 3, TimeSpan.FromSeconds(1));
            cache.TryGet(2, out _).Should().BeTrue();
            if (flag1)
            {
                successfulResults.Should().HaveCount(2);
                successfulResults.Last().Item1.Should().Be(2);
                successfulResults.Last().Item2.Should().BeTrue();
                successfulResults.Last().Item3.Should().Be(3);
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.TryGet(3, out _);
            if (flag2)
            {
                action();
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(3);
                failedResults.Last().Item2.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void Set_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int>();

            var successfulResults = new List<(int, int, TimeSpan, TimeSpan)>();
            var failedResults = new List<(int, int, TimeSpan, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnSetCompletedSuccessfully = (key, value, timeToLive, duration) =>
                {
                    successfulResults.Add((key, value, timeToLive, duration));
                };
            }

            if (flag2)
            {
                config.OnSetException = (key, value, timeToLive, duration, exception) =>
                {
                    failedResults.Add((key, value, timeToLive, duration, exception));
                    return key == 5;
                };
            }
            
            var innerCache = new MockLocalCache<int, int>();
            var cache = new LocalCacheEventsWrapper<int, int>(config, innerCache);

            cache.Set(1, 2, TimeSpan.FromSeconds(1));
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().Be(2);
                successfulResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.Set(3, 4, TimeSpan.FromSeconds(1));
            action.Should().Throw<Exception>();
            if (flag2)
            {
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(3);
                failedResults.Last().Item2.Should().Be(4);
                failedResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                failedResults.Should().BeEmpty();
            }
            
            innerCache.ThrowExceptionOnNextAction();
            action = () => cache.Set(5, 6, TimeSpan.FromSeconds(1));
            if (flag2)
            {
                action();
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().Be(5);
                failedResults.Last().Item2.Should().Be(6);
                failedResults.Last().Item3.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void GetMany_EventsAreTriggeredSuccessfully(bool flag1, bool flag2)
        {
            var config = new LocalCacheEventsWrapperConfig<int, int>();

            var successfulResults = new List<(IReadOnlyCollection<int>, IReadOnlyCollection<KeyValuePair<int, int>>, TimeSpan)>();
            var failedResults = new List<(IReadOnlyCollection<int>, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnGetManyCompletedSuccessfully = (keys, values, duration) =>
                {
                    successfulResults.Add((keys, values, duration));
                };
            }

            if (flag2)
            {
                config.OnGetManyException = (keys, duration, exception) =>
                {
                    failedResults.Add((keys, duration, exception));
                    return keys.Contains(3);
                };
            }
            
            var innerCache = new MockLocalCache<int, int>();
            var cache = new LocalCacheEventsWrapper<int, int>(config, innerCache);

            var keys = new[] { 1, 2 };
            
            cache.Set(1, 2, TimeSpan.FromSeconds(1));
            cache.GetMany(keys);
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().BeSameAs(keys);
                successfulResults.Last().Item2.Should().BeEquivalentTo(new KeyValuePair<int, int>(1, 2));
                successfulResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.GetMany(keys);
            action.Should().Throw<Exception>();
            if (flag2)
            {
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().BeSameAs(keys);
                failedResults.Last().Item2.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                failedResults.Should().BeEmpty();
            }

            keys[1] = 3;
            
            innerCache.ThrowExceptionOnNextAction();
            action = () => cache.GetMany(keys);
            if (flag2)
            {
                action();
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().BeSameAs(keys);
                failedResults.Last().Item2.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
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
            var config = new LocalCacheEventsWrapperConfig<int, int>();

            var successfulResults = new List<(IReadOnlyCollection<KeyValuePair<int, int>>, TimeSpan, TimeSpan)>();
            var failedResults = new List<(IReadOnlyCollection<KeyValuePair<int, int>>, TimeSpan, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnSetManyCompletedSuccessfully = (values, timeToLive, duration) =>
                {
                    successfulResults.Add((values, timeToLive, duration));
                };
            }

            if (flag2)
            {
                config.OnSetManyException = (values, timeToLive, duration, exception) =>
                {
                    failedResults.Add((values, timeToLive, duration, exception));
                    return values.Any(kv => kv.Key == 5);
                };
            }
            
            var innerCache = new MockLocalCache<int, int>();
            var cache = new LocalCacheEventsWrapper<int, int>(config, innerCache);

            var values = new[] { new KeyValuePair<int, int>(1, 2), new KeyValuePair<int, int>(3, 4) };
            
            cache.SetMany(values, TimeSpan.FromSeconds(1));
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().BeSameAs(values);
                successfulResults.Last().Item2.Should().Be(TimeSpan.FromSeconds(1));
                successfulResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.SetMany(values, TimeSpan.FromSeconds(1));
            action.Should().Throw<Exception>();
            if (flag2)
            {
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().BeSameAs(values);
                failedResults.Last().Item2.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                failedResults.Should().BeEmpty();
            }
            
            values[1] = new KeyValuePair<int, int>(5, 6);
            
            innerCache.ThrowExceptionOnNextAction();
            action = () => cache.SetMany(values, TimeSpan.FromSeconds(1));
            if (flag2)
            {
                action();
                failedResults.Should().HaveCount(2);
                failedResults.Last().Item1.Should().BeSameAs(values);
                failedResults.Last().Item2.Should().Be(TimeSpan.FromSeconds(1));
                failedResults.Last().Item3.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
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
            var config = new LocalCacheEventsWrapperConfig<int, int>();

            var successfulResults = new List<(int, bool, int, TimeSpan)>();
            var failedResults = new List<(int, TimeSpan, Exception)>();
            
            if (flag1)
            {
                config.OnTryRemoveCompletedSuccessfully = (key, found, value, duration) =>
                {
                    successfulResults.Add((key, found, value, duration));
                };
            }

            if (flag2)
            {
                config.OnTryRemoveException = (key, duration, exception) =>
                {
                    failedResults.Add((key, duration, exception));
                    return key == 4;
                };
            }
            
            var innerCache = new MockLocalCache<int, int>();
            var cache = new LocalCacheEventsWrapper<int, int>(config, innerCache);

            cache.TryRemove(1, out _).Should().BeFalse();
            if (flag1)
            {
                successfulResults.Should().ContainSingle();
                successfulResults.Last().Item1.Should().Be(1);
                successfulResults.Last().Item2.Should().BeFalse();
                successfulResults.Last().Item3.Should().Be(0);
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            cache.Set(2, 3, TimeSpan.FromSeconds(1));
            cache.TryRemove(2, out _).Should().BeTrue();
            if (flag1)
            {
                successfulResults.Should().HaveCount(2);
                successfulResults.Last().Item1.Should().Be(2);
                successfulResults.Last().Item2.Should().BeTrue();
                successfulResults.Last().Item3.Should().Be(3);
                successfulResults.Last().Item4.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                successfulResults.Should().BeEmpty();
            }

            innerCache.ThrowExceptionOnNextAction();
            Action action = () => cache.TryRemove(4, out _);
            if (flag2)
            {
                action();
                failedResults.Should().ContainSingle();
                failedResults.Last().Item1.Should().Be(4);
                failedResults.Last().Item2.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
            }
            else
            {
                action.Should().Throw<Exception>();
                failedResults.Should().BeEmpty();
            }
        }
    }
}