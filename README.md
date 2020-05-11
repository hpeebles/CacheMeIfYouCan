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

|    Method |      Mean |     Error |   StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------- |----------:|----------:|---------:|-------:|-------:|------:|----------:|
|  CacheHit |  89.02 ns |  0.591 ns | 0.553 ns |      - |      - |     - |         - |
| CacheMiss | 269.76 ns | 11.085 ns | 9.827 ns | 0.0043 | 0.0014 |     - |      28 B |

Cached function with enumerable keys using DictionaryCache -

|                            Method |        Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|----------:|-------:|-------:|------:|----------:|
|                            OneHit |    437.7 ns |   5.68 ns |   5.32 ns | 0.0391 |      - |     - |     248 B |
|                           OneMiss |    914.4 ns |  10.82 ns |   9.59 ns | 0.0467 | 0.0086 |     - |     298 B |
|                  OneHitAndOneMiss |    996.7 ns |  14.98 ns |  14.01 ns | 0.0782 | 0.0191 |     - |     496 B |
|                    OneHundredHits |  2,613.0 ns |  18.27 ns |  16.19 ns | 0.3700 | 0.0038 |     - |    2328 B |
|                  OneHundredMisses | 21,696.7 ns | 982.24 ns | 870.73 ns | 0.8240 | 0.2747 |     - |    5302 B |
| OneHundredHitsAndOneHundredMisses | 26,992.5 ns | 996.80 ns | 883.64 ns | 1.9531 | 0.8240 |     - |   12296 B |
