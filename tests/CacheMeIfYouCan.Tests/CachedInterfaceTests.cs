using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class CachedInterfaceTests
    {
        [Fact]
        public void SingleKey_WorksForAllMethodTypes()
        {
            var originalImpl = new DummyInterfaceImpl();

            var cache1 = new MockLocalCache<int, int>();
            var cache2 = new MockLocalCache<int, int>();
            var cache3 = new MockLocalCache<int, int>();
            var cache4 = new MockLocalCache<int, int>();
            
            var cachedInterface = CachedInterfaceFactory.For<IDummyInterface>(originalImpl)
                .Configure<int, int>(x => x.GetAsync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache1))
                .Configure<int, int>(x => x.GetSync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache2))
                .Configure<int, int>(x => x.GetAsyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache3))
                .Configure<int, int>(x => x.GetSyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache4))
                .Build();

            cachedInterface.GetAsync(1).Result.Should().Be(1);
            cache1.TryGet(1, out _).Should().BeTrue();

            cachedInterface.GetSync(2).Should().Be(2);
            cache2.TryGet(2, out _).Should().BeTrue();
            
            cachedInterface.GetAsyncCanx(3, CancellationToken.None).Result.Should().Be(3);
            cache3.TryGet(3, out _).Should().BeTrue();

            cachedInterface.GetSyncCanx(4, CancellationToken.None).Should().Be(4);
            cache4.TryGet(4, out _).Should().BeTrue();
        }
    }

    public interface IDummyInterface
    {
        Task<int> GetAsync(int key);
        int GetSync(int key);
        Task<int> GetAsyncCanx(int key, CancellationToken cancellationToken);
        int GetSyncCanx(int key, CancellationToken cancellationToken);
    }
    
    public class DummyInterfaceImpl : IDummyInterface
    {
        public int GetSync(int key) => key;
        public Task<int> GetAsync(int key) => Task.FromResult(key);
        public int GetSyncCanx(int key, CancellationToken cancellationToken) => key;
        public Task<int> GetAsyncCanx(int key, CancellationToken cancellationToken) => Task.FromResult(key);
    }
}