using System;

namespace CacheMeIfYouCan.Notifications
{
    [Flags]
    public enum NotificationTypes
    {
        None,
        FunctionCacheGetResult = 0b0001,
        FunctionCacheFetchResult = 0b0010,
        FunctionCacheGetException = 0b0100,
        FunctionCacheFetchException = 0b1000,
        FunctionCacheAllExceptions = 0b1100,
        FunctionCacheAll = 0b1111,
        CacheGetResult = 0b0001_0000,
        CacheSetResult = 0b0010_0000,
        CacheGetException = 0b0100_0000,
        CacheSetException = 0b1000_0000,
        CacheAllExceptions = 0b1100_0000,
        CacheAll = 0b1111_0000,
        CachedObjectRefreshResult = 0b0001_0000_0000,
        CachedObjectRefreshException = 0b0010_0000_0000,
        All = Int32.MaxValue
    }
}