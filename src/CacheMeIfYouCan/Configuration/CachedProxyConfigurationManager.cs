using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public class CachedProxyConfigurationManager<T>
    {
        private readonly T _impl;
        private readonly string _name;
        private readonly KeySerializers _keySerializers;
        private readonly ValueSerializers _valueSerializers;
        private TimeSpan? _timeToLive;
        private bool? _earlyFetchEnabled;
        private bool? _disableCache;
        private ILocalCacheFactory _localCacheFactory;
        private ICacheFactory _remoteCacheFactory;
        private Action<FunctionCacheGetResult> _onResult;
        private Action<FunctionCacheFetchResult> _onFetch;
        private Action<FunctionCacheErrorEvent> _onError;
        private Action<CacheGetResult> _onCacheGet;
        private Action<CacheSetResult> _onCacheSet;
        private readonly IDictionary<MethodInfoKey, object> _functionCacheConfigActions;

        internal CachedProxyConfigurationManager(T impl, string name)
        {
            _impl = impl;
            _name = name;
            _keySerializers = new KeySerializers();
            _valueSerializers = new ValueSerializers();
            _onResult = DefaultCacheConfig.Configuration.OnResult;
            _onFetch = DefaultCacheConfig.Configuration.OnFetch;
            _onError = DefaultCacheConfig.Configuration.OnError;
            _onCacheGet = DefaultCacheConfig.Configuration.OnCacheGet;
            _onCacheSet = DefaultCacheConfig.Configuration.OnCacheSet;
            _functionCacheConfigActions = new Dictionary<MethodInfoKey, object>();
        }
                
        public CachedProxyConfigurationManager<T> For(TimeSpan timeToLive)
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

        public CachedProxyConfigurationManager<T> WithRemoteCacheFactory(ICacheFactory cacheFactory)
        {
            _remoteCacheFactory = cacheFactory;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnResult(Action<FunctionCacheGetResult> onResult, bool append = false)
        {
            if (_onResult == null || !append)
                _onResult = onResult;
            else
                _onResult = x => { _onResult(x); onResult(x); };
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnFetch(Action<FunctionCacheFetchResult> onFetch, bool append = false)
        {
            if (_onFetch == null || !append)
                _onFetch = onFetch;
            else
                _onFetch = x => { _onFetch(x); onFetch(x); };

            return this;
        }

        public CachedProxyConfigurationManager<T> OnError(Action<FunctionCacheErrorEvent> onError, bool append = false)
        {
            if (_onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheGet(Action<CacheGetResult> onCacheGet, bool append = false)
        {
            if (onCacheGet == null || !append)
                _onCacheGet = onCacheGet;
            else
                _onCacheGet = x => { _onCacheGet(x); onCacheGet(x); };

            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnCacheSet(Action<CacheSetResult> onCacheSet, bool append = false)
        {
            if (onCacheSet == null || !append)
                _onCacheSet = onCacheSet;
            else
                _onCacheSet = x => { _onCacheSet(x); onCacheSet(x); };

            return this;
        }

        public CachedProxyConfigurationManager<T> ConfigureFor<TK, TV>(
            Expression<Func<T, Func<TK, Task<TV>>>> expression,
            Action<FunctionCacheConfigurationManager<TK, TV>> configAction)
        {
            var methodInfo = GetMethodInfo(expression);
            
            var key = new MethodInfoKey(methodInfo);
            
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
                _remoteCacheFactory,
                _onResult,
                _onFetch,
                _onError,
                _onCacheGet,
                _onCacheSet,
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