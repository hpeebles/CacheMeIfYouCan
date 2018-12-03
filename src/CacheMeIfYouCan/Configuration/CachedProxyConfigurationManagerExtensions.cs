using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class CachedProxyConfigurationManagerExtensions
    {
        public static CachedProxyConfigurationManager<T> OnResultObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheGetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnResult, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnFetchObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheFetchResult>> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onFetch, configManager.OnFetch, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnErrorObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheGetObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheGetResult>> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheSetObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheSetResult>> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheErrorObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheException>> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheError, configManager.OnCacheError, ordering);
        }
    }
}