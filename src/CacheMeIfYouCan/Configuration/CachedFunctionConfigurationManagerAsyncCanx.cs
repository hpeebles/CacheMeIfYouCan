using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>>
    {
        public CachedFunctionConfigurationManagerAsyncCanx(Func<TKey, CancellationToken, Task<TValue>> originalFunc)
            : base(originalFunc)
        { }

        public Func<TKey, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildCachedFunction();

            return cachedFunction.Get;
        }
    }
}