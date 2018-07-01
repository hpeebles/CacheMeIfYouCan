using System;

namespace CacheMeIfYouCan
{
    public class FunctionInfo
    {
        public readonly Type InterfaceType;
        public readonly string FunctionName;
        public readonly Type ParameterType;
        public readonly Type ReturnType;

        internal FunctionInfo(Type interfaceType, string functionName, Type parameterType, Type returnType)
        {
            InterfaceType = interfaceType;
            FunctionName = functionName;
            ParameterType = parameterType;
            ReturnType = returnType;
        }
    }
}