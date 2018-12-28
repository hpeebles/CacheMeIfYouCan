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
        
        public static DefaultCacheConfiguration WithOnExceptionObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<FunctionCacheException>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, config.WithOnExceptionAction, behaviour);
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
        
        public static DefaultCacheConfiguration WithOnCacheExceptionObservable(
            this DefaultCacheConfiguration config,
            Action<IObservable<CacheException>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, config.WithOnCacheExceptionAction, behaviour);
        }
    }
}