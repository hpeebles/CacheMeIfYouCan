using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class Configuration
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NotificationsEnabledChangesExceptionType(bool enableNotifications)
        {
            var cache = new TestCacheFactory(error: () => true)
                .Configure(c => c.WithNotificationsEnabled(enableNotifications))
                .Build(new DistributedCacheFactoryConfig<string, string>());

            var exceptionType = enableNotifications
                ? typeof(CacheException<string>)
                : typeof(Exception);
            
            await Assert.ThrowsAsync(exceptionType, () => cache.Get(new Key<string>("123", "123")));
        }
    }
}