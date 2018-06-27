using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class CachedProxyConfigurationManager<T>
    {
        private readonly T _impl;
        private readonly string _name;
        private readonly Serializers _serializers;
        private TimeSpan? _timeToLive;
        private int? _memoryCacheMaxSizeMB;
        private bool? _earlyFetchEnabled;
        private ICacheFactory _cacheFactory;
        private Action<FunctionCacheGetResult> _onResult;
        private Action<FunctionCacheFetchResult> _onFetch;
        private Action<FunctionCacheErrorEvent> _onError;
        private readonly IDictionary<MethodInfoKey, object> _functionCacheConfigActions;

        internal CachedProxyConfigurationManager(T impl, string name)
        {
            _impl = impl;
            _name = name;
            _serializers = new Serializers();
            _functionCacheConfigActions = new Dictionary<MethodInfoKey, object>();
        }
                
        public CachedProxyConfigurationManager<T> For(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithSerializer<TField>(Func<TField, string> serializer, Func<string, TField> deserializer = null)
        {
            _serializers.Set(serializer, deserializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithSerializer<TField>(ISerializer serializer)
        {
            _serializers.Set<TField>(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithDefaultSerializer(Func<object, string> serializer)
        {
            _serializers.SetDefault(serializer);
            return this;
        }

        public CachedProxyConfigurationManager<T> WithDefaultSerializer(ISerializer serializer)
        {
            _serializers.SetDefault(serializer);
            return this;
        }

        public CachedProxyConfigurationManager<T> WithMaxMemoryCacheMaxSizeMB(int maxSizeMB)
        {
            _memoryCacheMaxSizeMB = maxSizeMB;
            return this;
        }

        public CachedProxyConfigurationManager<T> WithEarlyFetch(bool enabled = true)
        {
            _earlyFetchEnabled = enabled;
            return this;
        }

        public CachedProxyConfigurationManager<T> WithCacheFactory(ICacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnResult(Action<FunctionCacheGetResult> onResult, bool append = true)
        {
            if (_onResult == null || !append)
                _onResult = onResult;
            else
                _onResult = x => { _onResult(x); onResult(x); };
            
            return this;
        }
        
        public CachedProxyConfigurationManager<T> OnFetch(Action<FunctionCacheFetchResult> onFetch, bool append = true)
        {
            if (_onFetch == null || !append)
                _onFetch = onFetch;
            else
                _onFetch = x => { _onFetch(x); onFetch(x); };

            return this;
        }

        public CachedProxyConfigurationManager<T> OnError(Action<FunctionCacheErrorEvent> onError, bool append = true)
        {
            if (_onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

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
                _name,
                _serializers,
                _timeToLive ?? DefaultCacheSettings.TimeToLive,
                _memoryCacheMaxSizeMB ?? DefaultCacheSettings.MemoryCacheMaxSizeMB,
                _earlyFetchEnabled ?? DefaultCacheSettings.EarlyFetchEnabled,
                _cacheFactory,
                _onResult,
                _onFetch,
                _onError,
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