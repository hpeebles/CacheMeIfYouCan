using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheConfigurationManagerExtensions
    {
        public static IDistributedCacheFactory WithPolicy(
            this IDistributedCacheFactory configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory(policy));
        }
        
        public static IDistributedCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this IDistributedCacheFactory<TK, TV> configurationManager,
            Policy policy)
        {
            return configurationManager.AddWrapper(new CachePollyWrapperFactory<TK, TV>(policy));
        }
    }
}