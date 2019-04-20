
# CacheMeIfYouCan [![Build status](https://ci.appveyor.com/api/projects/status/pl5d7dyb7iu59nyx?svg=true)](https://ci.appveyor.com/project/hpeebles/cachemeifyoucan)

CacheMeIfYouCan can be used for 2 main purposes -
1. To create a cached copy of a function (or instance of an interface) improving performance and reducing load on external resources ([Function Cache](#function-cache)).
2. To create an object which holds a single value and exposes it for fast access while allowing this value to be updated in the background ([Cached Object](#cached-object)).

## Function Cache
```csharp
// Original function
Func<TK, TV> myFunc; // Or Func<TK, Task<TV>>

// Cached copy
Func<TK, TV> myCachedFunc = myFunc
    .Cached()
    .Build();

// Original interface
IMyInterface myInterface;

// Cached copy
IMyInterface myCachedInterface = myInterface
    .Cached()
    .Build();
```

## Cached Object
```csharp
// Create an ICachedObject<ConfigSettings> instance which refreshes the
// config settings in the background every 10 minutes
ICachedObject<ConfigSettings> cachedConfigSettings = CachedObjectFactory
    .ConfigureFor(() => GetConfigSettingsFromDatabase())
    .WithRefreshInterval(TimeSpan.FromMinutes(10))
    .Build();

// To retrieve the current value...
ConfigSettings configSettings = cachedConfigSettings.Value;
```