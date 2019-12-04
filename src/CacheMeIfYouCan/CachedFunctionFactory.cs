using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public static class CachedFunctionFactory
    {
        public static CachedFunctionConfigurationManager<TKey, TValue> ConfigureFor<TKey, TValue>(Func<TKey, Task<TValue>> originalFunction)
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManager<TKey, TValue>(originalFunction);
        }
    }
}