
# CacheMeIfYouCan [![Build status](https://ci.appveyor.com/api/projects/status/pl5d7dyb7iu59nyx?svg=true)](https://ci.appveyor.com/project/hpeebles/cachemeifyoucan)

CacheMeIfYouCan serves 2 main purposes -
1. To create cached versions of functions (or interfaces) using any local and/or distributed cache as the underlying data store ([Function Cache](#function-cache)).
2. To create objects which each hold a single value and expose it for fast access while allowing this value to be updated in the background ([Cached Object](#cached-object)).

## Installing via NuGet
    Install-Package CacheMeIfYouCan

## Function Cache
High level FunctionCache work flow -

![FunctionCache work flow](https://github.com/hpeebles/CacheMeIfYouCan/blob/master/FlowCharts/FunctionCache.png)

Basic usage -
```csharp
// Original function
Func<TK, TV> myFunc = ... // Or Func<TK, Task<TV>>

// Cached copy
Func<TK, TV> myCachedFunc = myFunc
    .Cached()
    .Build();

// Original interface
IMyInterface myInterface = ...

// Cached copy
IMyInterface myCachedInterface = myInterface
    .Cached()
    .Build();
```

## Cached Object
Basic usage -
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