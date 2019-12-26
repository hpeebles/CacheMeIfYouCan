using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerAsync<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerAsync<TKey, TValue>>
    {
        private readonly Func<TKey, Task<TValue>> _originalFunction;

        public CachedFunctionConfigurationManagerAsync(Func<TKey, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TKey, Task<TValue>> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            return key => cachedFunction.Get(key, CancellationToken.None);
        }

        private Func<TKey, CancellationToken, Task<TValue>> ConvertFunction()
        {
            return (keys, _) => _originalFunction(keys);
        }
    }
}