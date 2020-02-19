using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Configuration.SingleKey;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal static class ConfigurationManagerInterfaceTypeToConcreteTypeMap
    {
        private static readonly Dictionary<Type, Type> Map = BuildTypeMap();

        public static Type GetConcreteType(Type interfaceType)
        {
            var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

            if (!Map.TryGetValue(genericTypeDefinition, out var concreteGenericTypeDefinition))
                throw new InvalidOperationException("Type not supported " + interfaceType);

            return concreteGenericTypeDefinition.MakeGenericType(interfaceType.GenericTypeArguments);
        }

        private static Dictionary<Type, Type> BuildTypeMap()
        {
            return new Dictionary<Type, Type>
            {
                { typeof(ICachedFunctionConfigurationManagerAsync_1Param_KeySelector<,>), typeof(CachedFunctionConfigurationManagerAsync_1Param<,>) },
                { typeof(ICachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<,>), typeof(CachedFunctionConfigurationManagerAsyncCanx_1Param<,>) },
                { typeof(ICachedFunctionConfigurationManagerSync_1Param_KeySelector<,>), typeof(CachedFunctionConfigurationManagerSync_1Param<,>) },
                { typeof(ICachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<,>), typeof(CachedFunctionConfigurationManagerSyncCanx_1Param<,>) }
            };
        }
    }
}