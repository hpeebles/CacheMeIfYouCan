using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
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
            ITest impl = new TestImpl(TimeSpan.FromSeconds(1));
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            var token = cancel
                ? new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token
                : CancellationToken.None;

            Func<Task> func = () => proxy.StringToStringCanx(Guid.NewGuid().ToString(), token);

            if (cancel)
                await func.Should().ThrowAsync<FunctionCacheException>();
            else
                await func.Should().NotThrowAsync();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MultiKey(bool cancel)
        {
            ITest impl = new TestImpl(TimeSpan.FromSeconds(1));
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            var token = cancel
                ? new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token
                : CancellationToken.None;

            Func<Task> func = () => proxy.MultiEchoCanx(new[] { Guid.NewGuid().ToString() }, token);

            if (cancel)
                await func.Should().ThrowAsync<FunctionCacheException>();
            else
                await func.Should().NotThrowAsync();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MultiParam(bool cancel)
        {
            ITest impl = new TestImpl(TimeSpan.FromSeconds(1));
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            var token = cancel
                ? new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token
                : CancellationToken.None;

            Func<Task> func = () => proxy.MultiParamEchoCanx(Guid.NewGuid().ToString(), 1, token);

            if (cancel)
                await func.Should().ThrowAsync<FunctionCacheException>();
            else
                await func.Should().NotThrowAsync();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MultiParamEnumerableKey(bool cancel)
        {
            ITest impl = new TestImpl(TimeSpan.FromSeconds(1));
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            var token = cancel
                ? new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token
                : CancellationToken.None;

            Func<Task> func = () => proxy.MultiParamEnumerableKeyCanx(Guid.NewGuid().ToString(), new[] { 1 }, token);

            if (cancel)
                await func.Should().ThrowAsync<FunctionCacheException>();
            else
                await func.Should().NotThrowAsync();
        }
    }
}