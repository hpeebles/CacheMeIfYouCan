using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public interface ILocalCacheFactory
    {
        ILocalCache<TK, TV> Build<TK, TV>(ILocalCacheConfig<TK> config);
    }
    
    public interface ILocalCacheFactory<TK, TV>
    {
        ILocalCache<TK, TV> Build(ILocalCacheConfig<TK> config);
    }

    internal class LocalCacheFactoryToGenericAdapter<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory _factory;

        public LocalCacheFactoryToGenericAdapter(ILocalCacheFactory factory)
        {
            _factory = factory;
        }
        
        public ILocalCache<TK, TV> Build(ILocalCacheConfig<TK> config)
        {
            return _factory.Build<TK, TV>(config);
        }
    }
    
    internal class LocalCacheFactoryFromFuncAdapter<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly Func<ILocalCacheConfig<TK>, ILocalCache<TK, TV>> _func;

        public LocalCacheFactoryFromFuncAdapter(Func<ILocalCacheConfig<TK>, ILocalCache<TK, TV>> func)
        {
            _func = func;
        }

        public ILocalCache<TK, TV> Build(ILocalCacheConfig<TK> config)
        {
            return _func(config);
        }
    }
}