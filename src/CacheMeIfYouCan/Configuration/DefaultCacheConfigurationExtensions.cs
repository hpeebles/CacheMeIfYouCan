using System;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithMemoryCache(
            this DefaultCacheConfiguration config)
        {
            return config.WithLocalCacheFactory(new MemoryCacheFactory());
        }

        public static DefaultCacheConfiguration WithDictionaryCache(
            this DefaultCacheConfiguration config)
        {
            return config.WithLocalCacheFactory(new DictionaryCacheFactory());
        }

        public static DefaultCacheConfiguration CreateCacheFactoryPreset(
            this DefaultCacheConfiguration config,
            int id,
            ILocalCacheFactory cacheFactory)
        {
            return config.CreateCacheFactoryPreset(id, cacheFactory, null);
        }
        
        public static DefaultCacheConfiguration CreateCacheFactoryPreset(
            this DefaultCacheConfiguration config,
            int id,
            IDistributedCacheFactory cacheFactory)
        {
            return config.CreateCacheFactoryPreset(id, null, cacheFactory);
        }
        
        public static DefaultCacheConfiguration CreateCacheFactoryPreset<TEnum>(
            this DefaultCacheConfiguration config,
            TEnum id,
            ILocalCacheFactory cacheFactory)
            where TEnum : struct, Enum
        {
            return config.CreateCacheFactoryPreset(id, cacheFactory, null);
        }
        
        public static DefaultCacheConfiguration CreateCacheFactoryPreset<TEnum>(
            this DefaultCacheConfiguration config,
            TEnum id,
            IDistributedCacheFactory cacheFactory)
            where TEnum : struct, Enum
        {
            return config.CreateCacheFactoryPreset(id, null, cacheFactory);
        }
        
        public static DefaultCacheConfiguration OnResultObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheGetResult>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, config.OnResult, behaviour);
        }
        
        public static DefaultCacheConfiguration OnFetchObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheFetchResult>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, config.OnFetch, behaviour);
        }
        
        public static DefaultCacheConfiguration OnExceptionObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheException>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, config.OnException, behaviour);
        }
        
        public static DefaultCacheConfiguration OnCacheGetObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheGetResult>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, config.OnCacheGet, behaviour);
        }
        
        public static DefaultCacheConfiguration OnCacheSetObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheSetResult>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, config.OnCacheSet, behaviour);
        }
        
        public static DefaultCacheConfiguration OnCacheExceptionObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheException>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, config.OnCacheException, behaviour);
        }
    }
}