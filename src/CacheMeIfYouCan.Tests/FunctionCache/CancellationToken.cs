using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class WithCancellationToken
    {
        private readonly CacheSetupLock _setupLock;

        public WithCancellationToken(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SingleKey(bool cancel)
        {
            Func<string, CancellationToken, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            Func<string, CancellationToken, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .Build();
            }

            var cts = new CancellationTokenSource(cancel ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMinutes(1));
            
            Func<Task<string>> func = () => cachedEcho("abc", cts.Token);

            if (cancel)
            {
                (await func.Should().ThrowExactlyAsync<FunctionCacheGetException<string>>())
                    .WithInnerExceptionExactly<FunctionCacheFetchException<string>>()
                    .WithInnerExceptionExactly<TaskCanceledException>();
            }
            else
            {
                await func.Should().NotThrowAsync();
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnumerableKey(bool cancel)
        {
            Func<IEnumerable<string>, CancellationToken, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.FromSeconds(1));
            Func<IEnumerable<string>, CancellationToken, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .Build();
            }

            var cts = new CancellationTokenSource(cancel ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMinutes(1));
            
            Func<Task<IDictionary<string, string>>> func = () => cachedEcho(new[] { "1", "2" }, cts.Token);

            if (cancel)
            {
                (await func.Should().ThrowExactlyAsync<FunctionCacheGetException<string>>())
                    .WithInnerExceptionExactly<FunctionCacheFetchException<string>>()
                    .WithInnerExceptionExactly<TaskCanceledException>();
            }
            else
            {
                await func.Should().NotThrowAsync();
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MultiParamEnumerableKey(bool cancel)
        {
            Func<string, IEnumerable<string>, CancellationToken, Task<IDictionary<string, string>>> inputFunc = async (k1, k2, t) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1), t);
                return k2.ToDictionary(k => k);
            };
            
            Func<string, IEnumerable<string>, CancellationToken, Task<IDictionary<string, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = inputFunc
                    .Cached<string, IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .Build();
            }

            var cts = new CancellationTokenSource(cancel ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMinutes(1));
            
            Func<Task<IDictionary<string, string>>> func = () => cachedFunc("123", new[] { "1", "2" }, cts.Token);

            if (cancel)
            {
                (await func.Should().ThrowExactlyAsync<FunctionCacheGetException<(string, string)>>())
                    .WithInnerExceptionExactly<FunctionCacheFetchException<(string, string)>>()
                    .WithInnerExceptionExactly<TaskCanceledException>();
            }
            else
            {
                await func.Should().NotThrowAsync();
            }
        }
    }
}