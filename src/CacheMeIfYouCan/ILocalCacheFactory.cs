using System;

namespace CacheMeIfYouCan
{
    public interface ILocalCacheFactory
    {
        bool RequiresStringKeys { get; }
        ILocalCache<TK, TV> Build<TK, TV>(string cacheName);
    }
    
    public interface ILocalCacheFactory<TK, TV>
    {
        bool RequiresStringKeys { get; }
        ILocalCache<TK, TV> Build(string cacheName);
    }

    internal class LocalCacheFactoryAdaptor<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory _factory;

        public LocalCacheFactoryAdaptor(ILocalCacheFactory factory)
        {
            _factory = factory;
        }

        public bool RequiresStringKeys => _factory.RequiresStringKeys;
        
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return _factory.Build<TK, TV>(cacheName);
        }
    }
    
    internal class LocalCacheFactoryFuncAdaptor<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly Func<string, ILocalCache<TK, TV>> _func;

        public LocalCacheFactoryFuncAdaptor(Func<string, ILocalCache<TK, TV>> func, bool requiresStringKeys)
        {
            _func = func;
            RequiresStringKeys = requiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }

        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return _func(cacheName);
        }
    }
}