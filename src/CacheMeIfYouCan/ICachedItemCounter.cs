namespace CacheMeIfYouCan
{
    public interface ICachedItemCounter
    {
        string CacheName { get; }
        string CacheType { get; }
        long Count { get; }
    }
}