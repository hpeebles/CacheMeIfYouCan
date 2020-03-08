using System;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal sealed class CachedFunctionWithSingleKeyConfiguration<TParams, TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<TKey, TimeSpan> TimeToLiveFactory { get; set; }
        public Action<CachedFunctionWithSingleKeyResult_MultiParam_Success<TParams, TKey, TValue>> OnSuccessAction { get; set; }
        public Action<CachedFunctionWithSingleKeyResult_MultiParam_Exception<TParams, TKey>> OnExceptionAction { get; set; }
    }
}