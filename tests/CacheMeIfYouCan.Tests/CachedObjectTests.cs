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
                
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
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
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.Version.Should().Be(1);
            }
            
            void Action(CachedObjectValueRefreshExceptionEvent<DateTime> e)
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

            var cachedObject = CachedObjectFactory
                .ConfigureFor(() => disposable)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

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
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

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
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(500))
                .Build();

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
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

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

        [Theory]
        [InlineData(0, 2, 3, 4, 5)]
        [InlineData(150, 2, 2, 3, 4)]
        [InlineData(300, 1, 2, 2, 3)]
        public async Task RefreshValueAsync_WithSkipIfPreviousRefreshStartedWithinTimeFrameSet_SkipsWhenWithinTimeFrame(
            int timeFrameMs, int expected1, int expected2, int expected3, int expected4)
        {
            var timeFrame = TimeSpan.FromMilliseconds(timeFrameMs);
            
            var executionStartCount = 0;
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(GetValue)
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            cachedObject.Initialize();

            var task1 = cachedObject.RefreshValueAsync(timeFrame);

            await Task.Delay(10).ConfigureAwait(false);
            
            executionStartCount.Should().Be(expected1);

            await Task.Delay(100).ConfigureAwait(false);

            var task2 = cachedObject.RefreshValueAsync(timeFrame);

            await Task.Delay(150).ConfigureAwait(false);

            executionStartCount.Should().Be(expected2);
            
            var task3 = cachedObject.RefreshValueAsync(timeFrame);
            var task4 = cachedObject.RefreshValueAsync(timeFrame);

            await Task.Delay(250).ConfigureAwait(false);

            executionStartCount.Should().Be(expected3);
            
            var task5 = cachedObject.RefreshValueAsync(timeFrame);
            
            await Task.WhenAll(task1, task2, task3, task4, task5).ConfigureAwait(false);
            
            executionStartCount.Should().Be(expected4);
            
            async Task<DateTime> GetValue()
            {
                Interlocked.Increment(ref executionStartCount);
                await Task.Delay(TimeSpan.FromMilliseconds(200)).ConfigureAwait(false);
                return DateTime.UtcNow;
            }
        }

        [Fact]
        public void WithUpdateFunc_ValueIsUpdatedCorrectly()
        {
            var cachedObject = CachedObjectFactory
                .ConfigureFor(() => 1)
                .WithUpdates<int>((current, input) => current + input)
                .Build();

            cachedObject.Value.Should().Be(1);

            for (var i = 2; i < 100; i++)
            {
                cachedObject.UpdateValue(1);
                cachedObject.Value.Should().Be(i);
                cachedObject.Version.Should().Be(i);
            }
        }

        [Fact]
        public async Task UpdateAsync_RefreshValueAsync_UnderlyingTasksAlwaysRunOneAtATime()
        {
            var concurrentExecutions = 0;
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(RefreshValue)
                .WithUpdates<int>(UpdateValue)
                .Build();

            cachedObject.Initialize();

            var tasks = Enumerable
                .Range(0, 10)
                .SelectMany(_ => new[] { cachedObject.UpdateValueAsync(1), cachedObject.RefreshValueAsync() })
                .ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
             
            async Task<int> RefreshValue()
            {
                if (Interlocked.Increment(ref concurrentExecutions) > 1)
                    throw new Exception();
                
                await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

                Interlocked.Decrement(ref concurrentExecutions);

                return 0;
            }

            async Task<int> UpdateValue(int current, int input)
            {
                if (Interlocked.Increment(ref concurrentExecutions) > 1)
                    throw new Exception();
                
                await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

                Interlocked.Decrement(ref concurrentExecutions);

                return current + input;
            }
        }
        
        [Fact]
        public async Task UpdateAsync_RefreshValueAsync_RefreshesTakePriority()
        {
            var lockObj = new object();
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(RefreshValue)
                .WithUpdates<int>(UpdateValue)
                .Build();

            cachedObject.Initialize();
            
            var tasksInOrderOfCompletion = new List<(Task Task, bool IsRefresh)>();
            
            var updates = Enumerable.Range(0, 10).Select(_ => RunTask(cachedObject.UpdateValueAsync(1), false)).ToList();
            var refreshes = Enumerable.Range(0, 10).Select(_ => RunTask(cachedObject.RefreshValueAsync(), true)).ToList();

            await Task.WhenAll(updates.Concat(refreshes)).ConfigureAwait(false);

            for (var i = 0; i < 20; i++)
            {
                if (i == 0)
                    tasksInOrderOfCompletion[i].IsRefresh.Should().BeFalse();
                else if (i <= 10)
                    tasksInOrderOfCompletion[i].IsRefresh.Should().BeTrue();
                else
                    tasksInOrderOfCompletion[i].IsRefresh.Should().BeFalse();
            }
            
            Task RunTask(Task task, bool isRefresh)
            {
                return task.ContinueWith(t =>
                {
                    lock (lockObj)
                        tasksInOrderOfCompletion.Add((t, isRefresh));
                });
            }
                
            static async Task<int> RefreshValue()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

                return 0;
            }

            static async Task<int> UpdateValue(int current, int input)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

                return current + input;
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValueUpdated_ActionCalledAsExpected(bool addedPreBuilding)
        {
            var events = new List<CachedObjectValueUpdatedEvent<int, int>>();
            
            var config = CachedObjectFactory
                .ConfigureFor(() => 1)
                .WithUpdates<int>(async (current, input) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    return current + input;
                });

            if (addedPreBuilding)
                config.OnValueUpdated(events.Add);

            var cachedObject = config.Build();

            if (!addedPreBuilding)
                cachedObject.OnValueUpdated += (_, e) => events.Add(e);

            var start = DateTime.UtcNow;
            
            await cachedObject.InitializeAsync();

            var previousValue = cachedObject.Value;
            
            for (var i = 0; i < 10; i++)
            {
                cachedObject.UpdateValue(i);

                events.Should().HaveCount(i + 1);
                
                var e = events[i];
                e.NewValue.Should().Be(previousValue + i);
                e.PreviousValue.Should().Be(previousValue);
                e.UpdateFuncInput.Should().Be(i);
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.DateOfPreviousSuccessfulRefresh.Should().BeCloseTo(start);
                e.Version.Should().Be(i + 2);

                previousValue += i;
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValueUpdateException_ActionCalledAsExpected(bool addedPreBuilding)
        {
            var events = new List<CachedObjectValueUpdateExceptionEvent<int, int>>();
            var updateIndex = 0;
            
            var config = CachedObjectFactory
                .ConfigureFor(() => 1)
                .WithUpdates<int>(async (current, input) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    
                    if (updateIndex++ % 2 == 1)
                        throw new Exception("error!");

                    return current + input;
                });

            if (addedPreBuilding)
                config.OnValueUpdateException(events.Add);

            var cachedObject = config.Build();

            if (!addedPreBuilding)
                cachedObject.OnValueUpdateException += (_, e) => events.Add(e);

            var start = DateTime.UtcNow;
            
            cachedObject.Initialize();

            var expectedValue = 1;
            
            Func<Task> func = () => cachedObject.UpdateValueAsync(1);

            for (var i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    await func.Should().NotThrowAsync();
                    expectedValue++;
                    continue;
                }

                await func.Should().ThrowAsync<Exception>();

                var e = events[(i - 1) / 2];
                e.Exception.Message.Should().Be("error!");
                e.CurrentValue.Should().Be(expectedValue);
                e.UpdateFuncInput.Should().Be(1);
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.DateOfPreviousSuccessfulRefresh.Should().BeCloseTo(start);
                e.Version.Should().Be(1 + ((i + 1) / 2));
            }
        }
    }
}