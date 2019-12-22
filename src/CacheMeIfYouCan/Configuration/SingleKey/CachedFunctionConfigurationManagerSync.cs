using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerSync<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerSync<TKey, TValue>>
    {
        private readonly Func<TKey, TValue> _originalFunc;

        public CachedFunctionConfigurationManagerSync(Func<TKey, TValue> originalFunc)
        {
            _originalFunc = originalFunc;
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
            return (keys, _) => Task.FromResult(_originalFunc(keys));
        }
    }
}