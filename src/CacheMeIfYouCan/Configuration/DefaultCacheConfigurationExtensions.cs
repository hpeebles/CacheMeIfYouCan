using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithOnResultObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheGetResult>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, config.WithOnResultAction, behaviour);
        }
        
        public static DefaultCacheConfiguration WithOnFetchObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheFetchResult>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, config.WithOnFetchAction, behaviour);
        }
        
        public static DefaultCacheConfiguration WithOnErrorObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheException>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, config.WithOnErrorAction, behaviour);
        }
        
        public static DefaultCacheConfiguration WithOnCacheGetObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheGetResult>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, config.WithOnCacheGetAction, behaviour);
        }
        
        public static DefaultCacheConfiguration WithOnCacheSetObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheSetResult>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, config.WithOnCacheSetAction, behaviour);
        }
        
        public static DefaultCacheConfiguration WithOnCacheErrorObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheException>> onCacheError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheError, config.WithOnCacheErrorAction, behaviour);
        }
    }
}