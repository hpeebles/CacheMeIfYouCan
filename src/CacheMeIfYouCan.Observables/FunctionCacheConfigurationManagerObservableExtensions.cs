using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Observables
{
    public static class FunctionCacheConfigurationManagerObservableExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> OnResultObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onResult, configManager.OnResult, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnFetchObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onFetch, configManager.OnFetch, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheGetObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onCacheGet, configManager.OnCacheGet, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheSetObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return SetupObservable(onCacheSet, configManager.OnCacheSet, ordering);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheErrorObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheError,
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