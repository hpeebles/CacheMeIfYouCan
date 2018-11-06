using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class General
    {
        [Fact]
        public async Task RefreshedValueIsImmediatelyExposed()
        {
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build(false);

            await date.Init();

            var first = date.Value;

            DateTime next;
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                
                next = date.Value;
            }
            while (next == first);
            
            Assert.True(DateTime.UtcNow - next < TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task ContinuesToRefreshAfterException()
        {
            var index = 0;
            
            var date = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    if (index++ == 2)
                        throw new Exception();
                    
                    return DateTime.UtcNow;
                })
                .WithRefreshInterval(TimeSpan.FromMilliseconds(100))
                .Build(false);

            await date.Init();

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            Assert.True(DateTime.UtcNow - date.Value < TimeSpan.FromMilliseconds(200));
        }

        [Fact]
        public void CannotRegisterTwoOfTheSameType()
        {
            CachedObjectFactory
                .ConfigureFor(Guid.NewGuid)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            Assert.Throws<Exception>(() => CachedObjectFactory
                .ConfigureFor(Guid.NewGuid)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build());
        }
    }
}