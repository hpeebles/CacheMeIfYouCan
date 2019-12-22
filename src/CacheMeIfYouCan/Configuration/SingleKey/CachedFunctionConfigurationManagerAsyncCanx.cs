using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>>
    {
        private readonly Func<TKey, CancellationToken, Task<TValue>> _originalFunc;

        public CachedFunctionConfigurationManagerAsyncCanx(Func<TKey, CancellationToken, Task<TValue>> originalFunc)
        {
            _originalFunc = originalFunc;
        }

        public Func<TKey, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildCachedFunction(_originalFunc);

            return cachedFunction.Get;
        }
    }
}