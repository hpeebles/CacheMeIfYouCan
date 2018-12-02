using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Observables
{
    public static class CachedProxyConfigurationManagerObservableExtensions
    {
        public static CachedProxyConfigurationManager<T> OnResultObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheGetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onResult, configManager.OnResult, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnFetchObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheFetchResult>> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onFetch, configManager.OnFetch, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnErrorObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<FunctionCacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheGetObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheGetResult>> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onCacheGet, configManager.OnCacheGet, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheSetObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheSetResult>> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onCacheSet, configManager.OnCacheSet, ordering);
        }
        
        public static CachedProxyConfigurationManager<T> OnCacheErrorObservable<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<IObservable<CacheException>> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onCacheError, configManager.OnCacheError, ordering);
        }

        private static TConfig SetupObservable<T, TConfig>(
            Action<IObservable<T>> action,
            Func<Action<T>, ActionOrdering, TConfig> configFunc,
            ActionOrdering ordering)
        {
            var subject = new Subject<T>();

            action(subject.AsObservable());

            return configFunc(subject.OnNext, ordering);
        }
    }
}