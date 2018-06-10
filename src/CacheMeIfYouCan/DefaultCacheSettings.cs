﻿using System;

namespace CacheMeIfYouCan
{
    public static class DefaultCacheSettings
    {
        public static TimeSpan TimeToLive = TimeSpan.FromHours(1);
        public static int MaxItemsInMemoryCache = 1000000;
        public static int MaxConcurrentFetches = 100;
        public static bool EarlyFetchEnabled = true;
        public static Action<FunctionCacheErrorEvent> OnError = null;
    }
}