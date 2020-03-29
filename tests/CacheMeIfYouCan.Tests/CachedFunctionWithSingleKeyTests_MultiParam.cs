using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public partial class CachedFunctionWithSingleKeyTests
    {
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With1Param_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, CancellationToken, Task<int>> originalFunction = (p, cancellationToken) => Task.FromResult(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, CancellationToken.None).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
                case "async":
                {
                    Func<int, Task<int>> originalFunction = Task.FromResult;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, CancellationToken, int> originalFunction = (p, cancellationToken) => p;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, CancellationToken.None).Should().Be(1);
                    break;
                }
                case "sync":
                {
                    Func<int, int> originalFunction = p => p;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1).Should().Be(1);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, CancellationToken, ValueTask<int>> originalFunction = (p, cancellationToken) => new ValueTask<int>(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, CancellationToken.None).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
                case "valuetask":
                {
                    Func<int, ValueTask<int>> originalFunction = p => new ValueTask<int>(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
            }

            cache.TryGet("1", out var value).Should().BeTrue();
            value.Should().Be(1);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With2Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, cancellationToken) => Task.FromResult(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, CancellationToken.None).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
                case "async":
                {
                    Func<int, int, Task<int>> originalFunction = (p1, p2) => Task.FromResult(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, int> originalFunction = (p1, p2, cancellationToken) => p1 + p2;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, CancellationToken.None).Should().Be(3);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int> originalFunction = (p1, p2) => p1 + p2;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2).Should().Be(3);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, cancellationToken) => new ValueTask<int>(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, CancellationToken.None).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, ValueTask<int>> originalFunction = (p1, p2) => new ValueTask<int>(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(3);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With3Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, cancellationToken) => Task.FromResult(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, CancellationToken.None).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, Task<int>> originalFunction = (p1, p2, p3) => Task.FromResult(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, cancellationToken) => p1 + p2 + p3;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, CancellationToken.None).Should().Be(6);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int> originalFunction = (p1, p2, p3) => p1 + p2 + p3;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3).Should().Be(6);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, cancellationToken) => new ValueTask<int>(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, CancellationToken.None).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3) => new ValueTask<int>(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(6);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With4Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, CancellationToken.None).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4) => Task.FromResult(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, cancellationToken) => p1 + p2 + p3 + p4;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, CancellationToken.None).Should().Be(10);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int> originalFunction = (p1, p2, p3, p4) => p1 + p2 + p3 + p4;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4).Should().Be(10);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, CancellationToken.None).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4) => new ValueTask<int>(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(10);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With5Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5) => Task.FromResult(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => p1 + p2 + p3 + p4 + p5;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).Should().Be(15);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5).Should().Be(15);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(15);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With6Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).Should().Be(21);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6).Should().Be(21);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(21);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With7Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6 + p7;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).Should().Be(28);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7).Should().Be(28);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(28);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With8Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).ConfigureAwait(false)).Should().Be(36);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).ConfigureAwait(false)).Should().Be(36);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).Should().Be(36);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).Should().Be(36);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).Result.Should().Be(36);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).Result.Should().Be(36);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(36);
        }
    }
}