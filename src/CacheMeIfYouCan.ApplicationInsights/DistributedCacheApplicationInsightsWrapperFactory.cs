using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.ApplicationInsights
{
    internal class DistributedCacheApplicationInsightsWrapperFactory : IDistributedCacheWrapperFactory
    {
        private readonly CacheApplicationInsightsConfig _config;

        public DistributedCacheApplicationInsightsWrapperFactory(CacheApplicationInsightsConfig config)
        {
            _config = config;
        }

        public IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache, IDistributedCacheConfig<TK, TV> config)
        {
            return new DistributedCacheApplicationInsightsWrapper<TK, TV>(cache, _config);
        }
    }
}