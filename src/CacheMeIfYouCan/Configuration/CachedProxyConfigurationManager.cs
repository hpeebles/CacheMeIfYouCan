using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public class CachedProxyConfigurationManager<T>
    {
        private readonly T _impl;
        private readonly KeySerializers _keySerializers;
        private readonly ValueSerializers _valueSerializers;
        private readonly EqualityComparers _keyComparers;
        private TimeSpan? _timeToLive;
        private bool? _disableCache;
        private ILocalCacheFactory _localCacheFactory;
        private IDistributedCacheFactory _distributedCacheFactory;
        private Func<CachedProxyFunctionInfo, string> _keyspacePrefixFunc;
        private Action<FunctionCacheGetResult> _onResult;
        private Action<FunctionCacheFetchResult> _onFetch;
        private Action<FunctionCacheException> _onException;
        private Action<CacheGetResult> _onCacheGet;
        private Action<CacheSetResult> _onCacheSet;
        private Action<CacheException> _onCacheException;
        private string _keyParamSeparator;
        private int _maxFetchBatchSize;
        private readonly IDictionary<MethodInfoKey, object> _functionCacheConfigActions;

        internal CachedProxyConfigurationManager(T impl)
        {
            _impl = impl;
            _keySerializers = new KeySerializers();
            _valueSerializers = new ValueSerializers();
            _keyComparers = new EqualityComparers();
            _onResult = DefaultSettings.Cache.OnResultAction;
            _onFetch = DefaultSettings.Cache.OnFetchAction;
            _onException = DefaultSettings.Cache.OnExceptionAction;
            _onCacheGet = DefaultSettings.Cache.OnCacheGetAction;
            _onCacheSet = DefaultSettings.Cache.OnCacheSetAction;
            _onCacheException = DefaultSettings.Cache.OnCacheExceptionAction;
            _keyParamSeparator = DefaultSettings.Cache.KeyParamSeparator;
            _maxFetchBatchSize = DefaultSettings.Cache.MaxFetchBatchSize;
            _functionCacheConfigActions = new Dictionary<MethodInfoKey, object>();
        }
                
        public CachedProxyConfigurationManager<T> WithTimeToLive(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithKeySerializers(Action<KeySerializers> configAction)
        {
            configAction(_keySerializers);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithValueSerializers(Action<ValueSerializers> configAction)
        {
            configAction(_valueSerializers);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithKeyComparer<TK>(IEqualityComparer<TK> comparer)
        {
            _keyComparers.Set(comparer);
            return this;
        }

        public CachedProxyConfigurationManager<T> DisableCache(bool disableCache = true)
        {
            _disableCache = disableCache;
            return this;
        }

        public CachedProxyConfigurationManager<T> WithLocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            _localCacheFactory = cacheFactory;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> SkipLocalCache(bool skipLocalCache = true)
        {
            if (skipLocalCache)
                _localCacheFactory = new NullLocalCacheFactory();
            
            return this;
        }

        public CachedProxyConfigurationManager<T> WithDistributedCacheFactory(IDistributedCacheFactory cacheFactory)
        {
            return WithDistributedCacheFactory(cacheFactory, f => null);
        }
        
        public CachedProxyConfigurationManager<T> WithDistributedCacheFactory(IDistributedCacheFactory cacheFactory, string keyspacePrefix)
        {
            return WithDistributedCacheFactory(cacheFactory, f => keyspacePrefix);
        }
        
        public CachedProxyConfigurationManager<T> WithDistributedCacheFactory(IDistributedCacheFactory cacheFactory, Func<CachedProxyFunctionInfo, string> keyspacePrefixFunc)
        {
            _distributedCacheFactory = cacheFactory;
            _keyspacePrefixFunc = keyspacePrefixFunc;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> SkipDistributedCache(bool skipDistributedCache = true)
        {
            if (skipDistributedCache)
            {
                _distributedCacheFactory = new NullDistributedCacheFactory();
                _keyspacePrefixFunc = null;
            }

            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithCacheFactoryPreset(int id)
        {
            return WithCacheFactoryPresetImpl(CacheFactoryPresetKeyFactory.Create(id));
        }
        
        public CachedProxyConfigurationManager<T> WithCacheFactoryPreset<TEnum>(TEnum id) where TEnum : struct, Enum
        {
            return WithCacheFactoryPresetImpl(CacheFactoryPresetKeyFactory.Create(id));
        }
        
        private CachedProxyConfigurationManager<T> WithCacheFactoryPresetImpl(CacheFactoryPresetKey key)
        {
            if (!DefaultSettings.Cache.CacheFactoryPresets.TryGetValue(key, out var cacheFactories))
            {
                var existingKeys = String.Join(
                    ", ",
                    DefaultSettings.Cache.CacheFactoryPresets.Keys.Select(k => k.ToString()));
                
                throw new Exception($"Cache factory preset not found. Requested Key: {key}. Existing Keys: {existingKeys}");
            }

            if (cacheFactories.local == null)
                SkipLocalCache();
            else
                WithLocalCacheFactory(cacheFactories.local);

            if (cacheFactories.distributed == null)
                SkipDistributedCache();
            else
                WithDistributedCacheFactory(cacheFactories.distributed);

            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnResult(
            Action<FunctionCacheGetResult> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onResult = ActionsHelper.Combine(_onResult, onResult, behaviour);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnFetch(
            Action<FunctionCacheFetchResult> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onFetch = ActionsHelper.Combine(_onFetch, onFetch, behaviour);
            return this;
        }

        public CachedProxyConfigurationManager<T> OnException(
            Action<FunctionCacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onException = ActionsHelper.Combine(_onException, onException, behaviour);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheGet(
            Action<CacheGetResult> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onCacheGet = ActionsHelper.Combine(_onCacheGet, onCacheGet, behaviour);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheSet(
            Action<CacheSetResult> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onCacheSet = ActionsHelper.Combine(_onCacheSet, onCacheSet, behaviour);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheException(
            Action<CacheException> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onCacheException = ActionsHelper.Combine(_onCacheException, onCacheException, behaviour);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithKeyParamSeparator(string separator)
        {
            _keyParamSeparator = separator;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithBatchedFetches(int batchSize)
        {
            _maxFetchBatchSize = batchSize;
            return this;
        }

        public CachedProxyConfigurationManager<T> ConfigureFor<TK, TV>(
            Expression<Func<T, Func<TK, Task<TV>>>> expression,
            Action<SingleKeyFunctionCacheConfigurationManager<TK, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }

        public CachedProxyConfigurationManager<T> ConfigureFor<TK, TV>(
            Expression<Func<T, Func<TK, TV>>> expression,
            Action<SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TReq, TRes, TK, TV>(
            Expression<Func<T, Func<TReq, Task<TRes>>>> expression,
            Action<EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>> configAction)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }

        public CachedProxyConfigurationManager<T> ConfigureFor<TReq, TRes, TK, TV>(
            Expression<Func<T, Func<TReq, TRes, TK, TV>>> expression,
            Action<EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>> configAction)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TK1, TK2, TV>(
            Expression<Func<T, Func<TK1, TK2, Task<TV>>>> expression,
            Action<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TK1, TK2, TV>(
            Expression<Func<T, Func<TK1, TK2, TV>>> expression,
            Action<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TK1, TK2, TK3, TV>(
            Expression<Func<T, Func<TK1, TK2, TK3, Task<TV>>>> expression,
            Action<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TK1, TK2, TK3, TV>(
            Expression<Func<T, Func<TK1, TK2, TK3, TV>>> expression,
            Action<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TK1, TK2, TK3, TK4, TV>(
            Expression<Func<T, Func<TK1, TK2, TK3, TK4, Task<TV>>>> expression,
            Action<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> ConfigureFor<TK1, TK2, TK3, TK4, TV>(
            Expression<Func<T, Func<TK1, TK2, TK3, TK4, TV>>> expression,
            Action<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(typeof(T), methodInfo);
            
            _functionCacheConfigActions[key] = configAction;
            
            return this;
        }
        
        public T Build()
        {
            var config = new CachedProxyConfig(
                typeof(T),
                _keySerializers,
                _valueSerializers,
                _keyComparers,
                _timeToLive,
                _disableCache,
                _localCacheFactory,
                _distributedCacheFactory,
                _keyspacePrefixFunc,
                _onResult,
                _onFetch,
                _onException,
                _onCacheGet,
                _onCacheSet,
                _onCacheException,
                _keyParamSeparator,
                _functionCacheConfigActions);
            
            return CachedProxyFactory.Build(_impl, config);
        }
        
        private static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodCallObject = (ConstantExpression)methodCallExpression.Object;
            return (MethodInfo)methodCallObject.Value;
        }
    }
}