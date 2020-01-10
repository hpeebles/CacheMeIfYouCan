using System;
using System.Linq;
using System.Reflection;

namespace CacheMeIfYouCan.Internal
{
    internal static class InterfaceMethodsResolver
    {
        public static MethodInfo[] GetAllMethods(Type interfaceType)
        {
            return GetAllMethodsUnordered(interfaceType)
                .OrderBy(MethodInfoToStringInjective)
                .ToArray();
        }

        private static MethodInfo[] GetAllMethodsUnordered(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new Exception($"Type must be an interface - '{interfaceType.FullName}'");
            
            var implementedInterfaces = interfaceType.GetTypeInfo().ImplementedInterfaces?.ToList();

            if (implementedInterfaces == null || !implementedInterfaces.Any())
                return interfaceType.GetMethods();

            return interfaceType.GetMethods()
                .Concat(implementedInterfaces.SelectMany(i => i.GetMethods()))
                .ToArray();
        }

        private static string MethodInfoToStringInjective(MethodInfo methodInfo)
        {
            var parameterTypesString = String.Join(
                "_",
                methodInfo.GetParameters().Select(p => GetTypeString(p.ParameterType)));

            return $"{methodInfo.Name}|{parameterTypesString}";

            static string GetTypeString(Type type)
            {
                return $"{type.FullName}_{type.Assembly}";
            }
        }
    }
}