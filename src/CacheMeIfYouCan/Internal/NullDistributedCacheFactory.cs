using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal
{
    internal class NullDistributedCacheFactory : IDistributedCacheFactory
    {
        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config)
        {
            return null;
        }
    }
    
    internal class NullDistributedCacheFactory<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        public IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config)
        {
            return null;
        }
    }
}