using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public interface IDistributedCacheFactory
    {
        IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config);
    }

    public interface IDistributedCacheFactory<TK, TV>
    {
        IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config);
    }

    internal class DistributedCacheFactoryToGenericAdapter<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private readonly IDistributedCacheFactory _factory;

        public DistributedCacheFactoryToGenericAdapter(IDistributedCacheFactory factory)
        {
            _factory = factory;
        }
        
        public IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config)
        {
            return _factory.Build(config);
        }
    }

    internal class DistributedCacheFactoryFromFuncAdapter<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private readonly Func<DistributedCacheConfig<TK, TV>, IDistributedCache<TK, TV>> _func;

        public DistributedCacheFactoryFromFuncAdapter(Func<DistributedCacheConfig<TK, TV>, IDistributedCache<TK, TV>> func)
        {
            _func = func;
        }
        
        public IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config)
        {
            return _func(config);
        }
    }
}