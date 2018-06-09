namespace CacheMeIfYouCan.Internal
{
    internal static class MemoryCacheBuilder
    {
        public static ICache<T> Build<T>(long maxItems)
        {
            return new MemoryCache<T>(maxItems);
        }
    }
}