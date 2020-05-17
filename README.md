# CacheMeIfYouCan
[![Build Status](https://dev.azure.com/hamishpeebles/CacheMeIfYouCan/_apis/build/status/hpeebles.CacheMeIfYouCan?branchName=master)](https://dev.azure.com/hamishpeebles/CacheMeIfYouCan/_build/latest?definitionId=5&branchName=master)
[![NuGet version (CacheMeIfYouCan)](https://img.shields.io/nuget/v/CacheMeIfYouCan.svg)](https://www.nuget.org/packages/CacheMeIfYouCan/)

## Features
- [Cached Functions](#cached-functions)
- [Cached Interfaces](#cached-interfaces)
- [Cached Objects](#cached-objects)

## Installing via NuGet
    Install-Package CacheMeIfYouCan

## Cached Functions
A cached function is a new version of an input function, which when invoked first tries to find the return value in a
cache and only calls into the original function if the value was not found.
 
Cached functions come with a wide variety of configuration options and the ability to use any local cache (such as
System.Runtime.Caching.MemoryCache) or distributed cache (such as Redis) or a 2-tier caching strategy that uses a local
cache backed by a distributed cache.

```csharp
Func<int, Task<int>> originalFunction = ...

var cachedFunction = CachedFunctionFactory
    .ConfigureFor(originalFunction)
    .WithLocalCache(new DictionaryCache<int, int>())
    .WithDistributedCache(new RedisCache<int, int>(...))
    .WithTimeToLive(TimeSpan.FromHours(1))
    .OnResult(r => logSuccess(r), ex => logException(ex))
    .Build();
```

It is possible to create cached functions where the original function has up to 8 parameters (+ an optional
cancellation token) and the cache key can be any function of the input parameters. The original function can be
synchronous, asynchronous, or return a `ValueTask` (the underlying implementation uses ValueTasks). The resulting
cached function will always have exactly the same input parameters and return type as the original function.

```csharp
Func<string, bool, int, CancellationToken, Task<int>> originalFunction = ...

var cachedFunction = CachedFunctionFactory
    .ConfigureFor(originalFunction)
    .WithCacheKey((param1, param2, param3) => param3)
    .WithLocalCache(new DictionaryCache<int, int>())
    .WithTimeToLive(TimeSpan.FromHours(1))
    .Build();
```

If your function takes a list of keys as the last parameter and returns a dictionary of values where each key in the
returned dictionary corresponds to a key in the input list then you can use the `WithEnumerableKeys` configuration
option. This will cause each key in the input list to be cached separately. When the cached function is called, all
the keys are looked up in the cache and the underlying function is only called for those keys that were not found.

_Note: The `WithEnumerableKeys` option is available whenever the last parameter is an `IEnumerable<TKey>` and the return
type is an `IEnumerable<KeyValuePair<TKey, TValue>>`._

```csharp
Func<List<int>, Dictionary<int, string>> originalFunction = null;

var cachedFunction = CachedFunctionFactory
    .ConfigureFor(originalFunction)
    .WithEnumerableKeys<List<int>, Dictionary<int, string>, int, string>() // The generic arguments are each of the parameter types, the return type (only the inner type if Task<T> or ValueTask<T>), then the type of the keys, then the type of the values
    .WithLocalCache(new DictionaryCache<int, string>())
    .WithTimeToLive(TimeSpan.FromHours(1))
    .Build();
```

You can use the `WithEnumerableKeys` configuration option for functions with multiple input parameters. In these cases
the `WithOuterCacheKey` option is available. Use `WithOuterCacheKey` to make each key in the cache be made up of 2
parts, for each value in the last input parameter an 'inner key' is created, and then the 'outer key' is made by
passing the earlier parameters into the outer key generator function. In order for there to be a cache hit both the 
inner key and the outer key need to be matched.

```csharp
Func<string, string, List<int>, Dictionary<int, string>> originalFunction = null;

var cachedFunction = CachedFunctionFactory
    .ConfigureFor(originalFunction)
    .WithEnumerableKeys<string, string, List<int>, Dictionary<int, string>, int, string>()
    .WithOuterCacheKey((param1, param2) => $"{param1}_{param2}")
    .WithLocalCache(new DictionaryCache<string, int, string>())
    .WithTimeToLive(TimeSpan.FromHours(1))
    .Build();
```

## Cached Interfaces
A cached interface is an implementation of an interface where each function is an individually configured cached
function.

```csharp
public interface IMyCoolInterface
{
    string GetOne(int id);
    Dictionary<int, string> GetMany(List<int> ids);
}
```

```csharp
IMyCoolInterface originalImpl = ...;

var cache = new DictionaryCache<int, string>();

IMyCoolInterface cachedImpl = CachedInterfaceFactory.For(originalImpl)
    .Configure<int, string>(x => x.GetOne, c => c
        .WithLocalCache(cache)
        .WithTimeToLive(TimeSpan.FromHours(1)))
    .Configure<List<int>, Dictionary<int, string>>(x => x.GetMany, c => c
        .WithEnumerableKeys<List<int>, Dictionary<int, string>, int, string>()
        .WithLocalCache(cache) // Note that I am sharing the same cache between the 2 methods
        .WithTimeToLive(TimeSpan.FromHours(1)))
    .Build();
```

Each call to `Configure` takes 2 input parameters, the first is an expression which (along with the generic type
arguments) is used to identify the method to be configured, the second is the configuration function which is used to
configure the cached function.

## Cached Objects
A cached object is an object which exposes a single value which can be refreshed or updated in the background.
Accessing the value is fast and reliable since any failures that happen while trying to update the value do not
propagate to the threads accessing the value.

```csharp
Func<Task<Configuration>> getConfigurationFunc = ...

var cachedConfiguration = CachedObjectFactory
    .ConfigureFor(getConfigurationFunc)
    .WithRefreshInterval(TimeSpan.FromMinutes(10))
    .WithJitter(10) // Use WithJitter if you want the refresh intervals to flucuate slightly
    .Build();

cachedConfiguration.Initialize(); // The first access will trigger a call to initialize if not already initialized

// To access the value
var configuration = cachedConfiguration.Value;
```

## Benchmarks

Cached function with single key using DictionaryCache -

|    Method |      Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |----------:|---------:|---------:|------:|------:|------:|----------:|
|  CacheHit |  90.10 ns | 0.269 ns | 0.252 ns |     - |     - |     - |         - |
| CacheMiss | 180.04 ns | 1.192 ns | 1.115 ns |     - |     - |     - |         - |

Cached function with enumerable keys using DictionaryCache -

|                            Method |        Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|---------:|---------:|-------:|-------:|------:|----------:|
|                            OneHit |    410.9 ns |  3.19 ns |  2.98 ns | 0.0391 |      - |     - |     248 B |
|                           OneMiss |    683.2 ns |  1.96 ns |  1.63 ns | 0.0439 |      - |     - |     280 B |
|                  OneHitAndOneMiss |    810.8 ns |  4.53 ns |  4.24 ns | 0.0734 |      - |     - |     464 B |
|                    OneHundredHits |  2,704.3 ns | 20.80 ns | 19.45 ns | 0.3700 | 0.0038 |     - |    2328 B |
|                  OneHundredMisses | 10,784.1 ns | 42.62 ns | 37.78 ns | 0.3662 |      - |     - |    2360 B |
| OneHundredHitsAndOneHundredMisses | 15,643.8 ns | 99.99 ns | 88.64 ns | 1.4954 | 0.0610 |     - |    9456 B |
