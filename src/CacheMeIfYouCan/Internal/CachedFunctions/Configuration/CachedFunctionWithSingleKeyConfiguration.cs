using System;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal sealed class CachedFunctionWithSingleKeyConfiguration<TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<TKey, TimeSpan> TimeToLiveFactory { get; set; }
    }
}