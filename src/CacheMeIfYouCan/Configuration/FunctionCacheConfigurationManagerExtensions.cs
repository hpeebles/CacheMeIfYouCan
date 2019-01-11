using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class FunctionCacheConfigurationManagerExtensions
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
        
        public static FunctionCacheConfigurationManager<TK, TV> OnExceptionObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
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
        
        public static FunctionCacheConfigurationManager<TK, TV> OnCacheExceptionObservable<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> OnResultObservable<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> OnFetchObservable<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> OnExceptionObservable<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> OnCacheGetObservable<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> OnCacheSetObservable<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> OnCacheExceptionObservable<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnResultObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnFetchObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnExceptionObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnCacheGetObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnCacheSetObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnCacheExceptionObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnResultObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnFetchObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnExceptionObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnCacheGetObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnCacheSetObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnCacheExceptionObservable<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
    }
}