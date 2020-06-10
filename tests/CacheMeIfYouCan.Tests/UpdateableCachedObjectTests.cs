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
    public class UpdateableCachedObjectTests : CachedObjectTests
    {
        protected override ICachedObject<T> BuildCachedObject<T>(ICachedObjectConfigurationManager<T> config)
        {
            return config
                .WithUpdates<bool>((current, input) => current)
                .Build();
        }

        [Fact]
        public void WithUpdateFunc_ValueIsUpdatedCorrectly()
        {
            using var cachedObject = CachedObjectFactory
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
                .WithUpdatesAsync<int>(UpdateValue)
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
                .WithUpdatesAsync<int>(UpdateValue)
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
            var events = new List<ValueUpdatedEvent<int, int>>();
            
            var config = CachedObjectFactory
                .ConfigureFor(() => 1)
                .WithUpdatesAsync<int>(async (current, input) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    return current + input;
                });

            if (addedPreBuilding)
                config.OnValueUpdate(events.Add);

            var cachedObject = config.Build();

            if (!addedPreBuilding)
                cachedObject.OnValueUpdated += (_, e) => events.Add(e);

            await cachedObject.InitializeAsync();

            var previousValue = cachedObject.Value;
            
            for (var i = 0; i < 10; i++)
            {
                cachedObject.UpdateValue(i);

                events.Should().HaveCount(i + 1);
                
                var e = events[i];
                e.NewValue.Should().Be(previousValue + i);
                e.PreviousValue.Should().Be(previousValue);
                e.Updates.Should().Be(i);
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.Version.Should().Be(i + 2);

                previousValue += i;
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValueUpdateException_ActionCalledAsExpected(bool addedPreBuilding)
        {
            var events = new List<ValueUpdateExceptionEvent<int, int>>();
            var updateIndex = 0;
            
            var config = CachedObjectFactory
                .ConfigureFor(() => 1)
                .WithUpdatesAsync<int>(async (current, input) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    
                    if (updateIndex++ % 2 == 1)
                        throw new Exception("error!");

                    return current + input;
                });

            if (addedPreBuilding)
                config.OnValueUpdate(onException: events.Add);

            var cachedObject = config.Build();

            if (!addedPreBuilding)
                cachedObject.OnValueUpdateException += (_, e) => events.Add(e);

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
                e.Updates.Should().Be(1);
                e.Duration.Should().BePositive().And.BeLessThan(TimeSpan.FromMilliseconds(200));
                e.Version.Should().Be(1 + ((i + 1) / 2));
            }
        }
    }
}