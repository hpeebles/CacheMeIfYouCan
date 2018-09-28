using System;

namespace CacheMeIfYouCan.Caches
{
    public interface ILocalCacheFactory
    {
        bool RequiresStringKeys { get; }
        ILocalCache<TK, TV> Build<TK, TV>();
    }
    
    public interface ILocalCacheFactory<TK, TV>
    {
        bool RequiresStringKeys { get; }
        ILocalCache<TK, TV> Build();
    }

    internal class LocalCacheFactoryWrapper<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory _factory;

        public LocalCacheFactoryWrapper(ILocalCacheFactory factory)
        {
            _factory = factory;
        }

        public bool RequiresStringKeys => _factory.RequiresStringKeys;
        
        public ILocalCache<TK, TV> Build()
        {
            return _factory.Build<TK, TV>();
        }
    }
    
    internal class LocalCacheFactoryFuncWrapper<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly Func<ILocalCache<TK, TV>> _func;

        public LocalCacheFactoryFuncWrapper(Func<ILocalCache<TK, TV>> func, bool requiresStringKeys)
        {
            _func = func;
            RequiresStringKeys = requiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }

        public ILocalCache<TK, TV> Build()
        {
            return _func();
        }
    }
}