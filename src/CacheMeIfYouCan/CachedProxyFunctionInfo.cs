using System;
using System.Reflection;

namespace CacheMeIfYouCan
{
    public class CachedProxyFunctionInfo
    {
        public readonly Type InterfaceType;
        public readonly MethodInfo MethodInfo;
        public readonly Type KeyType;
        public readonly Type ValueType;

        internal CachedProxyFunctionInfo(Type interfaceType, MethodInfo methodInfo, Type keyType, Type valueType)
        {
            InterfaceType = interfaceType;
            MethodInfo = methodInfo;
            KeyType = keyType;
            ValueType = valueType;
        }
    }
}