using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class General
    {
        private readonly Random _rng = new Random();
        private readonly CacheSetupLock _setupLock;

        public General(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task ReturnsExpectedValue()
        {
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            var randomString = Guid.NewGuid().ToString();
            Assert.Equal(randomString, await proxy.StringToString(randomString));

            var randomInt = _rng.Next(1000);
            Assert.Equal(randomInt.ToString(), await proxy.IntToString(randomInt));

            var randomLong = _rng.Next(1000);
            Assert.Equal(randomLong * 2, await proxy.LongToInt(randomLong));
        }

        [Fact]
        public async Task IsCached()
        {
            FunctionCacheGetResult lastResult = null;
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(r => lastResult = r)
                    .Build();
            }

            for (var i = 0; i < 10; i++)
            {
                var randomString = Guid.NewGuid().ToString();

                for (var j = 0; j < 10; j++)
                {
                    Assert.Equal(randomString, await proxy.StringToString(randomString));
                    
                    Assert.Equal(j == 0 ? Outcome.Fetch : Outcome.FromCache, lastResult.Results.Single().Outcome);
                }
            }
        }

        [Fact]
        public async Task WorksForAsyncFunctions()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();
            
            Assert.Equal(key, await proxy.StringToString(key));

            results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results.Single().Results.Single().Outcome);

            await proxy.StringToString(key);
            
            Assert.Equal(2, results.Count);
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);
        }
        
        [Fact]
        public void WorksForSyncFunctions()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();
            
            Assert.Equal(key, proxy.StringToStringSync(key));

            results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results.Single().Results.Single().Outcome);

            proxy.StringToStringSync(key);
            
            Assert.Equal(2, results.Count);
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);
        }
    }
}