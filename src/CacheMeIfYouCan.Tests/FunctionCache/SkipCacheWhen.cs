using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class SkipCacheWhen
    {
        private readonly CacheSetupLock _setupLock;

        public SkipCacheWhen(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task SingleKeySkipGet()
        {
            var results = new List<FunctionCacheGetResult<string, string>>();
            var cache = new TestLocalCache<string, string>();
            
            Func<string, Task<string>> echo = new Echo();
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k => k == "abc", SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("abc");

            results.Should().ContainSingle();
            cache.Values.Should().ContainKey("abc");

            await cachedEcho("abc");

            results.Should().HaveCount(2);
            results.Last().Results.Single().Outcome.Should().Be(Outcome.Fetch);
        }
        
        [Fact]
        public async Task SingleKeySkipSet()
        {
            var results = new List<FunctionCacheGetResult<string, string>>();
            var cache = new TestLocalCache<string, string>();
            
            Func<string, Task<string>> echo = new Echo();
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k => k == "abc", SkipCacheSettings.SkipSet)
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("abc");

            results.Should().ContainSingle();
            cache.Values.Should().BeEmpty();
        }
        
        [Fact]
        public async Task SingleKeySkipSetBasedOnKeyAndValue()
        {
            var results = new List<FunctionCacheGetResult<string, string>>();
            var cache = new TestLocalCache<string, string>();
            
            Func<string, Task<string>> echo = new Echo();
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k, v) => k == "1" || v == "2")
                    .OnResult(results.Add)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToList();
            
            foreach (var key in keys)
                await cachedEcho(key);

            cache.Values.Keys.Should().BeEquivalentTo(keys.Except(new[] { "1", "2" }));
        }

        [Fact]
        public async Task EnumerableKeySkipGet()
        {
            var results = new List<FunctionCacheGetResult<string, string>>();
            var cache = new TestLocalCache<string, string>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k => k == "1", SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();

            await cachedEcho(keys);
            
            results.Should().ContainSingle();
            cache.Values.Keys.Should().BeEquivalentTo(keys);
            
            await cachedEcho(keys);

            results.Should().HaveCount(2);

            foreach (var x in results.Last().Results)
                x.Outcome.Should().Be(x.Key.AsObject == "1" ? Outcome.Fetch : Outcome.FromCache);
        }
        
        [Fact]
        public async Task EnumerableKeySkipSet()
        {
            var cache = new TestLocalCache<string, string>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k => k == "1", SkipCacheSettings.SkipSet)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();

            await cachedEcho(keys);
            
            cache.Values.Keys.Should().BeEquivalentTo(keys.Where(k => k != "1"));
        }
        
        [Fact]
        public async Task EnumerableKeySkipSetBasedOnKeyAndValue()
        {
            var cache = new TestLocalCache<string, string>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k, v) => k == "1" || v == "2")
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();

            await cachedEcho(keys);
            
            cache.Values.Keys.Should().BeEquivalentTo(keys.Except(new[] { "1", "2" }));
        }
        
        [Fact]
        public async Task MultiParamKeySkipGet()
        {
            var results = new List<FunctionCacheGetResult<(string, string), string>>();
            var cache = new TestLocalCache<(string, string), string>();
            
            Func<string, string, Task<string>> func = (k1, k2) => Task.FromResult(k1 + k2);
            
            Func<string, string, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k1, k2) => k1 == "abc", SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedFunc("abc", "xyz");

            results.Should().ContainSingle();
            cache.Values.Should().ContainKey(("abc", "xyz"));

            await cachedFunc("abc", "xyz");

            results.Should().HaveCount(2);
            results.Last().Results.Single().Outcome.Should().Be(Outcome.Fetch);
        }
        
        [Fact]
        public async Task MultiParamKeySkipSet()
        {
            var results = new List<FunctionCacheGetResult<(string, string), string>>();
            var cache = new TestLocalCache<(string, string), string>();
            
            Func<string, string, Task<string>> func = (k1, k2) => Task.FromResult(k1 + k2);
            
            Func<string, string, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k1, k2) => k1 == "abc", SkipCacheSettings.SkipSet)
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedFunc("abc", "xyz");

            results.Should().ContainSingle();
            cache.Values.Should().BeEmpty();
        }

        [Fact]
        public async Task MultiParamKeySkipSetBasedOnKeyAndValue()
        {
            var results = new List<FunctionCacheGetResult<(string, string), string>>();
            var cache = new TestLocalCache<(string, string), string>();
            
            Func<string, string, Task<string>> func = (k1, k2) => Task.FromResult(k1);
            
            Func<string, string, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k1, k2, v) => k1 == "1" || v == "2")
                    .OnResult(results.Add)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToList();
            
            foreach (var key in keys)
                await cachedFunc(key, "ignored");

            cache.Values.Keys.Select(k => k.Item1).Should().BeEquivalentTo(keys.Except(new[] { "1", "2" }));
        }

        [Fact]
        public async Task MultiParamEnumerableKeySkipGet()
        {
            var results = new List<FunctionCacheGetResult<(string, int), string>>();
            var cache = new TestLocalCache<(string, int), string>();
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k1, k2) => k2 == 1, SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            var outerKey = Guid.NewGuid().ToString();
            var innerKeys = Enumerable.Range(0, 10).ToArray();

            await cachedFunc(outerKey, innerKeys);
            
            results.Should().ContainSingle();
            cache.Values.Should().ContainKey((outerKey, 1));
            
            await cachedFunc(outerKey, innerKeys);

            results.Should().HaveCount(2);

            foreach (var x in results.Last().Results)
                x.Outcome.Should().Be(x.Key.AsObject.Item2 == 1 ? Outcome.Fetch : Outcome.FromCache);
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeySkipSet()
        {
            var cache = new TestLocalCache<(string, int), string>();
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k1, k2) => k2 == 1, SkipCacheSettings.SkipSet)
                    .Build();
            }

            var outerKey = Guid.NewGuid().ToString();
            var innerKeys = Enumerable.Range(0, 10).ToArray();

            await cachedFunc(outerKey, innerKeys);

            cache.Values.Keys.Should().BeEquivalentTo(innerKeys.Where(k => k != 1).Select(k => (outerKey, k)));
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeySkipGetOuterKeyOnly()
        {
            var results = new List<FunctionCacheGetResult<(int, int), int>>();
            var cache = new TestLocalCache<(int, int), int>();
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, int>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<int, IEnumerable<int>, IDictionary<int, int>, int, int>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k1 => k1 == 1, SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            for (var outerKey = 0; outerKey < 5; outerKey++)
            {
                var innerKeys = Enumerable.Range(0, 10).ToArray();

                await cachedFunc(outerKey, innerKeys);

                results.Should().ContainSingle();
                cache.Values.Should().ContainKeys(innerKeys.Select(i => (outerKey, i)).ToArray());

                await cachedFunc(outerKey, innerKeys);

                results.Should().HaveCount(2);

                foreach (var x in results.Last().Results)
                    x.Outcome.Should().Be(outerKey == 1 ? Outcome.Fetch : Outcome.FromCache);

                results.Clear();
            }
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeySkipSetOuterKeyOnly()
        {
            var cache = new TestLocalCache<(int, int), int>();
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, int>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<int, IEnumerable<int>, IDictionary<int, int>, int, int>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k1 => k1 == 1, SkipCacheSettings.SkipSet)
                    .Build();
            }

            for (var outerKey = 0; outerKey < 5; outerKey++)
            {
                var innerKeys = Enumerable.Range(0, 10).ToArray();

                await cachedFunc(outerKey, innerKeys);

                if (outerKey == 1)
                    cache.Values.Keys.Should().NotContain(innerKeys.Select(i => (outerKey, i)).ToArray());
                else
                    cache.Values.Keys.Should().Contain(innerKeys.Select(i => (outerKey, i)).ToArray());
            }
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeySkipSetBasedOnKeyAndValue()
        {
            var cache = new TestLocalCache<(int, int), int>();
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, int>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<int, IEnumerable<int>, IDictionary<int, int>, int, int>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen((k1, k2, v) => k2 == 1 || v == 2)
                    .Build();
            }

            var innerKeys = Enumerable.Range(0, 10).ToArray();

            await cachedFunc(0, innerKeys);

            cache.Values.Keys.Select(k => k.Item2).Should().BeEquivalentTo(innerKeys.Except(new[] { 1, 2 }));
        }

        [Fact]
        public async Task MultipleSkipGetConditions()
        {
            var results = new List<FunctionCacheGetResult<string, string>>();
            var cache = new TestLocalCache<string, string>();
            
            Func<string, Task<string>> echo = new Echo();
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k => k == "1", SkipCacheSettings.SkipGet)
                    .SkipCacheWhen(k => k == "2", SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();
            
            foreach (var k in keys)
                await cachedEcho(k);

            cache.Values.Should().ContainKeys(keys);

            foreach (var k in keys)
            {
                await cachedEcho(k);

                results.Last().Results.Single().Key.AsObject.Should().Be(k);
                results.Last().Results.Single().Outcome.Should().Be(k == "1" || k == "2" ? Outcome.Fetch : Outcome.FromCache);
            }
        }
        
        [Fact]
        public async Task MultipleSkipSetConditions()
        {
            var cache = new TestLocalCache<string, string>();
            
            Func<string, Task<string>> echo = new Echo();
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k => k == "1", SkipCacheSettings.SkipSet)
                    .SkipCacheWhen(k => k == "2", SkipCacheSettings.SkipSet)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();
            
            foreach (var k in keys)
                await cachedEcho(k);
           
            cache.Values.Should().ContainKeys(keys.Where(k => k != "1" && k != "2"));
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeyMultipleConditionsSkipGet()
        {
            var results = new List<FunctionCacheGetResult<(int, int), int>>();
            var cache = new TestLocalCache<(int, int), int>();
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, int>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<int, IEnumerable<int>, IDictionary<int, int>, int, int>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k1 => k1 <= 2, SkipCacheSettings.SkipGet)
                    .SkipCacheWhen((k1, k2) => k1 + k2 > 10, SkipCacheSettings.SkipGet)
                    .OnResult(results.Add)
                    .Build();
            }

            for (var outerKey = 1; outerKey < 5; outerKey++)
            {
                var innerKeys = Enumerable.Range(0, 10).ToArray();

                await cachedFunc(outerKey, innerKeys);

                results.Should().ContainSingle();
                cache.Values.Should().ContainKeys(innerKeys.Select(i => (outerKey, i)).ToArray());

                await cachedFunc(outerKey, innerKeys);

                results.Should().HaveCount(2);

                foreach (var x in results.Last().Results)
                    x.Outcome.Should().Be(outerKey <= 2 || (outerKey + x.Key.AsObject.Item2) > 10 ? Outcome.Fetch : Outcome.FromCache);
                
                results.Clear();
            }
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeyMultipleConditionsSkipSet()
        {
            var results = new List<FunctionCacheGetResult<(int, int), int>>();
            var cache = new TestLocalCache<(int, int), int>();
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, int>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<int, IEnumerable<int>, Task<IDictionary<int, int>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<int, IEnumerable<int>, IDictionary<int, int>, int, int>()
                    .WithLocalCache(cache)
                    .SkipCacheWhen(k1 => k1 <= 2, SkipCacheSettings.SkipSet)
                    .SkipCacheWhen((k1, k2) => k1 + k2 > 10, SkipCacheSettings.SkipSet)
                    .OnResult(results.Add)
                    .Build();
            }

            for (var outerKey = 1; outerKey < 5; outerKey++)
            {
                var innerKeys = Enumerable.Range(0, 10).ToArray();

                await cachedFunc(outerKey, innerKeys);

                foreach (var innerKey in innerKeys)
                    cache.Values.Keys.Contains((outerKey, innerKey)).Should().Be(outerKey > 2 && outerKey + innerKey <= 10);
            }
        }
    }
}