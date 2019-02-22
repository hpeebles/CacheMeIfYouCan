namespace CacheMeIfYouCan.ApplicationInsights
{
    internal class LocalCacheApplicationInsightsWrapperFactory : ILocalCacheWrapperFactory
    {
        private readonly CacheApplicationInsightsConfig _config;

        public LocalCacheApplicationInsightsWrapperFactory(CacheApplicationInsightsConfig config)
        {
            _config = config;
        }

        public ILocalCache<TK, TV> Wrap<TK, TV>(ILocalCache<TK, TV> cache)
        {
            return new LocalCacheApplicationInsightsWrapper<TK, TV>(cache, _config);
        }
    }
}