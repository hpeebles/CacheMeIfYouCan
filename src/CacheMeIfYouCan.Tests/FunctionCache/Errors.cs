using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class Errors
    {
        private readonly CacheSetupLock _setupLock;

        public Errors(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task OnExceptionIsTriggered()
        {
            var errors = new List<FunctionCacheException>();
            
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnException(errors.Add)
                    .Build();
            }

            var previousErrorCount = 0;
            for (var i = 0; i < 10; i++)
            {
                var key = Guid.NewGuid().ToString();
                
                if (i % 2 == 0)
                {
                    var exception = await Assert.ThrowsAsync<FunctionCacheGetException<string>>(() => cachedEcho(key));
                    
                    Assert.Equal(previousErrorCount += 2, errors.Count); // one for failing the fetch, one for failing the get
                    Assert.Equal(key, errors[errors.Count - 1].Keys.Single());
                    Assert.Equal(key, errors[errors.Count - 2].Keys.Single());
                    Assert.IsType<FunctionCacheFetchException<string>>(exception.InnerException);
                }
                else
                {
                    Assert.Equal(key, await cachedEcho(key));
                }
            }
        }
        
        [Fact]
        public async Task ReturnsNullIfContinueOnExceptionIsSet()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMilliseconds(1))
                    .ContinueOnException()
                    .Build();
            }

            for (var i = 0; i < 10; i++)
            {
                var result = await cachedEcho("abc");
                
                if (i % 2 == 1)
                    Assert.Equal("abc", result);
                else
                    Assert.Null(result);

                await Task.Delay(5);
            }
        }
        
        [Fact]
        public async Task ReturnsDefaultValueIfContinueOnExceptionIsSet()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMilliseconds(1))
                    .ContinueOnException("defaultValue")
                    .Build();
            }

            for (var i = 0; i < 10; i++)
            {
                var result = await cachedEcho("abc");
                
                if (i % 2 == 1)
                    Assert.Equal("abc", result);
                else
                    Assert.Equal("defaultValue", result);

                await Task.Delay(5);
            }
        }
        
        [Fact]
        public async Task CacheStillWorksForOtherKeys()
        {
            var errors = new List<FunctionCacheException>();
            var fetches = new List<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => x.Equals("error!"));
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnException(errors.Add)
                    .OnFetch(fetches.Add)
                    .Build();
            }

            var keys = new[] { "one", "error!", "two" };

            var results = new List<KeyValuePair<string, string>>();

            var loopCount = 5;
            var thrownErrorsCount = 0;

            for (var i = 0; i < loopCount; i++)
            {
                foreach (var key in keys)
                {
                    try
                    {
                        var value = await cachedEcho(key);

                        results.Add(new KeyValuePair<string, string>(key, value));
                    }
                    catch
                    {
                        thrownErrorsCount++;
                    }
                }
            }

            Assert.Equal(loopCount * 2, errors.Count);
            errors.All(k => k.Keys.Single() == "error!").Should().BeTrue();
            Assert.Equal(loopCount, thrownErrorsCount);
            Assert.Equal(loopCount, results.Count(kv => kv.Key == "one"));
            Assert.Equal(loopCount, results.Count(kv => kv.Key == "two"));
            Assert.Equal(0, results.Count(kv => kv.Key == "error!"));
            Assert.Equal(2, fetches.Count(f => f.Success));
            Assert.Equal(0, fetches.Count(f => f.Results.Single().Duplicate));
        }

        [Fact]
        public async Task CacheStillWorksForSubsequentCallsToSameKey()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromMilliseconds(10), x => count++ % 4 == 2);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMilliseconds(1))
                    .Build();
            }

            for (var i = 0; i < 20; i++)
            {
                if (i % 4 == 2)
                    await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("abc"));
                else
                    Assert.Equal("abc", await cachedEcho("abc"));

                await Task.Delay(5);
            }
        }

        [Fact]
        public async Task ExceptionAddedToOnResultNotification()
        {
            var results = new List<FunctionCacheGetResult>();

            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();
            
            Func<Task<string>> func = () => cachedEcho(key);

            await func.Should().ThrowAsync<Exception>();

            results.Single().Exception.Keys.Should().ContainSingle().Which.Should().Be(key);
        }
        
        [Fact]
        public async Task ExceptionAddedToOnFetchNotification()
        {
            var fetches = new List<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnFetch(fetches.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();
            
            Func<Task<string>> func = () => cachedEcho(key);

            await func.Should().ThrowAsync<Exception>();

            fetches.Single().Exception.Keys.Should().ContainSingle().Which.Should().Be(key);
        }
    }
}