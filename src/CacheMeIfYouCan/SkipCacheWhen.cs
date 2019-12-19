namespace CacheMeIfYouCan
{
    public enum SkipCacheWhen
    {
        SkipCacheGet = 1,
        SkipCacheSet = 2,
        SkipCacheGetAndCacheSet = SkipCacheGet | SkipCacheSet
    }
}