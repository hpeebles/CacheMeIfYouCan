using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class RedisConnectionTests
    {
        [Fact]
        public void ResetConnectionSucceeds()
        {
            var connection = new RedisConnection(TestConnectionString.Value);

            connection.Connect().Should().BeTrue();
            
            connection.Get().Dispose();

            connection.Get().IsConnected.Should().BeFalse();

            connection.Reset(false).Should().BeTrue();

            connection.Get().IsConnected.Should().BeTrue();
        }
        
        [Fact]
        public async Task SubscriptionsAreRestoredAfterReset()
        {
            using var connection = new RedisConnection(TestConnectionString.Value);

            var keyPrefix = Guid.NewGuid().ToString();
            using var cache1 = BuildCache(connection, keyPrefix);
            using var cache2 = BuildCache(connection, keyPrefix);

            using var signal = new AutoResetEvent(false);

            cache1.KeysChangedRemotely.Subscribe(_ => signal.Set());

            await cache2.Set(1, 1, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            signal.WaitOne(TimeSpan.FromSeconds(5)).Should().BeTrue();
            
            connection.Get().Dispose();

            connection.Reset();

            await cache2.Set(2, 2, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            signal.WaitOne(TimeSpan.FromSeconds(5)).Should().BeTrue();
        }

        private static RedisCache<int, int> BuildCache(RedisConnection connection, string keyPrefix)
        {
            return new RedisCache<int, int>(
                connection,
                k => k.ToString(),
                v => v.ToString(),
                v => Int32.Parse(v),
                keyPrefix: keyPrefix,
                subscriber: connection,
                keyEventTypesToSubscribeTo: KeyEventType.All);
        }
    }
}