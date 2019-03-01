﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

// Each type built by this class gets put into a dynamically built assembly called CachedProxyFactoryAsm
[assembly: InternalsVisibleTo("CachedProxyFactoryAsm")]
namespace CacheMeIfYouCan.Internal
{
    internal class CachedProxyFactory
    {
        private static readonly ModuleBuilder ModuleBuilder;
        
        static CachedProxyFactory()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("CachedProxyFactoryAsm"),
                AssemblyBuilderAccess.Run);
            
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        }

        internal static T Build<T>(T impl, CachedProxyConfig config)
        {
            var interfaceType = typeof(T);

            ValidateType(interfaceType);
            
            var newTypeName = GetProxyName(interfaceType);

            var newType = CreateType(interfaceType, newTypeName);

            return (T)Activator.CreateInstance(newType, impl, config);
        }

        // The IL generated here is based on the IL generated by SampleProxyILTemplate
        private static Type CreateType(Type interfaceType, string name)
        {
            var typeBuilder = ModuleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);
            typeBuilder.AddInterfaceImplementation(interfaceType);
            
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { interfaceType, typeof(CachedProxyConfig) });

            var ctorGen = ctorBuilder.GetILGenerator();
            
            ctorGen.DeclareLocal(typeof(MemberInfo[]));

            ctorGen.Emit(OpCodes.Ldarg_0); // this
            ctorGen.Emit(OpCodes.Call, typeof(Object).GetConstructor(new Type[0]));
            
            ctorGen.Emit(OpCodes.Ldtoken, interfaceType);
            ctorGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
            ctorGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetMethods", new Type[0]));
            ctorGen.Emit(OpCodes.Stloc_0);

            var methods = interfaceType.GetMethods();
            
            for (var index = 0; index < methods.Length; index++)
            {
                var methodInfo = methods[index];
                var definition = GetMethodDefinition(methodInfo);
                
                // Create a field called _methodName of type Func<TK, Task<TV>> in which to store the cached function
                var fieldName = $"_{Char.ToLower(methodInfo.Name[0])}{methodInfo.Name.Substring(1)}_{index}";
                var field = typeBuilder.DefineField(fieldName, definition.FuncType, FieldAttributes.Private);
                var fieldCtor = definition.FuncType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                
                // Get the relevant configuration manager type based on whether the func is single key or enumerable key
                // and sync or async
                var configManagerType = GetConfigManagerType(definition);
                var configManagerTypeCtor = configManagerType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { definition.FuncType, typeof(CachedProxyConfig), typeof(MethodInfo) },
                    new ParameterModifier[0]);
                
                // Build a cached version of the function and store it in the newly created field
                ctorGen.Emit(OpCodes.Ldarg_0); // this
                ctorGen.Emit(OpCodes.Ldarg_1); // T impl
                ctorGen.Emit(OpCodes.Dup);
                ctorGen.Emit(OpCodes.Ldvirtftn, methodInfo); // impl.MethodName
                ctorGen.Emit(OpCodes.Newobj, fieldCtor); // Func<TK, Task<TV>> .ctor(object, IntPointer)
                ctorGen.Emit(OpCodes.Ldarg_2); // CachedProxyConfig config
                ctorGen.Emit(OpCodes.Ldloc_0);
                ctorGen.Emit(OpCodes.Ldc_I4, index);
                ctorGen.Emit(OpCodes.Ldelem_Ref);
                ctorGen.Emit(OpCodes.Newobj, configManagerTypeCtor); // FunctionCacheConfigurationManager .ctor(Func<TK, Task<TV>>, string, CachedProxyConfig)
                ctorGen.Emit(OpCodes.Call, configManagerType.GetMethod("Build")); // FunctionCacheConfigurationManager.Build()
                ctorGen.Emit(OpCodes.Stfld, field); // Store result in _methodName
                
                // Build a method to implement the interface method by calling the function stored in the newly created field
                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    methodInfo.ReturnType,
                    definition.ParameterTypes);
                
                var methodGen = methodBuilder.GetILGenerator();
                
                methodGen.Emit(OpCodes.Ldarg_0); // this
                methodGen.Emit(OpCodes.Ldfld, field); // _methodName
                
                for (var i = 1; i <= definition.ParameterTypes.Length; i++)
                    methodGen.Emit(OpCodes.Ldarg, i); // Load each of the keys
                
                methodGen.Emit(OpCodes.Callvirt, definition.FuncType.GetMethod("Invoke", definition.ParameterTypes)); // _methodName.Invoke(key)
                methodGen.Emit(OpCodes.Ret); // Return result
            }
            
            ctorGen.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo();
        }

        private static void ValidateType(Type type)
        {
            if (!type.IsInterface)
                throw new InvalidOperationException("<T> must be an interface");

            foreach (var methodInfo in type.GetMethods())
            {
                var returnType = methodInfo.ReturnType;
                if (returnType == typeof(void) || returnType == typeof(Task))
                {
                    throw new Exception($@"
Method must return a value. '{type.FullName}.{methodInfo.Name}'");
                }
            }
        }

        private static MethodDefinition GetMethodDefinition(MethodInfo methodInfo)
        {
            var parameterTypes = methodInfo
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var hasCancellation = parameterTypes.Length > 1 && parameterTypes.Last() == typeof(CancellationToken);
            
            var isAsync = typeof(Task).IsAssignableFrom(methodInfo.ReturnType);

            var returnType = methodInfo.ReturnType;
            var returnTypeInner = isAsync
                ? returnType.GenericTypeArguments.Single()
                : returnType;

            var isEnumerableKey = IsEnumerableKeyFunc(out var keyType, out var valueType);

            var funcTypes = parameterTypes
                .Concat(new[] { returnType })
                .ToArray();

            Type funcTypeGeneric;
            switch (funcTypes.Length)
            {
                case 2:
                    funcTypeGeneric = typeof(Func<,>);
                    break;

                case 3:
                    funcTypeGeneric = typeof(Func<,,>);
                    break;

                case 4:
                    funcTypeGeneric = typeof(Func<,,,>);
                    break;

                case 5:
                    funcTypeGeneric = typeof(Func<,,,,>);
                    break;

                case 6:
                    funcTypeGeneric = typeof(Func<,,,,,>);
                    break;

                default:
                    throw new Exception("Only functions with up to 4 input parameters are supported");
            }

            var funcType = funcTypeGeneric.MakeGenericType(funcTypes);
            
            return new MethodDefinition
            {
                FuncType = funcType,
                ParameterTypes = parameterTypes,
                ReturnType = returnType,
                ReturnTypeInner = returnTypeInner,
                KeyType = keyType,
                ValueType = valueType,
                IsEnumerableKey = isEnumerableKey,
                IsAsync = isAsync,
                HasCancellation = hasCancellation
            };

            bool IsEnumerableKeyFunc(out Type _keyType, out Type _valueType)
            {
                var lastKeyParam = hasCancellation ? parameterTypes[parameterTypes.Length - 1] : parameterTypes.Last();
            
                if (lastKeyParam != typeof(String) &&
                    IsEnumerable(lastKeyParam, out _keyType) &&
                    IsDictionary(returnTypeInner, out var returnKeyType, out _valueType))
                {
                    if (_keyType != returnKeyType)
                        throw new Exception(@"
The key type in the returned dictionary must match the type of the items in the input parameter");

                    return true;
                }

                _keyType = null;
                _valueType = null;
                return false;
            }
        }

        private static Type GetConfigManagerType(MethodDefinition definition)
        {
            Type configManagerGenericType;
            Type[] genericTypeInputs;

            return definition.IsEnumerableKey
                ? BuildForEnumerableKey()
                : BuildForSingleKey();
            
            Type BuildForEnumerableKey()
            {
                genericTypeInputs = definition
                    .ParameterTypes
                    .Concat(new[] { definition.ReturnTypeInner, definition.KeyType, definition.ValueType })
                    .ToArray();

                switch (definition.HasCancellation ? genericTypeInputs.Length - 1 : genericTypeInputs.Length)
                {
                    case 5:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<,,,,>)
                                : typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<,,,,>)
                            : definition.HasCancellation
                                ? typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<,,,,>)
                                : typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<,,,,>);
                        break;

                    case 6:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<,,,,,>)
                                : typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<,,,,,>)
                            : definition.HasCancellation
                                ? typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<,,,,,>)
                                : typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<,,,,,>);
                        break;

                    case 7:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<,,,,,,>)
                                : typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<,,,,,,>)
                            : definition.HasCancellation
                                ? typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<,,,,,,>)
                                : typeof(MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<,,,,,,>);
                        break;
                
                    default:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(EnumerableKeyFunctionCacheConfigurationManagerCanx<,,,>)
                                : typeof(EnumerableKeyFunctionCacheConfigurationManagerNoCanx<,,,>)
                            : definition.HasCancellation
                                ? typeof(EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<,,,>)
                                : typeof(EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<,,,>);
                        break;
                }

                return configManagerGenericType.MakeGenericType(genericTypeInputs);
            }

            Type BuildForSingleKey()
            {
                genericTypeInputs = definition
                    .ParameterTypes
                    .Concat(new[] { definition.ReturnTypeInner })
                    .ToArray();

                switch (genericTypeInputs.Length)
                {
                    case 3:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(MultiParamFunctionCacheConfigurationManagerCanx<,,>)
                                : typeof(MultiParamFunctionCacheConfigurationManagerNoCanx<,,>)
                            : definition.HasCancellation
                                ? typeof(MultiParamFunctionCacheConfigurationManagerSyncCanx<,,>)
                                : typeof(MultiParamFunctionCacheConfigurationManagerSyncNoCanx<,,>);
                        break;

                    case 4:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(MultiParamFunctionCacheConfigurationManagerCanx<,,,>)
                                : typeof(MultiParamFunctionCacheConfigurationManagerNoCanx<,,,>)
                            : definition.HasCancellation
                                ? typeof(MultiParamFunctionCacheConfigurationManagerSyncCanx<,,,>)
                                : typeof(MultiParamFunctionCacheConfigurationManagerSyncNoCanx<,,,>);
                        break;

                    case 5:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(MultiParamFunctionCacheConfigurationManagerCanx<,,,,>)
                                : typeof(MultiParamFunctionCacheConfigurationManagerNoCanx<,,,,>)
                            : definition.HasCancellation
                                ? typeof(MultiParamFunctionCacheConfigurationManagerSyncCanx<,,,,>)
                                : typeof(MultiParamFunctionCacheConfigurationManagerSyncNoCanx<,,,,>);
                        break;
                
                    default:
                        configManagerGenericType = definition.IsAsync
                            ? definition.HasCancellation
                                ? typeof(SingleKeyFunctionCacheConfigurationManagerCanx<,>)
                                : typeof(SingleKeyFunctionCacheConfigurationManagerNoCanx<,>)
                            : definition.HasCancellation
                                ? typeof(SingleKeyFunctionCacheConfigurationManagerSyncCanx<,>)
                                : typeof(SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<,>);
                        break;
                }
                
                return configManagerGenericType.MakeGenericType(genericTypeInputs);
            }
        }

        private static string GetProxyName(Type type)
        {
            var name = $"{type.Namespace}.{type.Name.Remove(0, type.Name.StartsWith("I") ? 1 : 0)}Proxy_{Guid.NewGuid()}";

            return name;
        }
        
        private static bool IsEnumerable(Type type, out Type innerType)
        {
            foreach (var i in new[] { type }.Concat(type.GetInterfaces()))
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    innerType = i.GenericTypeArguments.Single();
                    return true;
                }
            }

            innerType = null;
            return false;
        }

        private static bool IsDictionary(Type type, out Type keyType, out Type valueType)
        {
            foreach (var i in new[] { type }.Concat(type.GetInterfaces()))
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    keyType = i.GenericTypeArguments[0];
                    valueType = i.GenericTypeArguments[1];
                    return true;
                }
            }

            keyType = null;
            valueType = null;
            return false;
        }

        private class MethodDefinition
        {
            public Type FuncType;
            public Type[] ParameterTypes;
            public Type ReturnType;
            public Type ReturnTypeInner;
            public Type KeyType; // Only populated for enumerable key functions
            public Type ValueType; // Only populated for enumerable key functions
            public bool IsEnumerableKey;
            public bool IsAsync;
            public bool HasCancellation;
        }
    }
}