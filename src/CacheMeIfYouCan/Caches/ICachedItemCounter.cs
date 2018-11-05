namespace CacheMeIfYouCan.Caches
{
    public interface ICachedItemCounter
    {
        string CacheType { get; }
        FunctionInfo FunctionInfo { get; }
        long Count { get; }
    }
}