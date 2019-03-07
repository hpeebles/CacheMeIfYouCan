namespace CacheMeIfYouCan.Configuration
{
    public class LocalCacheConfig<TK> : CacheConfig<TK>
    {
        public LocalCacheConfig(string name = null, bool setDefaults = false)
            : base(name, setDefaults)
        { }
    }
}
