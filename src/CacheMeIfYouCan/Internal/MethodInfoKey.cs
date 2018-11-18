using System;
using System.Linq;
using System.Reflection;

namespace CacheMeIfYouCan.Internal
{
    internal readonly struct MethodInfoKey
    {
        public string MethodName { get; }
        public string ParameterTypeName { get; }
        
        public MethodInfoKey(Type interfaceType, MethodInfo methodInfo)
            :this($"{interfaceType.Name}.{methodInfo.Name}", methodInfo.GetParameters().Single().ParameterType.FullName)
        { }
        
        public MethodInfoKey(string methodName, string parameterTypeName)
        {
            MethodName = methodName;
            ParameterTypeName = parameterTypeName;
        }
        
        public bool Equals(MethodInfoKey other)
        {
            return
                String.Equals(MethodName, other.MethodName) &&
                String.Equals(ParameterTypeName, other.ParameterTypeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            
            return obj is MethodInfoKey key && Equals(key);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MethodName.GetHashCode() * 397) ^ ParameterTypeName.GetHashCode();
            }
        }
    }
}