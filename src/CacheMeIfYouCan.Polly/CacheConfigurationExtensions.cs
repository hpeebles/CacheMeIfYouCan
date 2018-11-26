using CacheMeIfYouCan.Configuration;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheConfigurationManagerExtensions
    {
        public static DistributedCacheConfigurationManager WithPolicy(
            this DistributedCacheConfigurationManager configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory(policy));
        }
        
        public static DistributedCacheConfigurationManager<TK, TV> WithPolicy<TK, TV>(
            this DistributedCacheConfigurationManager<TK, TV> configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory<TK, TV>(policy));
        }
    }
}