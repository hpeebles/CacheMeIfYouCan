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
            var cache5 = new MockLocalCache<int, int>();
            var cache6 = new MockLocalCache<int, int>();
            
            var cachedInterface = CachedInterfaceFactory.For<IDummySingleKeyInterface>(originalImpl)
                .Configure<int, int>(x => x.GetAsync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache1))
                .Configure<int, int>(x => x.GetAsyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache2))
                .Configure<int, int>(x => x.GetSync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache3))
                .Configure<int, int>(x => x.GetSyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache4))
                .Configure<int, int>(x => x.GetValueTask, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache5))
                .Configure<int, int>(x => x.GetValueTaskCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache6))
                .Build();

            cachedInterface.GetAsync(1).Result.Should().Be(1);
            cache1.TryGet(1, out _).Should().BeTrue();
            
            cachedInterface.GetAsyncCanx(2, CancellationToken.None).Result.Should().Be(2);
            cache2.TryGet(2, out _).Should().BeTrue();

            cachedInterface.GetSync(3).Should().Be(3);
            cache3.TryGet(3, out _).Should().BeTrue();

            cachedInterface.GetSyncCanx(4, CancellationToken.None).Should().Be(4);
            cache4.TryGet(4, out _).Should().BeTrue();
            
            cachedInterface.GetValueTask(5).Result.Should().Be(5);
            cache5.TryGet(5, out _).Should().BeTrue();
            
            cachedInterface.GetValueTaskCanx(6, CancellationToken.None).Result.Should().Be(6);
            cache6.TryGet(6, out _).Should().BeTrue();
        }
        
        [Fact]
        public void EnumerableKey_WorksForAllMethodTypes()
        {
            var originalImpl = new DummyEnumerableKeyInterfaceImpl();

            var cache1 = new MockLocalCache<int, int>();
            var cache2 = new MockLocalCache<int, int>();
            var cache3 = new MockLocalCache<int, int>();
            var cache4 = new MockLocalCache<int, int>();
            var cache5 = new MockLocalCache<int, int>();
            var cache6 = new MockLocalCache<int, int>();
            
            var cachedInterface = CachedInterfaceFactory.For<IDummyEnumerableKeyInterface>(originalImpl)
                .Configure<IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetAsync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache1))
                .Configure<IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetAsyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache2))
                .Configure<IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetSync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache3))
                .Configure<IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetSyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache4))
                .Configure<IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetValueTask, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache5))
                .Configure<IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetValueTaskCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache6))
                .Build();

            cachedInterface.GetAsync(new[] { 1 }).Result.Single().Should().Be(new KeyValuePair<int, int>(1, 1));
            cache1.TryGet(1, out _).Should().BeTrue();

            cachedInterface.GetAsyncCanx(new[] { 2 }, CancellationToken.None).Result.Single().Should().Be(new KeyValuePair<int, int>(2, 2));
            cache2.TryGet(2, out _).Should().BeTrue();

            cachedInterface.GetSync(new[] { 3 }).Single().Should().Be(new KeyValuePair<int, int>(3, 3));
            cache3.TryGet(3, out _).Should().BeTrue();
            
            cachedInterface.GetSyncCanx(new[] { 4 }, CancellationToken.None).Single().Should().Be(new KeyValuePair<int, int>(4, 4));
            cache4.TryGet(4, out _).Should().BeTrue();
            
            cachedInterface.GetValueTask(new[] { 5 }).Result.Single().Should().Be(new KeyValuePair<int, int>(5, 5));
            cache5.TryGet(5, out _).Should().BeTrue();

            cachedInterface.GetValueTaskCanx(new[] { 6 }, CancellationToken.None).Result.Single().Should().Be(new KeyValuePair<int, int>(6, 6));
            cache6.TryGet(6, out _).Should().BeTrue();
        }
        
        [Fact]
        public void OuterKeyAndInnerEnumerableKey_WorksForAllMethodTypes()
        {
            var originalImpl = new DummyOuterKeyAndInnerEnumerableKeyInterfaceImpl();

            var cache1 = new MockLocalCache<int, int, int>();
            var cache2 = new MockLocalCache<int, int, int>();
            var cache3 = new MockLocalCache<int, int, int>();
            var cache4 = new MockLocalCache<int, int, int>();
            var cache5 = new MockLocalCache<int, int, int>();
            var cache6 = new MockLocalCache<int, int, int>();
            
            var cachedInterface = CachedInterfaceFactory.For<IDummyOuterKeyAndInnerEnumerableKeyInterface>(originalImpl)
                .Configure<int, IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetAsync, c => c.UseFirstParamAsOuterCacheKey().WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache1))
                .Configure<int, IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetAsyncCanx, c => c.UseFirstParamAsOuterCacheKey().WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache2))
                .Configure<int, IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetSync, c => c.UseFirstParamAsOuterCacheKey().WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache3))
                .Configure<int, IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetSyncCanx, c => c.UseFirstParamAsOuterCacheKey().WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache4))
                .Configure<int, IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetValueTask, c => c.UseFirstParamAsOuterCacheKey().WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache5))
                .Configure<int, IEnumerable<int>, Dictionary<int, int>, int, int>(x => x.GetValueTaskCanx, c => c.UseFirstParamAsOuterCacheKey().WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache6))
                .Build();

            cachedInterface.GetAsync(0, new[] { 1 }).Result.Single().Should().Be(new KeyValuePair<int, int>(1, 1));
            cache1.GetMany(0, new[] { 1 }).Should().BeEquivalentTo(new KeyValuePair<int, int>(1, 1));
            
            cachedInterface.GetAsyncCanx(0, new[] { 2 }, CancellationToken.None).Result.Single().Should().Be(new KeyValuePair<int, int>(2, 2));
            cache2.GetMany(0, new[] { 2 }).Should().BeEquivalentTo(new KeyValuePair<int, int>(2, 2));

            cachedInterface.GetSync(0, new[] { 3 }).Single().Should().Be(new KeyValuePair<int, int>(3, 3));
            cache3.GetMany(0, new[] { 3 }).Should().BeEquivalentTo(new KeyValuePair<int, int>(3, 3));
            
            cachedInterface.GetSyncCanx(0, new[] { 4 }, CancellationToken.None).Single().Should().Be(new KeyValuePair<int, int>(4, 4));
            cache4.GetMany(0, new[] { 4 }).Should().BeEquivalentTo(new KeyValuePair<int, int>(4, 4));
            
            cachedInterface.GetValueTask(0, new[] { 5 }).Result.Single().Should().Be(new KeyValuePair<int, int>(5, 5));
            cache5.GetMany(0, new[] { 5 }).Should().BeEquivalentTo(new KeyValuePair<int, int>(5, 5));
            
            cachedInterface.GetValueTaskCanx(0, new[] { 6 }, CancellationToken.None).Result.Single().Should().Be(new KeyValuePair<int, int>(6, 6));
            cache6.GetMany(0, new[] { 6 }).Should().BeEquivalentTo(new KeyValuePair<int, int>(6, 6));
        }

        [Fact]
        public void IfAnyMethodsNotConfigured_ThrowsException()
        {
            var originalImpl = new DummySingleKeyInterfaceImpl();

            var cache = new MockLocalCache<int, int>();
            
            Action action = () => CachedInterfaceFactory.For<IDummySingleKeyInterface>(originalImpl)
                .Configure<int, int>(x => x.GetAsync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache))
                .Configure<int, int>(x => x.GetAsyncCanx, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache))
                .Configure<int, int>(x => x.GetSync, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)).WithLocalCache(cache))
                .Build();
            
            action.Should().Throw<Exception>().Where(ex => ex.Message.Contains(nameof(IDummySingleKeyInterface.GetSyncCanx)));
        }
    }

    public interface IDummySingleKeyInterface
    {
        Task<int> GetAsync(int key);
        Task<int> GetAsyncCanx(int key, CancellationToken cancellationToken);
        int GetSync(int key);
        int GetSyncCanx(int key, CancellationToken cancellationToken);
        ValueTask<int> GetValueTask(int key);
        ValueTask<int> GetValueTaskCanx(int key, CancellationToken cancellationToken);
    }
    
    public class DummySingleKeyInterfaceImpl : IDummySingleKeyInterface
    {
        public Task<int> GetAsync(int key) => Task.FromResult(key);
        public Task<int> GetAsyncCanx(int key, CancellationToken cancellationToken) => Task.FromResult(key);
        public int GetSync(int key) => key;
        public int GetSyncCanx(int key, CancellationToken cancellationToken) => key;
        public ValueTask<int> GetValueTask(int key) => new ValueTask<int>(key);
        public ValueTask<int> GetValueTaskCanx(int key, CancellationToken cancellationToken) => new ValueTask<int>(key);
    }
    
    public interface IDummyEnumerableKeyInterface
    {
        Task<Dictionary<int, int>> GetAsync(IEnumerable<int> keys);
        Task<Dictionary<int, int>> GetAsyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken);
        Dictionary<int, int> GetSync(IEnumerable<int> keys);
        Dictionary<int, int> GetSyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken);
        ValueTask<Dictionary<int, int>> GetValueTask(IEnumerable<int> keys);
        ValueTask<Dictionary<int, int>> GetValueTaskCanx(IEnumerable<int> keys, CancellationToken cancellationToken);
    }
    
    public class DummyEnumerableKeyInterfaceImpl : IDummyEnumerableKeyInterface
    {
        public Task<Dictionary<int, int>> GetAsync(IEnumerable<int> keys) => Task.FromResult(keys.ToDictionary(k => k));
        public Task<Dictionary<int, int>> GetAsyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken) => Task.FromResult(keys.ToDictionary(k => k));
        public Dictionary<int, int> GetSync(IEnumerable<int> keys) => keys.ToDictionary(k => k);
        public Dictionary<int, int> GetSyncCanx(IEnumerable<int> keys, CancellationToken cancellationToken) => keys.ToDictionary(k => k);
        public ValueTask<Dictionary<int, int>> GetValueTask(IEnumerable<int> keys) => new ValueTask<Dictionary<int, int>>(keys.ToDictionary(k => k));
        public ValueTask<Dictionary<int, int>> GetValueTaskCanx(IEnumerable<int> keys, CancellationToken cancellationToken) => new ValueTask<Dictionary<int, int>>(keys.ToDictionary(k => k));
    }
    
    public interface IDummyOuterKeyAndInnerEnumerableKeyInterface
    {
        Task<Dictionary<int, int>> GetAsync(int outerKey, IEnumerable<int> innerKeys);
        Task<Dictionary<int, int>> GetAsyncCanx(int outerKey, IEnumerable<int> innerKeys, CancellationToken cancellationToken);
        Dictionary<int, int> GetSync(int outerKey, IEnumerable<int> innerKeys);
        Dictionary<int, int> GetSyncCanx(int outerKey, IEnumerable<int> innerKeys, CancellationToken cancellationToken);
        ValueTask<Dictionary<int, int>> GetValueTask(int outerKey, IEnumerable<int> innerKeys);
        ValueTask<Dictionary<int, int>> GetValueTaskCanx(int outerKey, IEnumerable<int> innerKeys, CancellationToken cancellationToken);
    }
    
    public class DummyOuterKeyAndInnerEnumerableKeyInterfaceImpl : IDummyOuterKeyAndInnerEnumerableKeyInterface
    {
        public Task<Dictionary<int, int>> GetAsync(int outerKey, IEnumerable<int> innerKeys) => Task.FromResult(innerKeys.ToDictionary(k => k));
        public Task<Dictionary<int, int>> GetAsyncCanx(int outerKey, IEnumerable<int> innerKeys, CancellationToken cancellationToken) => Task.FromResult(innerKeys.ToDictionary(k => k));
        public Dictionary<int, int> GetSync(int outerKey, IEnumerable<int> innerKeys) => innerKeys.ToDictionary(k => k);
        public Dictionary<int, int> GetSyncCanx(int outerKey, IEnumerable<int> innerKeys, CancellationToken cancellationToken) => innerKeys.ToDictionary(k => k);
        public ValueTask<Dictionary<int, int>> GetValueTask(int outerKey, IEnumerable<int> innerKeys) => new ValueTask<Dictionary<int, int>>(innerKeys.ToDictionary(k => k));
        public ValueTask<Dictionary<int, int>> GetValueTaskCanx(int outerKey, IEnumerable<int> innerKeys, CancellationToken cancellationToken) => new ValueTask<Dictionary<int, int>>(innerKeys.ToDictionary(k => k));
    }
}