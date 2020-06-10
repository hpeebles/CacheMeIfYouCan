using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Events.CachedObject;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class CachedObjectTests
    {
        protected virtual ICachedObject<T> BuildCachedObject<T>(ICachedObjectConfigurationManager<T> config)
        {
            return config.Build();
        }
        
        [Fact]
        public void Initialize_WhenCalledMultipleTimesConcurrently_ExecutesOnce()
        {
            var executionCount = 0;
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1));
            
            using var cachedObject = BuildCachedObject(config);

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
            var config = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMinutes(1));

            using var cachedObject = BuildCachedObject(config);

            cachedObject.State.Should().Be(CachedObjectState.PendingInitialization);
            var value = cachedObject.Value;
            cachedObject.State.Should().Be(CachedObjectState.Ready);

            value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void OnceInitialized_ValueIsRefreshedAtRegularIntervals()
        {
            var values = new List<DateTime>();
            
            var countdown = new CountdownEvent(5);

            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromSeconds(1));

            using var cachedObject = BuildCachedObject(config);
            
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

            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromSeconds(1));
            
            using var cachedObject = BuildCachedObject(config);

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
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1));
                
            using var cachedObject = BuildCachedObject(config);

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
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50));
                
            using var cachedObject = BuildCachedObject(config);

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
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50));
            
            using var cachedObject = BuildCachedObject(config);

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
            var config = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50));
                
            using var cachedObject = BuildCachedObject(config);
            
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
            var config = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50));
            
            using var cachedObject = BuildCachedObject(config);
            
            cachedObject.Initialize();

            var task = cachedObject.InitializeAsync();
            task.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task WithRefreshValueFuncTimeout_FuncIsCancelledOnceTimeoutIsReached()
        {
            var isFirstAttempt = true;
            
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(50))
                .WithRefreshValueFuncTimeout(TimeSpan.FromMilliseconds(100));
            
            using var cachedObject = BuildCachedObject(config);

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
            
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(1));
            
            using var cachedObject = BuildCachedObject(config);

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
            using var fastRefreshEvent = new ManualResetEventSlim();
            using var slowRefreshEvent = new ManualResetEventSlim();

            var lastRefresh = DateTime.MinValue;
            var averageRefreshInterval = TimeSpan.FromSeconds(1);

            var config = CachedObjectFactory.ConfigureFor(() =>
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
                .WithJitter(99);
            
            using var cachedObject = BuildCachedObject(config);
            
            cachedObject.Initialize();

            WaitHandle
                .WaitAll(new[] { fastRefreshEvent.WaitHandle, slowRefreshEvent.WaitHandle }, TimeSpan.FromSeconds(30))
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnInitialized_ActionCalledAsExpected(bool addedPreBuilding)
        {
            using var signal = new ManualResetEventSlim();
            
            var config = CachedObjectFactory.ConfigureFor(() => DateTime.UtcNow);
            
            if (addedPreBuilding)
                config.OnInitialized(_ => signal.Set());

            using var cachedObject = BuildCachedObject(config);

            if (!addedPreBuilding)
                cachedObject.OnInitialized += (_, __) => signal.Set();

            signal.IsSet.Should().BeFalse();
            
            await cachedObject.InitializeAsync();

            signal.Wait(TimeSpan.FromMilliseconds(100)).Should().BeTrue();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnDisposed_ActionCalledAsExpected(bool addedPreBuilding)
        {
            using var signal = new ManualResetEventSlim();
            
            var config = CachedObjectFactory.ConfigureFor(() => DateTime.UtcNow);
            
            if (addedPreBuilding)
                config.OnDisposed(_ => signal.Set());

            using var cachedObject = BuildCachedObject(config);

            if (!addedPreBuilding)
                cachedObject.OnDisposed += (_, __) => signal.Set();

            await cachedObject.InitializeAsync();

            signal.IsSet.Should().BeFalse();

            cachedObject.Dispose();

            signal.IsSet.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValueRefreshed_ActionCalledAsExpected(bool addedPreBuilding)
        {
            var events = new List<ValueRefreshedEvent<DateTime>>();
            using var countdown = new CountdownEvent(10);
            
            var config = CachedObjectFactory.ConfigureFor(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    return DateTime.UtcNow;
                })
                .WithRefreshInterval(TimeSpan.FromMilliseconds(20));

            if (addedPreBuilding)
                config.OnValueRefresh(Action);

            using var cachedObject = BuildCachedObject(config);

            if (!addedPreBuilding)
                cachedObject.OnValueRefreshed += (_, e) => Action(e);
            
            await cachedObject.InitializeAsync();

            var start = DateTime.UtcNow;
            
            countdown.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

            var end = DateTime.UtcNow;
            
            cachedObject.Dispose();

            events.Should().HaveCountGreaterOrEqualTo(10);

            events.First().NewValue.Should().BeCloseTo(start, TimeSpan.FromMilliseconds(100));
            events.Last().NewValue.Should().BeCloseTo(end, TimeSpan.FromMilliseconds(100));
            
            for (var index = 0; index < 10; index++)
            {
                var e = events[index];

                if (index == 0)
                    e.PreviousValue.Should().Be(DateTime.MinValue);
                else
                    e.PreviousValue.Should().Be(events[index - 1].NewValue);
                
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.DateOfPreviousSuccessfulRefresh.Should().BeCloseTo(e.PreviousValue, TimeSpan.FromMilliseconds(100));
                e.Version.Should().Be(index + 1);
            }
            
            void Action(ValueRefreshedEvent<DateTime> e)
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
            var events = new List<ValueRefreshExceptionEvent<DateTime>>();
            using var countdown = new CountdownEvent(10);
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
                config.OnValueRefresh(onException: Action);

            using var cachedObject = BuildCachedObject(config);

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
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.Version.Should().Be(1);
            }
            
            void Action(ValueRefreshExceptionEvent<DateTime> e)
            {
                events.Add(e);
                if (!countdown.IsSet)
                    countdown.Signal();
            }
        }

        [Fact]
        public void WhenDisposed_InnerValueIsAlsoDisposed()
        {
            var disposable = new DisposableClass();

            var config = CachedObjectFactory
                .ConfigureFor(() => disposable)
                .WithRefreshInterval(TimeSpan.FromSeconds(1));
            
            using var cachedObject = BuildCachedObject(config);

            cachedObject.Initialize();

            disposable.IsDisposed.Should().BeFalse();
            
            cachedObject.Dispose();

            disposable.IsDisposed.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RefreshValueAsync_TriggersRefresh(bool async)
        {
            var executionCount = 0;
            
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1));
            
            using var cachedObject = BuildCachedObject(config);

            cachedObject.Initialize();

            for (var i = 2; i < 10; i++)
            {
                if (async)
                    await cachedObject.RefreshValueAsync().ConfigureAwait(false);
                else
                    cachedObject.RefreshValue();
                
                executionCount.Should().Be(i);
            }

            DateTime GetValue()
            {
                Interlocked.Increment(ref executionCount);
                return DateTime.UtcNow;
            }
        }
        
        [Fact]
        public async Task RefreshValueAsync_IfRefreshAlreadyRunning_TaskWillBeQueuedSoThatRefreshAlwaysStartsAfterMethodCall()
        {
            var executionCount = 0;
            
            var config = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(500));
            
            using var cachedObject = BuildCachedObject(config);

            cachedObject.Initialize();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            var tasks = Enumerable
                .Range(0, 10)
                .Select(_ => RunTask())
                .ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            executionCount.Should().BeGreaterThan(5).And.BeLessThan(15);

            async Task RunTask()
            {
                while (!cts.IsCancellationRequested)
                {
                    var start = DateTime.UtcNow;
                    await cachedObject.RefreshValueAsync().ConfigureAwait(false);
                    var value = cachedObject.Value;
                    start.Should().BeOnOrBefore(value.Start);
                }
            }
            
            async Task<(DateTime Start, DateTime Finish)> GetValue()
            {
                var start = DateTime.UtcNow;
                Interlocked.Increment(ref executionCount);
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                return (start, DateTime.UtcNow);
            }
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(9)]
        [InlineData(10)]
        public async Task RefreshValueAsync_IfCancelled_CancelsUnderlyingFuncOnlyIfEveryWaitingTaskIsCancelled(int numberToCancel)
        {
            var refreshValueFuncCancelled = false;
            
            var config = CachedObjectFactory.ConfigureFor(GetValue);
            
            using var cachedObject = BuildCachedObject(config);

            cachedObject.Initialize();

            var firstRefreshTask = cachedObject.RefreshValueAsync();
            
            var ctsList = Enumerable
                .Range(0, 10)
                .Select(_ => new CancellationTokenSource())
                .ToArray();

            var queuedRefreshTasks = Enumerable
                .Range(0, 10)
                .Select(i => cachedObject.RefreshValueAsync(cancellationToken: ctsList[i].Token))
                .ToArray();

            await firstRefreshTask.ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            
            for (var i = 0; i < numberToCancel; i++)
                ctsList[i].Cancel();

            try
            {
                await Task.WhenAll(queuedRefreshTasks).ConfigureAwait(false);
            }
            catch
            { }

            for (var i = 0; i < 10; i++)
                queuedRefreshTasks[i].Status.Should().Be(i < numberToCancel ? TaskStatus.Canceled : TaskStatus.RanToCompletion);

            refreshValueFuncCancelled.Should().Be(numberToCancel == 10);

            async Task<DateTime> GetValue(CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    refreshValueFuncCancelled = true;
                    throw;
                }
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public async Task RefreshValueAsync_WithSkipIfPreviousRefreshStartedWithinTimeFrameSet_SkipsWhenWithinTimeFrame()
        {
            var executionCount = 0;

            var config = CachedObjectFactory.ConfigureFor(GetValue);
            
            using var cachedObject = BuildCachedObject(config);

            cachedObject.Initialize();

            executionCount.Should().Be(1);

            cachedObject.RefreshValue();

            executionCount.Should().Be(2);

            cachedObject.RefreshValue(TimeSpan.FromMilliseconds(200));

            executionCount.Should().Be(2);

            cachedObject.RefreshValue(TimeSpan.FromMilliseconds(50));
            
            executionCount.Should().Be(3);

            var task1 = cachedObject.RefreshValueAsync(TimeSpan.FromMilliseconds(50));
            var task2 = cachedObject.RefreshValueAsync(TimeSpan.FromMilliseconds(50));

            await task1.ConfigureAwait(false);

            executionCount.Should().Be(4);
            
            await task2.ConfigureAwait(false);
            
            executionCount.Should().Be(4);

            async Task<DateTime> GetValue()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                Interlocked.Increment(ref executionCount);
                return DateTime.UtcNow;
            }
        }
    }
}