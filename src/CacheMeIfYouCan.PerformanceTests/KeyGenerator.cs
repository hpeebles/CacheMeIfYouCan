using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.PerformanceTests
{
    public static class KeyGenerator
    {
        public static IList<T> Generate<T>(int count)
        {
            var generateKeyFunc = GenerateKeyFunc<T>();
            
            return Enumerable
                .Range(0, count)
                .Select(generateKeyFunc)
                .ToArray();
        }

        private static Func<int, T> GenerateKeyFunc<T>()
        {
            if (typeof(T) == typeof(int))
                return i => (T)(object)i;
            
            if (typeof(T) == typeof(string))
                return i => (T)(object)Guid.NewGuid().ToString();
            
            if (typeof(T) == typeof(Guid))
                return i => (T)(object)Guid.NewGuid();

            throw new Exception();
        }
    }
}