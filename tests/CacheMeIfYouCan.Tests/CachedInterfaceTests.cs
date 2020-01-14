using System;
using System.Collections.Generic;
using System.Linq;
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
            var originalImpl = new DummySingleKeyInterfaceImpl();

            var cache1 = new MockLocalCache<int, int>();
            var cache2 = new MockLocalCache<int, int>();
            var cache3 = new MockLocalCache<int, int>();
            var cache4 = new MockLocalCache<int, int>();
            
            var cachedInterface = CachedInterfaceFactory.For<IDummySingleKeyInterface>(originalImpl)
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
        
        [Fact]
        public void EnumerableKey_WorksForAllMethodTypes()
        {
            var originalImpl = new DummyEnumerableKeyInterfaceImpl();

            var cache1 = new MockLocalCache<int, int>();
            var cache2 = new MockLocalCache<int, int>();
            var cache3 = new MockLocalCache<int, int>();
            var cache4 = new MockLocalCache<int, int>();
            
            var cachedInterface = CachedInterfaceFactory.For<IDummyEnumerableKeyInterface>(originalImpl)
                .Configure<int, int, IEnumerable<int>, Dictionary<int, int>>(x => x.GetAsync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache1))
                .Configure<int, int, IEnumerable<int>, Dictionary<int, int>>(x => x.GetSync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache2))
                .Configure<int, int, IEnumerable<int>, Dictionary<int, int>>(x => x.GetAsyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache3))
                .Configure<int, int, IEnumerable<int>, Dictionary<int, int>>(x => x.GetSyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache4))
                .Build();

            cachedInterface.GetAsync(new[] { 1 }).Result.Single().Should().Be(new KeyValuePair<int, int>(1, 1));
            cache1.TryGet(1, out _).Should().BeTrue();

            cachedInterface.GetSync(new[] { 2 }).Single().Should().Be(new KeyValuePair<int, int>(2, 2));
            cache2.TryGet(2, out _).Should().BeTrue();
            
            cachedInterface.GetAsyncCanx(new[] { 3 }, CancellationToken.None).Result.Single().Should().Be(new KeyValuePair<int, int>(3, 3));
            cache3.TryGet(3, out _).Should().BeTrue();

            cachedInterface.GetSyncCanx(new[] { 4 }, CancellationToken.None).Single().Should().Be(new KeyValuePair<int, int>(4, 4));
            cache4.TryGet(4, out _).Should().BeTrue();
        }
    }

    public interface IDummySingleKeyInterface
    {
        Task<int> GetAsync(int key);
        int GetSync(int key);
        Task<int> GetAsyncCanx(int key, CancellationToken cancellationToken);
        int GetSyncCanx(int key, CancellationToken cancellationToken);
    }
    
    public class DummySingleKeyInterfaceImpl : IDummySingleKeyInterface
    {
        public int GetSync(int key) => key;
        public Task<int> GetAsync(int key) => Task.FromResult(key);
        public int GetSyncCanx(int key, CancellationToken cancellationToken) => key;
        public Task<int> GetAsyncCanx(int key, CancellationToken cancellationToken) => Task.FromResult(key);
    }
    
    public interface IDummyEnumerableKeyInterface
    {
        Task<Dictionary<int, int>> GetAsync(IEnumerable<int> keys);
        Dictionary<int, int> GetSync(IEnumerable<int> keys);
        Task<Dictionary<int, int>> GetAsyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken);
        Dictionary<int, int> GetSyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken);
    }
    
    public class DummyEnumerableKeyInterfaceImpl : IDummyEnumerableKeyInterface
    {
        public Task<Dictionary<int, int>> GetAsync(IEnumerable<int> keys) => Task.FromResult(keys.ToDictionary(k => k));
        public Dictionary<int, int> GetSync(IEnumerable<int> keys) => keys.ToDictionary(k => k);
        public Task<Dictionary<int, int>> GetAsyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken) => Task.FromResult(keys.ToDictionary(k => k));

        public Dictionary<int, int> GetSyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken) => keys.ToDictionary(k => k);
    }
}