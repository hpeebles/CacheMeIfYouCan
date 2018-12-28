using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class CachedProxyConfigurationManagerExtensions
    {
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