using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerSync<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerSync<TKey, TValue>>
    {
        private readonly Func<TKey, TValue> _originalFunction;

        public CachedFunctionConfigurationManagerSync(Func<TKey, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }
        
        public Func<TKey, TValue> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            return Get;
            
            TValue Get(TKey key)
            {
                return Task.Run(() => cachedFunction.Get(key, CancellationToken.None)).GetAwaiter().GetResult();
            }
        }
        
        private Func<TKey, CancellationToken, Task<TValue>> ConvertFunction()
        {
            return (keys, _) => Task.FromResult(_originalFunction(keys));
        }
    }
}