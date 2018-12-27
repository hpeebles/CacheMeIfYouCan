using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class FunctionCacheConfigurationManagerObservableExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> OnResultObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnFetchObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, behaviour);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheGetObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheSetObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheErrorObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheError, configManager.OnCacheError, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnResultObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnFetchObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnCacheGetObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnCacheSetObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> OnCacheErrorObservable<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheError, configManager.OnCacheError, behaviour);
        }
    }
}