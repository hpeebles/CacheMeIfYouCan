using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>>
    {
        private readonly Func<TKey, CancellationToken, Task<TValue>> _originalFunction;

        public CachedFunctionConfigurationManagerAsyncCanx(Func<TKey, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TKey, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildCachedFunction(_originalFunction);

            return cachedFunction.Get;
        }
    }
}