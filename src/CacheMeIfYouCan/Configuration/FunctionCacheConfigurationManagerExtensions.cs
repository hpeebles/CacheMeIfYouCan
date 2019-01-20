using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class FunctionCacheConfigurationManagerExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> OnResultObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> OnFetchObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> OnExceptionObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> OnCacheGetObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> OnCacheSetObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> OnCacheExceptionObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> OnResultObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> OnFetchObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> OnExceptionObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> OnCacheGetObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> OnCacheSetObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> OnCacheExceptionObservable<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnResultObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnFetchObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnExceptionObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnCacheGetObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnCacheSetObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> OnCacheExceptionObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnResultObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnResult, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnFetchObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onFetch, configManager.OnFetch, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnExceptionObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnCacheGetObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheGet, configManager.OnCacheGet, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnCacheSetObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheSet, configManager.OnCacheSet, behaviour);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> OnCacheExceptionObservable<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return ObservablesHelper.SetupObservable(onCacheException, configManager.OnCacheException, behaviour);
        }
    }
}