namespace CacheMeIfYouCan
{
    /// <summary>
    /// Use this to build <see cref="ICache{TK,TV}"/> instances using a two tier caching strategy made up of a local
    /// cache backed by a distributed cache
    /// </summary>
    public sealed class TwoTierCacheFactory
    {
        private readonly ILocalCacheFactory _localCacheFactory;
        private readonly IDistributedCacheFactory _distributedCacheFactory;

        public TwoTierCacheFactory(
            ILocalCacheFactory localCacheFactory,
            IDistributedCacheFactory distributedCacheFactory)
        {
            _localCacheFactory = localCacheFactory;
            _distributedCacheFactory = distributedCacheFactory;
        }

        public ICache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new TwoTierCache<TK, TV>(
                _localCacheFactory.Build<TK, TV>(cacheName),
                _distributedCacheFactory.Build<TK, TV>(cacheName));
        }
    }
}