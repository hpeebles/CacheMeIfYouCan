using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public sealed partial class CachedInterfaceConfigurationManager<T>
    {
        private readonly T _originalImpl;
        private readonly HashSet<MethodInfo> _allInterfaceMethods; 
        private readonly Dictionary<MethodInfo, object> _functionCacheConfigurationFunctions;

        internal CachedInterfaceConfigurationManager(T originalImpl)
        {
            _originalImpl = originalImpl;
            _allInterfaceMethods = new HashSet<MethodInfo>(InterfaceMethodsResolver.GetAllMethods(typeof(T)));
            _functionCacheConfigurationFunctions = new Dictionary<MethodInfo, object>();
        }

        public T Build() => CachedInterfaceFactoryInternal.Build(_originalImpl, _functionCacheConfigurationFunctions);

        private void AddConfigFunc<TFunc>(Expression<Func<T, TFunc>> expression, object configurationFunc)
        {
            var methodInfo = GetMethodInfo(expression);
            
            if (_functionCacheConfigurationFunctions.ContainsKey(methodInfo))
                throw new Exception($"Duplicate configuration for {methodInfo}");
            
            _functionCacheConfigurationFunctions.Add(methodInfo, configurationFunc);
        }
        
        private MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodCallObject = (ConstantExpression)methodCallExpression.Object;
            var methodInfo = (MethodInfo)methodCallObject.Value;
            
            if (!_allInterfaceMethods.Contains(methodInfo))
                throw new InvalidOperationException($"Expression:'{expression.Body} does not match any methods on {typeof(T).Name}");

            return methodInfo;
        }
    }
}