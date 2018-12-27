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

    internal class LocalCacheFactoryToGenericAdaptor<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory _factory;

        public LocalCacheFactoryToGenericAdaptor(ILocalCacheFactory factory)
        {
            _factory = factory;
        }
        
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return _factory.Build<TK, TV>(cacheName);
        }
    }
    
    internal class LocalCacheFactoryFromFuncAdaptor<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly Func<string, ILocalCache<TK, TV>> _func;

        public LocalCacheFactoryFromFuncAdaptor(Func<string, ILocalCache<TK, TV>> func)
        {
            _func = func;
        }

        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return _func(cacheName);
        }
    }
}