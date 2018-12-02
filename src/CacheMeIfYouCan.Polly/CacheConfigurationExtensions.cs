using CacheMeIfYouCan.Configuration;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheConfigurationManagerExtensions
    {
        public static DistributedCacheFactoryConfigurationManager WithPolicy(
            this DistributedCacheFactoryConfigurationManager configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory(policy));
        }
        
        public static DistributedCacheFactoryConfigurationManager<TK, TV> WithPolicy<TK, TV>(
            this DistributedCacheFactoryConfigurationManager<TK, TV> configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory<TK, TV>(policy));
        }
    }
}