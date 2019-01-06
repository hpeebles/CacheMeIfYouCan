namespace CacheMeIfYouCan
{
    /// <summary>
    /// Use this to build <see cref="ICache{TK,TV}"/> instances using a two tier caching strategy made up of a local
    /// cache backed by a distributed cache
    /// </summary>
    public sealed class TwoTierCacheFactory
    {
        private readonly IDistributedCacheFactory _distributedCacheFactory;
        private readonly ILocalCacheFactory _localCacheFactory;

        public TwoTierCacheFactory(
            IDistributedCacheFactory distributedCacheFactory,
            ILocalCacheFactory localCacheFactory)
        {
            _distributedCacheFactory = distributedCacheFactory;
            _localCacheFactory = localCacheFactory;
        }

        public ICache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new TwoTierCache<TK, TV>(
                _distributedCacheFactory.Build<TK, TV>(cacheName),
                _localCacheFactory.Build<TK, TV>(cacheName));
        }
    }
}