using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class WithUpdates
    {
        private readonly CachedObjectSetupLock _setupLock;

        public WithUpdates(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task UpdatesAreApplied()
        {
            var updateResults = new List<CachedObjectUpdateResult<List<int>, int>>();
            var updatesObservable = new Subject<int>();
            
            ICachedObject<List<int>> listOfInts;
            using (_setupLock.Enter())
            {
                listOfInts = CachedObjectFactory
                    .ConfigureFor(() => new List<int>())
                    .WithUpdates(updatesObservable, (curr, next) => curr.Concat(new [] { next }).ToList())
                    .OnUpdate(updateResults.Add)
                    .Build();
            }

            await listOfInts.Initialize();

            updateResults.Should().HaveCount(1);
            listOfInts.Value.Should().BeEmpty();

            for (var i = 1; i < 10; i++)
            {
                updatesObservable.OnNext(i);

                await Task.Delay(1);

                updateResults.Should().HaveCount(i + 1);
                listOfInts.Value.Should().BeEquivalentTo(Enumerable.Range(1, i));
            }
        }

        [Fact]
        public async Task InitialiseValueFuncIsOnlyCalledOnce()
        {
            var updatesObservable = new Subject<int>();
            var counter = 0;
            
            ICachedObject<List<int>> listOfInts;
            using (_setupLock.Enter())
            {
                listOfInts = CachedObjectFactory
                    .ConfigureFor(() =>
                    {
                        counter++;
                        return new List<int>();
                    })
                    .WithUpdates(updatesObservable, (curr, next) => curr.Concat(new [] { next }).ToList())
                    .Build();
            }

            await listOfInts.Initialize();

            counter.Should().Be(1);
            listOfInts.Value.Should().BeEmpty();

            for (var i = 1; i < 10; i++)
            {
                updatesObservable.OnNext(i);

                await Task.Delay(1);

                counter.Should().Be(1);
            }
        }
        
        [Fact]
        public async Task NotificationsContainTheUpdates()
        {
            var updateResults = new List<CachedObjectUpdateResult<List<int>, int>>();
            var updatesObservable = new Subject<int>();
            
            ICachedObject<List<int>> listOfInts;
            using (_setupLock.Enter())
            {
                listOfInts = CachedObjectFactory
                    .ConfigureFor(() => new List<int>())
                    .WithUpdates(updatesObservable, (curr, next) => curr.Concat(new [] { next }).ToList())
                    .OnUpdate(updateResults.Add)
                    .Build();
            }

            await listOfInts.Initialize();

            updateResults.Should().HaveCount(1);
            updateResults.Single().Updates.Should().Be(default);

            for (var i = 1; i < 10; i++)
            {
                updatesObservable.OnNext(i);

                await Task.Delay(1);

                updateResults.Last().Updates.Should().Be(i);
            }
        }
        
        [Fact]
        public async Task OnlyOneUpdateRunningAtATime()
        {
            var countdown = new CountdownEvent(10);
            var updatesObservable = new Subject<int>();

            var currentlyRunningUpdatesCount = 0;
            
            ICachedObject<List<int>> listOfInts;
            using (_setupLock.Enter())
            {
                listOfInts = CachedObjectFactory
                    .ConfigureFor(() => new List<int>())
                    .WithUpdates(updatesObservable, async (curr, next) =>
                    {
                        if (Interlocked.Increment(ref currentlyRunningUpdatesCount) == 2)
                            throw new Exception();

                        await Task.Delay(10);

                        Interlocked.Decrement(ref currentlyRunningUpdatesCount);
                        
                        return curr.Concat(new[] {next}).ToList();
                    })
                    .OnUpdate(r => countdown.Signal())
                    .Build();
            }

            await listOfInts.Initialize();

            for (var i = 1; i < 10; i++)
                updatesObservable.OnNext(i);

            countdown.Wait(1000).Should().BeTrue();
        }
    }
}