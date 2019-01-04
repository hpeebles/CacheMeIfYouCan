using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class CachedProxyConfigurationManagerExtensions
    {
        public static CachedProxyConfigurationManager<T> WithCacheFactoryPreset<T>(
            this CachedProxyConfigurationManager<T> configManager,
            int id)
        {
            return WithCacheFactoryPresetImpl(configManager, CacheFactoryPresetKeyFactory.Create(id));
        }
        
        public static CachedProxyConfigurationManager<T> WithCacheFactoryPreset<T, TEnum>(
            this CachedProxyConfigurationManager<T> configManager,
            TEnum id)
            where TEnum : struct, Enum
        {
            return WithCacheFactoryPresetImpl(configManager, CacheFactoryPresetKeyFactory.Create(id));
        }

        private static CachedProxyConfigurationManager<T> WithCacheFactoryPresetImpl<T>(
            CachedProxyConfigurationManager<T> configManager,
            CacheFactoryPresetKey key)
        {
            if (!DefaultSettings.Cache.CacheFactoryPresets.TryGetValue(key, out var cacheFactories))
                throw new Exception("No cache factory preset found. " + key);

            return configManager
                .WithLocalCacheFactory(cacheFactories.local)
                .WithDistributedCacheFactory(cacheFactories.distributed);
        }
        
        public static CachedProxyConfigurationManager<T> OnResultObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheGetResult>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static CachedProxyConfigurationManager<T> OnFetchObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheFetchResult>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static CachedProxyConfigurationManager<T> OnExceptionObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheException>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheGetObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheGetResult>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheSetObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheSetResult>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheExceptionObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheException>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
    }
}