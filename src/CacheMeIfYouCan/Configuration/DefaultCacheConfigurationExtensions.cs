using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithOnResultObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheGetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, config.WithOnResultAction, ordering);
        }
        
        public static DefaultCacheConfiguration WithOnFetchObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheFetchResult>> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, config.WithOnFetchAction, ordering);
        }
        
        public static DefaultCacheConfiguration WithOnErrorObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, config.WithOnErrorAction, ordering);
        }
        
        public static DefaultCacheConfiguration WithOnCacheGetObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheGetResult>> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, config.WithOnCacheGetAction, ordering);
        }
        
        public static DefaultCacheConfiguration WithOnCacheSetObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheSetResult>> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, config.WithOnCacheSetAction, ordering);
        }
        
        public static DefaultCacheConfiguration WithOnCacheErrorObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheException>> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheError, config.WithOnCacheErrorAction, ordering);
        }
    }
}