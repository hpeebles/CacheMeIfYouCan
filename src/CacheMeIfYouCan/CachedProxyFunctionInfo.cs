using System;
using System.Reflection;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the details of a single method within an interface being proxied
    /// </summary>
    public sealed class CachedProxyFunctionInfo
    {
        internal CachedProxyFunctionInfo(Type interfaceType, MethodInfo methodInfo, Type keyType, Type valueType)
        {
            InterfaceType = interfaceType;
            MethodInfo = methodInfo;
            KeyType = keyType;
            ValueType = valueType;
        }
        
        /// <summary>
        /// The type of the interface being proxied
        /// </summary>
        public Type InterfaceType { get; }
        
        /// <summary>
        /// The method info for the method to be cached
        /// </summary>
        public MethodInfo MethodInfo { get; }
        
        /// <summary>
        /// The type of the cache key
        /// </summary>
        public Type KeyType { get; }
        
        /// <summary>
        /// The type of the cache value
        /// </summary>
        public Type ValueType { get; }
    }
}