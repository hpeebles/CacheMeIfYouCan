using System;

namespace CacheMeIfYouCan.Caches
{
    public interface ICacheFactory
    {
        bool RequiresStringKeys { get; }
        ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config, Action<Key<TK>> removeFromLocalCacheCallback = null);
    }

    public interface ICacheFactory<TK, TV>
    {
        bool RequiresStringKeys { get; }
        ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config, Action<Key<TK>> removeFromLocalCacheCallback = null);
    }

    internal class CacheFactoryWrapper<TK, TV> : ICacheFactory<TK, TV>
    {
        private readonly ICacheFactory _factory;

        public CacheFactoryWrapper(ICacheFactory factory)
        {
            _factory = factory;
        }

        public bool RequiresStringKeys => _factory.RequiresStringKeys;
        
        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config, Action<Key<TK>> removeFromLocalCacheCallback)
        {
            return _factory.Build(config, removeFromLocalCacheCallback);
        }
    }

    internal class CacheFactoryFuncWrapper<TK, TV> : ICacheFactory<TK, TV>
    {
        private readonly Func<CacheFactoryConfig<TK, TV>, Action<Key<TK>>, ICache<TK, TV>> _func;

        public CacheFactoryFuncWrapper(Func<CacheFactoryConfig<TK, TV>, Action<Key<TK>>, ICache<TK, TV>> func, bool requiresStringKeys)
        {
            _func = func;
            RequiresStringKeys = requiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }

        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config, Action<Key<TK>> removeFromLocalCacheCallback)
        {
            return _func(config, removeFromLocalCacheCallback);
        }
    }
}