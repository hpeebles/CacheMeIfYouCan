using System;
using System.Linq;
using System.Reflection;

namespace CacheMeIfYouCan.Internal
{
    internal static class InterfaceMethodsResolver
    {
        public static MethodInfo[] GetAllMethods(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new Exception($"Type must be an interface - '{interfaceType.FullName}'");
            
            var implementedInterfaces = interfaceType.GetTypeInfo().ImplementedInterfaces?.ToArray();

            if (implementedInterfaces == null || !implementedInterfaces.Any())
                return interfaceType.GetMethods();

            return interfaceType.GetMethods()
                .Concat(implementedInterfaces.SelectMany(i => i.GetMethods()))
                .ToArray();
        }
    }
}