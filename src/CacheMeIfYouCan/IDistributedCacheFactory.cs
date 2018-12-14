using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public interface IDistributedCacheFactory
    {
        bool RequiresStringKeys { get; }
        IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config);
    }

    public interface IDistributedCacheFactory<TK, TV>
    {
        bool RequiresStringKeys { get; }
        IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config);
    }

    internal class DistributedCacheFactoryGenericAdaptor<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private readonly IDistributedCacheFactory _factory;

        public DistributedCacheFactoryGenericAdaptor(IDistributedCacheFactory factory)
        {
            _factory = factory;
        }

        public bool RequiresStringKeys => _factory.RequiresStringKeys;
        
        public IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config)
        {
            return _factory.Build(config);
        }
    }

    internal class DistributedCacheFactoryFuncAdaptor<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private readonly Func<DistributedCacheConfig<TK, TV>, IDistributedCache<TK, TV>> _func;

        public DistributedCacheFactoryFuncAdaptor(Func<DistributedCacheConfig<TK, TV>, IDistributedCache<TK, TV>> func, bool requiresStringKeys)
        {
            _func = func;
            RequiresStringKeys = requiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }

        public IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config)
        {
            return _func(config);
        }
    }
}