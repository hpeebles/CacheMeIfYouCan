using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class CachedFunctionConfigurationManagerAsync<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerAsync<TKey, TValue>>
    {
        public CachedFunctionConfigurationManagerAsync(Func<TKey, Task<TValue>> originalFunc)
            : base(originalFunc)
        { }

        public Func<TKey, Task<TValue>> Build()
        {
            var cachedFunction = BuildCachedFunction();

            return cachedFunction.Get;
        }
    }
}