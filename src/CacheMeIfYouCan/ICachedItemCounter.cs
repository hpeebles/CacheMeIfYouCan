namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents an object which is able to count the number of items in a cache
    /// </summary>
    public interface ICachedItemCounter
    {
        string CacheName { get; }
        string CacheType { get; }
        long Count { get; }
    }
}