using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class CachedFunctionConfigurationManagerSyncCanx<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerSyncCanx<TKey, TValue>>
    {
        public CachedFunctionConfigurationManagerSyncCanx(Func<TKey, CancellationToken, TValue> originalFunc)
            : base((key, cancellationToken) => Task.FromResult(originalFunc(key, cancellationToken)))
        { }
        
        public Func<TKey, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildCachedFunction();

            return Get;

            TValue Get(TKey key, CancellationToken cancellationToken)
            {
                return Task.Run(() => cachedFunction.Get(key, cancellationToken)).GetAwaiter().GetResult();
            }
        }
    }
}