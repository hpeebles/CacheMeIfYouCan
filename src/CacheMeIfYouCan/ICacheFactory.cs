using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public interface ICacheFactory
    {
        bool RequiresStringKeys { get; }
        ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config);
    }

    public interface ICacheFactory<TK, TV>
    {
        bool RequiresStringKeys { get; }
        ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config);
    }

    internal class CacheFactoryGenericAdaptor<TK, TV> : ICacheFactory<TK, TV>
    {
        private readonly ICacheFactory _factory;

        public CacheFactoryGenericAdaptor(ICacheFactory factory)
        {
            _factory = factory;
        }

        public bool RequiresStringKeys => _factory.RequiresStringKeys;
        
        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config)
        {
            return _factory.Build(config);
        }
    }

    internal class CacheFactoryFuncAdaptor<TK, TV> : ICacheFactory<TK, TV>
    {
        private readonly Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> _func;

        public CacheFactoryFuncAdaptor(Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> func, bool requiresStringKeys)
        {
            _func = func;
            RequiresStringKeys = requiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }

        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config)
        {
            return _func(config);
        }
    }
}