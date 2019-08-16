using System;

namespace CacheMeIfYouCan.Internal
{
    internal static class TypeNameHelper
    {
        public static string GetNameIncludingInnerGenericTypeNames(Type type, int maxDepth = 3)
        {
            if (!type.IsGenericType || maxDepth == 0)
                return type.Name;

            var fullName = type.Name;

            foreach (var genericType in type.GetGenericArguments())
                fullName += "_" + GetNameIncludingInnerGenericTypeNames(genericType, maxDepth - 1);

            return fullName;
        }
    }
}