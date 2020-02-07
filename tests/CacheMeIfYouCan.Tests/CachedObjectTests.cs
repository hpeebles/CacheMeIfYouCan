using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class CachedObjectTests
    {
        [Fact]
        public void Initialize_WhenCalledMultipleTimesConcurrently_ExecutesOnce()
        {
            var executionCount = 0;
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            var tasks = Enumerable
                .Range(0, 10)
                .Select(_ => Task.Run(() => cachedObject.InitializeAsync()))
                .ToArray();

            Task.WaitAll(tasks);

            executionCount.Should().Be(1);
            
            async Task<DateTime> GetValue()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                Interlocked.Increment(ref executionCount);
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public void Value_IfCalledWhenNotInitialized_WillInitializeValue()
        {
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            cachedObject.State.Should().Be(CachedObjectState.PendingInitialization);
            var value = cachedObject.Value;
            cachedObject.State.Should().Be(CachedObjectState.Ready);

            value.Should().BeCloseTo(DateTime.UtcNow);
        }

        [Fact]
        public void OnceInitialized_ValueIsRefreshedAtRegularIntervals()
        {
            var values = new List<DateTime>();
            
            var countdown = new CountdownEvent(5);
            
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            cachedObject.Initialize();
            
            countdown.Wait();

            for (var i = 1; i < 5; i++)
            {
                var diff = values[i] - values[i - 1];
                diff.Should().BeGreaterThan(TimeSpan.FromSeconds(0.5)).And.BeLessThan(TimeSpan.FromSeconds(1.5));
            }
            
            DateTime GetValue()
            {
                var now = DateTime.UtcNow;
                values.Add(now);
                
                if (!countdown.IsSet)
                    countdown.Signal();
                
                return now;
            }
        }

        [Fact]
        public void Initialize_IfFails_CanBeCalledAgain()
        {
            var count = 0;
            
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            Action action = () => cachedObject.Initialize();

            for (var i = 0; i < 5; i++)
                action.Should().Throw<Exception>();

            cachedObject.State.Should().Be(CachedObjectState.PendingInitialization);
            
            action.Should().NotThrow();

            cachedObject.State.Should().Be(CachedObjectState.Ready);
            
            DateTime GetValue()
            {
                if (count++ < 5)
                    throw new Exception();
                
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public async Task Status_UpdatedCorrectly()
        {
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            cachedObject.State.Should().Be(CachedObjectState.PendingInitialization);
            
            var task = cachedObject.InitializeAsync();

            cachedObject.State.Should().Be(CachedObjectState.InitializationInProgress);

            await task.ConfigureAwait(false);

            cachedObject.State.Should().Be(CachedObjectState.Ready);
            
            cachedObject.Dispose();

            cachedObject.State.Should().Be(CachedObjectState.Disposed);
            
            static async Task<DateTime> GetValue()
            {
                await Task.Delay(TimeSpan.FromSeconds(0.5)).ConfigureAwait(false);
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public async Task OnceDisposed_RefreshValueFuncStopsBeingCalled()
        {
            var executionCount = 0;
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50))
                .Build();

            cachedObject.Initialize();

            executionCount.Should().Be(1);

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            
            cachedObject.Dispose();

            var countAfterDispose = executionCount;

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            
            executionCount.Should().Be(countAfterDispose);
            
            DateTime GetValue()
            {
                Interlocked.Increment(ref executionCount);
                return DateTime.UtcNow;
            }
        }
        
        [Fact]
        public async Task Version_UpdatedEachTimeValueIsRefreshed()
        {
            var executionCount = 0;
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50))
                .Build();

            cachedObject.Version.Should().Be(0);
            cachedObject.Initialize();
            cachedObject.Version.Should().Be(1);
            
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            
            cachedObject.Dispose();

            cachedObject.Version.Should().Be(executionCount);
            
            DateTime GetValue()
            {
                Interlocked.Increment(ref executionCount);
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public void OnceDisposed_ThrowsObjectDisposedExceptionIfAccessed()
        {
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50))
                .Build();
            
            cachedObject.Initialize();
            cachedObject.Dispose();

            Func<DateTime> func = () => cachedObject.Value;
            func.Should().ThrowExactly<ObjectDisposedException>();

            Action action = () => cachedObject.Initialize();
            action.Should().ThrowExactly<ObjectDisposedException>();

            cachedObject.Version.Should().Be(1);
        }

        [Fact]
        public void InitializeAsync_IfAlreadyInitialized_ReturnsCompletedTask()
        {
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50))
                .Build();
            
            cachedObject.Initialize();

            var task = cachedObject.InitializeAsync();
            task.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task WithRefreshValueFuncTimeout_FuncIsCancelledOnceTimeoutIsReached()
        {
            var isFirstAttempt = true;
            
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50))
                .WithRefreshValueFuncTimeout(TimeSpan.FromMilliseconds(100))
                .Build();

            Func<Task> func = () => cachedObject.InitializeAsync();
            await func.Should().ThrowAsync<TaskCanceledException>();

            cachedObject.State.Should().Be(CachedObjectState.PendingInitialization);
            
            await func.Should().NotThrowAsync();

            cachedObject.State.Should().Be(CachedObjectState.Ready);
            
            async Task<DateTime> GetValue(CancellationToken cancellationToken)
            {
                if (isFirstAttempt)
                {
                    isFirstAttempt = false;
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
                }
                
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public async Task WhenDisposed_RefreshValueFuncWillBeCancelledIfInProgress()
        {
            var signal = new ManualResetEventSlim();
            
            using var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(1))
                .Build();

            await cachedObject.InitializeAsync().ConfigureAwait(false);

            await Task.Delay(50).ConfigureAwait(false);
            
            cachedObject.Dispose();

            signal.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue();
            
            async Task<DateTime> GetValue(CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    if (!signal.IsSet)
                        signal.Set();
                }
                
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public void WithJitter_CausesRefreshIntervalToFluctuate()
        {
            var fastRefreshEvent = new ManualResetEventSlim();
            var slowRefreshEvent = new ManualResetEventSlim();

            var lastRefresh = DateTime.MinValue;
            var averageRefreshInterval = TimeSpan.FromSeconds(1);

            using var cachedObject = CachedObjectFactory.ConfigureFor(() =>
                {
                    var now = DateTime.UtcNow;

                    if (lastRefresh != DateTime.MinValue)
                    {
                        var refreshInterval = now - lastRefresh;
                        if (refreshInterval < 0.5 * averageRefreshInterval)
                            fastRefreshEvent.Set();
                        else if (refreshInterval > 1.5 * averageRefreshInterval)
                            slowRefreshEvent.Set();
                    }

                    lastRefresh = now;

                    return now;
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .WithJitter(99)
                .Build();
            
            cachedObject.Initialize();

            WaitHandle
                .WaitAll(new[] { fastRefreshEvent.WaitHandle, slowRefreshEvent.WaitHandle }, TimeSpan.FromSeconds(30))
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValueRefreshed_ActionCalledAsExpected(bool addedPreBuilding)
        {
            var events = new List<CachedObjectValueRefreshedEvent<DateTime>>();
            var countdown = new CountdownEvent(10);
            
            var config = CachedObjectFactory.ConfigureFor(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    return DateTime.UtcNow;
                })
                .WithRefreshInterval(TimeSpan.FromMilliseconds(20));

            if (addedPreBuilding)
                config.OnValueRefreshed(Action);

            var cachedObject = config.Build();

            if (!addedPreBuilding)
                cachedObject.OnValueRefreshed += (_, e) => Action(e);
            
            await cachedObject.InitializeAsync();

            var start = DateTime.UtcNow;
            
            countdown.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

            var end = DateTime.UtcNow;
            
            cachedObject.Dispose();

            events.Should().HaveCountGreaterOrEqualTo(10);

            events.First().NewValue.Should().BeCloseTo(start);
            events.Last().NewValue.Should().BeCloseTo(end);
            
            for (var index = 0; index < 10; index++)
            {
                var e = events[index];

                if (index == 0)
                    e.PreviousValue.Should().Be(DateTime.MinValue);
                else
                    e.PreviousValue.Should().Be(events[index - 1].NewValue);
                
                e.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(100));
                e.DateOfPreviousSuccessfulRefresh.Should().BeCloseTo(e.PreviousValue);
                e.Version.Should().Be(index + 1);
            }
            
            void Action(CachedObjectValueRefreshedEvent<DateTime> e)
            {
                events.Add(e);
                if (!countdown.IsSet)
                    countdown.Signal();
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValueRefreshException_ActionCalledAsExpected(bool addedPreBuilding)
        {
            var events = new List<CachedObjectValueRefreshExceptionEvent<DateTime>>();
            var countdown = new CountdownEvent(10);
            var first = true;
            
            var config = CachedObjectFactory.ConfigureFor(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    
                    if (!first)
                        throw new Exception("error!");

                    first = false;
                    return DateTime.UtcNow;
                })
                .WithRefreshInterval(TimeSpan.FromMilliseconds(20));

            if (addedPreBuilding)
                config.OnValueRefreshException(Action);

            var cachedObject = config.Build();

            if (!addedPreBuilding)
                cachedObject.OnValueRefreshException += (_, e) => Action(e);
            
            await cachedObject.InitializeAsync();

            var firstValue = cachedObject.Value;
            
            countdown.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

            cachedObject.Dispose();

            events.Should().HaveCountGreaterOrEqualTo(10);

            for (var index = 0; index < 10; index++)
            {
                var e = events[index];
                e.Exception.Message.Should().Be("error!");
                e.CurrentValue.Should().Be(firstValue);
                e.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50));
                e.Version.Should().Be(1);
            }
            
            void Action(CachedObjectValueRefreshExceptionEvent<DateTime> e)
            {
                events.Add(e);
                if (!countdown.IsSet)
                    countdown.Signal();
            }
        }
    }
}