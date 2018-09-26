﻿using System;
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
        private readonly KeySerializers _keySerializers;
        private readonly ValueSerializers _valueSerializers;
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
            _keySerializers = new KeySerializers();
            _valueSerializers = new ValueSerializers();
            _functionCacheConfigActions = new Dictionary<MethodInfoKey, object>();
        }
                
        public CachedProxyConfigurationManager<T> For(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithKeySerializer<TField>(Func<TField, string> serializer)
        {
            _keySerializers.Set(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithKeySerializer<TField>(IKeySerializer serializer)
        {
            _keySerializers.Set<TField>(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithKeySerializer<TField>(IKeySerializer<TField> serializer)
        {
            _keySerializers.Set(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithDefaultKeySerializer(Func<object, string> serializer)
        {
            _keySerializers.SetDefault(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithDefaultKeySerializer(IKeySerializer serializer)
        {
            _keySerializers.SetDefault(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithValueSerializer<TField>(Func<TField, string> serializer, Func<string, TField> deserializer)
        {
            _valueSerializers.Set(serializer, deserializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithValueSerializer<TField>(ISerializer serializer)
        {
            _valueSerializers.Set<TField>(serializer);
            return this;
        }
        
        public CachedProxyConfigurationManager<T> WithValueSerializer<TField>(ISerializer<TField> serializer)
        {
            _valueSerializers.Set(serializer);
            return this;
        }

        public CachedProxyConfigurationManager<T> WithDefaultValueSerializer(ISerializer serializer)
        {
            _valueSerializers.SetDefault(serializer);
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
                typeof(T),
                _keySerializers,
                _valueSerializers,
                _timeToLive ?? DefaultCacheSettings.TimeToLive,
                _memoryCacheMaxSizeMB ?? DefaultCacheSettings.MemoryCacheMaxSizeMB,
                _earlyFetchEnabled ?? DefaultCacheSettings.EarlyFetchEnabled,
                _cacheFactory ?? DefaultCacheSettings.CacheFactory,
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