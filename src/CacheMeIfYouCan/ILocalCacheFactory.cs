using System;

namespace CacheMeIfYouCan
{
    public interface ILocalCacheFactory
    {
        ILocalCache<TK, TV> Build<TK, TV>(string cacheName);
    }
    
    public interface ILocalCacheFactory<TK, TV>
    {
        ILocalCache<TK, TV> Build(string cacheName);
    }

    internal class LocalCacheFactoryAdaptor<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory _factory;

        public LocalCacheFactoryAdaptor(ILocalCacheFactory factory)
        {
            _factory = factory;
        }
        
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return _factory.Build<TK, TV>(cacheName);
        }
    }
    
    internal class LocalCacheFactoryFuncAdaptor<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly Func<string, ILocalCache<TK, TV>> _func;

        public LocalCacheFactoryFuncAdaptor(Func<string, ILocalCache<TK, TV>> func)
        {
            _func = func;
        }

        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return _func(cacheName);
        }
    }
}