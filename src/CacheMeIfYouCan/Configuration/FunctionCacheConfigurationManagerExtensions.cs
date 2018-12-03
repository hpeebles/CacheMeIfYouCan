using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class FunctionCacheConfigurationManagerObservableExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> OnResultObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnResult, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnFetchObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onFetch, configManager.OnFetch, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheGetObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheSetObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheErrorObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheError, configManager.OnCacheError, ordering);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnResultObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnResult, ordering);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnFetchObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onFetch, configManager.OnFetch, ordering);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnCacheGetObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, ordering);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnCacheSetObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, ordering);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnCacheErrorObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onCacheError, configManager.OnCacheError, ordering);
        }
    }
}