using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal
{
    internal static class CachedInterfaceFactoryInternal
    {
        private static readonly ModuleBuilder ModuleBuilder;
        
        static CachedInterfaceFactoryInternal()
        {
            var thisAssemblyName = Assembly.GetExecutingAssembly().GetName();

            var newAssemblyName = new AssemblyName("CacheMeIfYouCan.CachedInterfaces");
            newAssemblyName.SetPublicKey(thisAssemblyName.GetPublicKey());
            newAssemblyName.SetPublicKeyToken(thisAssemblyName.GetPublicKeyToken());
            newAssemblyName.Version = thisAssemblyName.Version;
            
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                newAssemblyName,
                AssemblyBuilderAccess.Run);
            
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        }
        
        public static T Build<T>(T originalImpl, Dictionary<MethodInfo, object> configFunctions)
        {
            var interfaceType = typeof(T);

            if (!interfaceType.IsInterface)
                throw new InvalidOperationException("<T> must be an interface");
            
            var newType = CreateType(interfaceType, configFunctions);

            return (T)Activator.CreateInstance(newType, originalImpl, configFunctions);
        }

        private static Type CreateType(Type interfaceType, Dictionary<MethodInfo, object> configFunctions)
        {
            var typeName = GetProxyName(interfaceType);

            var typeBuilder = ModuleBuilder.DefineType(
                typeName,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);
            
            typeBuilder.AddInterfaceImplementation(interfaceType);
            
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { interfaceType, typeof(Dictionary<MethodInfo, object>) });

            var ctorGen = ctorBuilder.GetILGenerator();
            
            ctorGen.DeclareLocal(typeof(MemberInfo[]));

            // Call base constructor
            ctorGen.Emit(OpCodes.Ldarg_0);
            ctorGen.Emit(OpCodes.Call, typeof(Object).GetConstructor(new Type[0]));
            
            // Run InterfaceMethodsResolver.GetAllMethods(interfaceType) and store result in local variable
            ctorGen.Emit(OpCodes.Ldtoken, interfaceType);
            ctorGen.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new[] { typeof(RuntimeTypeHandle) }));
            ctorGen.Emit(OpCodes.Call, typeof(InterfaceMethodsResolver).GetMethod(nameof(InterfaceMethodsResolver.GetAllMethods), new[] { typeof(Type) }));
            ctorGen.Emit(OpCodes.Stloc_0);

            var allInterfaceMethods = InterfaceMethodsResolver.GetAllMethods(interfaceType);

            var dictionaryGetItemMethodInfo = typeof(Dictionary<MethodInfo, object>).GetMethod("get_Item");
            
            for (var index = 0; index < allInterfaceMethods.Length; index++)
            {
                var methodInfo = allInterfaceMethods[index];

                if (!configFunctions.TryGetValue(methodInfo, out var configFunction))
                    throw new Exception($"{interfaceType.Name}.{methodInfo.Name} has not been configured");

                var configFunctionType = configFunction.GetType();
                var configManagerType = configFunctionType.GenericTypeArguments[0];
                
                if (configManagerType.IsInterface)
                    configManagerType = ConfigurationManagerInterfaceTypeToConcreteTypeMap.GetConcreteType(configManagerType);
                
                var configManagerTypeCtor = configManagerType
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Single();
                
                ctorGen.DeclareLocal(configManagerType);
                ctorGen.DeclareLocal(configFunctionType);
                
                var fieldName = $"_{Char.ToLower(methodInfo.Name[0])}{methodInfo.Name.Substring(1)}{index}";
                var fieldType = BuildFieldType(methodInfo);
                var field = typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Private);
                var fieldCtor = fieldType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                
                // Create new CachedFunctionConfigurationManager object and pass function into ctor
                ctorGen.Emit(OpCodes.Ldarg_1);
                ctorGen.Emit(OpCodes.Dup);
                ctorGen.Emit(OpCodes.Ldvirtftn, methodInfo);
                ctorGen.Emit(OpCodes.Newobj, fieldCtor);
                ctorGen.Emit(OpCodes.Newobj, configManagerTypeCtor);
                ctorGen.Emit(OpCodes.Stloc, (2 * index) + 1);

                // Get the configuration action for this method and cast it to its specific type
                ctorGen.Emit(OpCodes.Ldarg_2);
                ctorGen.Emit(OpCodes.Ldloc_0);
                ctorGen.Emit(OpCodes.Ldc_I4, index);
                ctorGen.Emit(OpCodes.Ldelem_Ref);
                ctorGen.Emit(OpCodes.Callvirt, dictionaryGetItemMethodInfo);
                ctorGen.Emit(OpCodes.Castclass, configFunctionType);
                ctorGen.Emit(OpCodes.Stloc, (2 * index) + 2);

                // Run the configuration action on the CachedFunctionConfigurationManager
                ctorGen.Emit(OpCodes.Ldarg_0);
                ctorGen.Emit(OpCodes.Ldloc, (2 * index) + 2);
                ctorGen.Emit(OpCodes.Ldloc, (2 * index) + 1);
                ctorGen.Emit(OpCodes.Callvirt, configFunctionType.GetMethod("Invoke"));
                ctorGen.Emit(OpCodes.Callvirt, configFunctionType.GenericTypeArguments[1].GetMethod("Build"));
                ctorGen.Emit(OpCodes.Stfld, field);

                var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(); 
                
                // Build a method to implement the interface method by calling the function stored in the newly created field
                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    methodInfo.ReturnType,
                    parameterTypes);

                var methodGen = methodBuilder.GetILGenerator();
                
                // Load the function
                methodGen.Emit(OpCodes.Ldarg_0);
                methodGen.Emit(OpCodes.Ldfld, field);

                // Pass in each of the parameters
                for (var i = 1; i <= parameterTypes.Length; i++)
                    methodGen.Emit(OpCodes.Ldarg, i);

                // Invoke the function and return result
                methodGen.Emit(OpCodes.Callvirt, fieldType.GetMethod("Invoke", parameterTypes));
                methodGen.Emit(OpCodes.Ret);
            }

            // Return from the ctor
            ctorGen.Emit(OpCodes.Ret);
            
            return typeBuilder.CreateTypeInfo();
        }

        private static Type BuildFieldType(MethodInfo methodInfo)
        {
            var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = methodInfo.ReturnType;
            var isFunc = returnType != typeof(void);
            var genericArgs = isFunc
                ? parameterTypes.Append(returnType).ToArray()
                : parameterTypes;

            switch (genericArgs.Length)
            {
                case 0:
                    return typeof(Action);
                
                case 1:
                    return (isFunc ? typeof(Func<>) : typeof(Action<>)).MakeGenericType(genericArgs);
                
                case 2:
                    return (isFunc ? typeof(Func<,>) : typeof(Action<,>)).MakeGenericType(genericArgs);
                
                case 3:
                    return (isFunc ? typeof(Func<,,>) : typeof(Action<,,>)).MakeGenericType(genericArgs);
                
                case 4:
                    return (isFunc ? typeof(Func<,,,>) : typeof(Action<,,,>)).MakeGenericType(genericArgs);
                
                case 5:
                    return (isFunc ? typeof(Func<,,,,>) : typeof(Action<,,,,>)).MakeGenericType(genericArgs);
                
                case 6:
                    return (isFunc ? typeof(Func<,,,,,>) : typeof(Action<,,,,,>)).MakeGenericType(genericArgs);
                
                case 7:
                    return (isFunc ? typeof(Func<,,,,,,>) : typeof(Action<,,,,,,>)).MakeGenericType(genericArgs);
                
                case 8:
                    return (isFunc ? typeof(Func<,,,,,,,>) : typeof(Action<,,,,,,,>)).MakeGenericType(genericArgs);
                
                case 9:
                    return (isFunc ? typeof(Func<,,,,,,,,>) : typeof(Action<,,,,,,,,>)).MakeGenericType(genericArgs);
                
                case 10:
                    return (isFunc ? typeof(Func<,,,,,,,,,>) : typeof(Action<,,,,,,,,,>)).MakeGenericType(genericArgs);
                
                default:
                    throw new NotImplementedException();
            }
        }

        private static string GetProxyName(Type type)
        {
            return $"{type.Namespace}.{type.Name.Remove(0, type.Name.StartsWith("I") ? 1 : 0)}Proxy_{Guid.NewGuid()}";
        }
    }
}