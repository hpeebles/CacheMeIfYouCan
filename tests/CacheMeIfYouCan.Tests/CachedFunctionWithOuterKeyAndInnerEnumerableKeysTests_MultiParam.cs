using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public partial class CachedFunctionWithOuterKeyAndInnerEnumerableKeysTests
    {
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With2Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, cancellationToken) => Task.FromResult(p2.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .UseFirstParamAsOuterCacheKey()
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2) => Task.FromResult(p2.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .UseFirstParamAsOuterCacheKey()
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, cancellationToken) => p2.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .UseFirstParamAsOuterCacheKey()
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2) => p2.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .UseFirstParamAsOuterCacheKey()
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, cancellationToken) => new ValueTask<Dictionary<int, int>>(p2.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .UseFirstParamAsOuterCacheKey()
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2) => new ValueTask<Dictionary<int, int>>(p2.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .UseFirstParamAsOuterCacheKey()
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            cache.GetMany(1, input).Should().BeEquivalentTo(expectedOutput);
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
            var cache = new MockLocalCache<int, int, int>();
        
            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, cancellationToken) => Task.FromResult(p3.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2) => p1 + p2)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3) => Task.FromResult(p3.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2) => p1 + p2)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, cancellationToken) => p3.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2) => p1 + p2)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3) => p3.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2) => p1 + p2)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, cancellationToken) => new ValueTask<Dictionary<int, int>>(p3.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2) => p1 + p2)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3) => new ValueTask<Dictionary<int, int>>(p3.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2) => p1 + p2)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            cache.GetMany(3, input).Should().BeEquivalentTo(expectedOutput);
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
            var cache = new MockLocalCache<int, int, int>();
        
            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, cancellationToken) => Task.FromResult(p4.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3) => p1 + p2 + p3)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4) => Task.FromResult(p4.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3) => p1 + p2 + p3)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => p4.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3) => p1 + p2 + p3)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4) => p4.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3) => p1 + p2 + p3)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, cancellationToken) => new ValueTask<Dictionary<int, int>>(p4.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3) => p1 + p2 + p3)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4) => new ValueTask<Dictionary<int, int>>(p4.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3) => p1 + p2 + p3)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }
        
            cache.GetMany(6, input).Should().BeEquivalentTo(expectedOutput);
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
            var cache = new MockLocalCache<int, int, int>();
        
            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => Task.FromResult(p5.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4) => p1 + p2 + p3 + p4)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5) => Task.FromResult(p5.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4) => p1 + p2 + p3 + p4)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => p5.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4) => p1 + p2 + p3 + p4)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5) => p5.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4) => p1 + p2 + p3 + p4)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => new ValueTask<Dictionary<int, int>>(p5.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4) => p1 + p2 + p3 + p4)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5) => new ValueTask<Dictionary<int, int>>(p5.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4) => p1 + p2 + p3 + p4)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }
        
            cache.GetMany(10, input).Should().BeEquivalentTo(expectedOutput);
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
            var cache = new MockLocalCache<int, int, int>();
        
            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => Task.FromResult(p6.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, 5, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6) => Task.FromResult(p6.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, 5, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => p6.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, 5, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6) => p6.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, 5, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => new ValueTask<Dictionary<int, int>>(p6.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6) => new ValueTask<Dictionary<int, int>>(p6.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }
        
            cache.GetMany(15, input).Should().BeEquivalentTo(expectedOutput);
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
            var cache = new MockLocalCache<int, int, int>();
        
            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => Task.FromResult(p7.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, 5, 6, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => Task.FromResult(p7.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, 5, 6, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => p7.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, 5, 6, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => p7.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, 5, 6, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => new ValueTask<Dictionary<int, int>>(p7.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => new ValueTask<Dictionary<int, int>>(p7.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }
        
            cache.GetMany(21, input).Should().BeEquivalentTo(expectedOutput);
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
            var cache = new MockLocalCache<int, int, int>();
        
            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => Task.FromResult(p8.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => Task.FromResult(p8.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => p8.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, 5, 6, 7, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => p8.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();
        
                    cachedFunction(1, 2, 3, 4, 5, 6, 7, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, CancellationToken, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => new ValueTask<Dictionary<int, int>>(p8.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, input, CancellationToken.None).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, ValueTask<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => new ValueTask<Dictionary<int, int>>(p8.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithEnumerableKeys<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                        .WithOuterCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, input).Result.Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }
        
            cache.GetMany(28, input).Should().BeEquivalentTo(expectedOutput);
        }
    }
}