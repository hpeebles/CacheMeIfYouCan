using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class CachedFunctionConfigurationManagerSync<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerSync<TKey, TValue>>
    {
        public CachedFunctionConfigurationManagerSync(Func<TKey, TValue> originalFunc)
            : base(key => Task.FromResult(originalFunc(key)))
        { }
        
        public Func<TKey, TValue> Build()
        {
            var cachedFunction = BuildCachedFunction();

            return k => Task.Run(() => cachedFunction.Get(k)).GetAwaiter().GetResult();
        }
    }
}