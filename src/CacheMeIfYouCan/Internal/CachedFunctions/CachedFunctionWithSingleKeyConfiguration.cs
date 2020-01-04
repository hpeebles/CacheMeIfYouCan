using System;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithSingleKeyConfiguration<TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<TKey, TimeSpan> TimeToLiveFactory { get; set; }
    }
}