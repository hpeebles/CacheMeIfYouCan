using System;
using System.Collections.Generic;
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
        private TimeSpan? _timeToLive;
        private bool? _earlyFetchEnabled;
        private bool? _disableCache;
        private ILocalCacheFactory _localCacheFactory;
        private IDistributedCacheFactory _distributedCacheFactory;
        private Func<CachedProxyFunctionInfo, string> _keyspacePrefixFunc;
        private Action<FunctionCacheGetResult> _onResult;
        private Action<FunctionCacheFetchResult> _onFetch;
        private Action<FunctionCacheException> _onError;
        private Action<CacheGetResult> _onCacheGet;
        private Action<CacheSetResult> _onCacheSet;
        private Action<CacheException> _onCacheError;
        private readonly IDictionary<MethodInfoKey, object> _functionCacheConfigActions;

        internal CachedProxyConfigurationManager(T impl)
        {
            _impl = impl;
            _keySerializers = new KeySerializers();
            _valueSerializers = new ValueSerializers();
            _onResult = DefaultCacheConfig.Configuration.OnResult;
            _onFetch = DefaultCacheConfig.Configuration.OnFetch;
            _onError = DefaultCacheConfig.Configuration.OnError;
            _onCacheGet = DefaultCacheConfig.Configuration.OnCacheGet;
            _onCacheSet = DefaultCacheConfig.Configuration.OnCacheSet;
            _onCacheError = DefaultCacheConfig.Configuration.OnCacheError;
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

        public CachedProxyConfigurationManager<T> WithEarlyFetch(bool enabled = true)
        {
            _earlyFetchEnabled = enabled;
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
        
        public CachedProxyConfigurationManager<T> OnResult(
            Action<FunctionCacheGetResult> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onResult = ActionsHelper.Combine(_onResult, onResult, ordering);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnFetch(
            Action<FunctionCacheFetchResult> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onFetch = ActionsHelper.Combine(_onFetch, onFetch, ordering);
            return this;
        }

        public CachedProxyConfigurationManager<T> OnError(
            Action<FunctionCacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, ordering);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheGet(
            Action<CacheGetResult> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onCacheGet = ActionsHelper.Combine(_onCacheGet, onCacheGet, ordering);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheSet(
            Action<CacheSetResult> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onCacheSet = ActionsHelper.Combine(_onCacheSet, onCacheSet, ordering);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheError(
            Action<CacheException> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onCacheError = ActionsHelper.Combine(_onCacheError, onCacheError, ordering);
            return this;
        }

        public CachedProxyConfigurationManager<T> ConfigureFor<TK, TV>(
            Expression<Func<T, Func<TK, Task<TV>>>> expression,
            Action<FunctionCacheConfigurationManager<TK, TV>> configAction)
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
                _timeToLive,
                _earlyFetchEnabled,
                _disableCache,
                _localCacheFactory,
                _distributedCacheFactory,
                _keyspacePrefixFunc,
                _onResult,
                _onFetch,
                _onError,
                _onCacheGet,
                _onCacheSet,
                _onCacheError,
                _functionCacheConfigActions);
            
            return CachedProxyFactory.Build(_impl, config);
        }

        public static implicit operator T(CachedProxyConfigurationManager<T> configManager)
        {
            return configManager.Build();
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