using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class Notifications
    {
        private readonly CachedObjectSetupLock _setupLock;

        public Notifications(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task OnValueUpdated()
        {
            var updateResults = new List<CachedObjectSuccessfulUpdateResult<DateTime>>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMilliseconds(200))
                    .OnValueUpdated(updateResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            updateResults.Count.Should().BeGreaterThan(2);

            foreach (var result in updateResults)
            {
                result.Start.Should().BeAfter(DateTime.MinValue);
                result.NewValue.Should().BeAfter(DateTime.MinValue);
                result.Duration
                    .Should().BeGreaterThan(TimeSpan.FromTicks(1))
                    .And.BeLessThan(TimeSpan.FromMilliseconds(10));
            }
        }
        
        [Fact]
        public void OnException()
        {
            var exceptions = new List<CachedObjectUpdateException>();
            
            ICachedObject<DateTime, Unit> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(Throw)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .OnException(exceptions.Add)
                    .Build();
            }

            date.Initialize();

            date.Dispose();
            
            exceptions.Should().ContainSingle();

            DateTime Throw() => throw new Exception();
        }
        
        [Fact]
        public void ActionsCanBeCombined()
        {
            var updateResults1 = new List<CachedObjectSuccessfulUpdateResult>();
            var updateResults2 = new List<CachedObjectSuccessfulUpdateResult>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.CachedObject.OnValueUpdated(updateResults1.Add);
                
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .OnValueUpdated(updateResults2.Add)
                    .Build();

                DefaultSettings.CachedObject.OnValueUpdated(null, AdditionBehaviour.Overwrite);
            }

            date.Initialize();

            date.Dispose();
            
            updateResults1.Should().ContainSingle();
            updateResults2.Should().ContainSingle();
        }
        
        [Fact]
        public void OnValueUpdatedEvent()
        {
            var updateResults = new List<CachedObjectSuccessfulUpdateResult>();
            
            ICachedObject<DateTime, Unit> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            date.OnValueUpdated += (sender, args) => updateResults.Add(args.Result);
            
            date.Initialize();

            date.Dispose();
            
            updateResults.Should().ContainSingle();
        }
        
        [Fact]
        public void OnExceptionEvent()
        {
            var exceptions = new List<CachedObjectUpdateException>();
            
            ICachedObject<DateTime, Unit> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(Throw)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            date.OnException += (sender, args) => exceptions.Add(args.Exception);
            
            date.Initialize();

            date.Dispose();
            
            exceptions.Should().ContainSingle();

            DateTime Throw() => throw new Exception();
        }
        
        [Fact]
        public void OnValueUpdatedEventOnBaseInterface()
        {
            var updateResults = new List<CachedObjectSuccessfulUpdateResult>();
            var refreshTrigger = new Subject<Unit>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .RefreshOnEach(refreshTrigger)
                    .Build();
            }

            EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<DateTime>> handler = (obj, args) => updateResults.Add(args.Result);

            date.OnValueUpdated += handler;
            
            date.Initialize();
            
            updateResults.Should().ContainSingle();
            
            refreshTrigger.OnNext(Unit.Instance);

            updateResults.Should().HaveCount(2);

            date.OnValueUpdated -= handler;
            
            refreshTrigger.OnNext(Unit.Instance);

            updateResults.Should().HaveCount(2);
        }
        
        [Fact]
        public async Task OnExceptionEventOnBaseInterface()
        {
            var exceptions = new List<CachedObjectUpdateException>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(Throw)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            EventHandler<CachedObjectUpdateExceptionEventArgs<DateTime>> handler = (obj, args) => exceptions.Add(args.Exception);

            date.OnException += handler;
            
            Func<Task> func = () => date.Initialize();

            await func.Should().ThrowAsync<Exception>();
            
            exceptions.Should().ContainSingle();
            
            date.OnException -= handler;
            
            await func.Should().ThrowAsync<Exception>();

            exceptions.Should().ContainSingle();
            
            DateTime Throw() => throw new Exception();
        }
    }
}