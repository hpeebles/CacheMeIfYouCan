using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class CachedFunctionConfigurationManagerSync<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerSync<TKey, TValue>>
    {
        public CachedFunctionConfigurationManagerSync(Func<TKey, TValue> originalFunc)
            : base((key, _) => Task.FromResult(originalFunc(key)))
        { }
        
        public Func<TKey, TValue> Build()
        {
            var cachedFunction = BuildCachedFunction();

            return Get;
            
            TValue Get(TKey key)
            {
                return Task.Run(() => cachedFunction.Get(key, CancellationToken.None)).GetAwaiter().GetResult();
            }
        }
    }
}