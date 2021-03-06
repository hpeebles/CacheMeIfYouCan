﻿using System;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal sealed class CachedFunctionWithSingleKeyConfiguration<TParams, TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<TParams, TimeSpan> TimeToLiveFactory { get; set; }
        public Func<TParams, bool> SkipCacheGetPredicate { get; set; }
        public Func<TParams, TValue, bool> SkipCacheSetPredicate { get; set; }
        public Action<SuccessfulRequestEvent<TParams, TKey, TValue>> OnSuccessAction { get; set; }
        public Action<ExceptionEvent<TParams, TKey>> OnExceptionAction { get; set; }
    }
}