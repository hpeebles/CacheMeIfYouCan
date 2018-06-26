using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class KeysToKeepAlive
    {
        [Fact]
        public async Task AllKeysAreKeptInCache()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new ConcurrentBag<FunctionCacheGetResult>();

            var keys = Enumerable
                .Range(0, 100)
                .Select(id => id.ToString())
                .ToList();
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromSeconds(2))
                .OnResult(results.Add)
                .WithKeysToKeepAlive(keys)
                .Build();

            await Task.Delay(TimeSpan.FromSeconds(1));

            for (var i = 0; i < 10; i++)
            {
                var tasks = keys
                    .Select(cachedEcho)
                    .ToList();

                await Task.WhenAll(tasks);

                Assert.Equal(tasks.Count, results.Count);
                Assert.True(results.All(r => r.Outcome == Outcome.FromCache));

                results.Clear();
                
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task WhenKeysToKeepAliveChangeTheNewKeysAreKeptInCache()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new ConcurrentBag<FunctionCacheGetResult>();

            var index = 0;
            
            Func<IList<string>> keysFunc = () => Enumerable
                .Range(index * 100, 100)
                .Select(id => id.ToString())
                .ToList();
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromSeconds(2))
                .OnResult(results.Add)
                .WithKeysToKeepAlive(keysFunc)
                .Build();

            await Task.Delay(TimeSpan.FromSeconds(1));

            for (var i = 0; i < 5; i++)
            {
                var keys = keysFunc();

                var tasks = keys
                    .Select(cachedEcho)
                    .ToList();

                await Task.WhenAll(tasks);

                Assert.Equal(tasks.Count, results.Count);
                Assert.True(results.All(r => r.Outcome == Outcome.FromCache));

                results.Clear();

                index++;

                await Task.Delay(TimeSpan.FromSeconds(5));

                tasks = keys
                    .Select(cachedEcho)
                    .ToList();

                await Task.WhenAll(tasks);

                Assert.Equal(tasks.Count, results.Count);
                Assert.True(results.All(r => r.Outcome == Outcome.Fetch));

                results.Clear();
            }
        }
    }
}