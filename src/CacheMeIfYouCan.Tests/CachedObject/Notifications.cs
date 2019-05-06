using System;
using System.Collections.Generic;
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
        public async Task OnRefreshResult()
        {
            var refreshResults = new List<CachedObjectUpdateResult<DateTime, Unit>>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMilliseconds(200))
                    .OnUpdate(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            refreshResults.Count.Should().BeGreaterThan(2);

            foreach (var result in refreshResults)
            {
                result.Start.Should().BeAfter(DateTime.MinValue);
                result.Success.Should().BeTrue();
                result.NewValue.Should().BeAfter(DateTime.MinValue);
                result.Duration
                    .Should().BeGreaterThan(TimeSpan.FromTicks(1))
                    .And.BeLessThan(TimeSpan.FromMilliseconds(10));
            }
        }
        
        [Fact]
        public void ActionsCanBeCombined()
        {
            var updateResults1 = new List<CachedObjectUpdateResult>();
            var updateResults2 = new List<CachedObjectUpdateResult>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.CachedObject.OnUpdate(updateResults1.Add);
                
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .OnUpdate(updateResults2.Add)
                    .Build();

                DefaultSettings.CachedObject.OnUpdate(null, AdditionBehaviour.Overwrite);
            }

            date.Initialize();

            date.Dispose();
            
            updateResults1.Should().ContainSingle();
            updateResults2.Should().ContainSingle();
        }
        
        [Fact]
        public void OnUpdateEvent()
        {
            var updateResults = new List<CachedObjectUpdateResult>();
            
            ICachedObject<DateTime, Unit> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            date.OnUpdate += (sender, args) => updateResults.Add(args.Result);
            
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
    }
}