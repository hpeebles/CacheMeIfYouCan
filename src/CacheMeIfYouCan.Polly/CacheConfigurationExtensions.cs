using CacheMeIfYouCan.Configuration;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheConfigurationManagerExtensions
    {
        public static CacheConfigurationManager WithPolicy(
            this CacheConfigurationManager configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory(policy));
        }
        
        public static CacheConfigurationManager<TK, TV> WithPolicy<TK, TV>(
            this CacheConfigurationManager<TK, TV> configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory<TK, TV>(policy));
        }
    }
}