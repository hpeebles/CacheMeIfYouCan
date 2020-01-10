using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class CachedInterfaceFactory
    {
        public static CachedInterfaceConfigurationManager<T> For<T>(T originalImpl) where T : class
        {
            if (!typeof(T).IsInterface)
                throw new InvalidOperationException($"Generic type argument must be an interface. '{typeof(T).Name}' is not an interface");
            
            return new CachedInterfaceConfigurationManager<T>(originalImpl);
        }
    }
}