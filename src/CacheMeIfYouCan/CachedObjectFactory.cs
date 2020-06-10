using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedObjects;

namespace CacheMeIfYouCan
{
    public static class CachedObjectFactory
    {
        public static ICachedObjectConfigurationManager<T> ConfigureFor<T>(Func<T> getValueFunc)
        {
            if (getValueFunc is null)
                throw new ArgumentNullException(nameof(getValueFunc));
            
            return ConfigureFor(_ => Task.FromResult(getValueFunc()));
        }
        
        public static ICachedObjectConfigurationManager<T> ConfigureFor<T>(Func<Task<T>> getValueFunc)
        {
            if (getValueFunc is null)
                throw new ArgumentNullException(nameof(getValueFunc));
            
            return new CachedObjectConfigurationManager<T>(_ => getValueFunc());
        }
        
        public static ICachedObjectConfigurationManager<T> ConfigureFor<T>(Func<CancellationToken, T> getValueFunc)
        {
            if (getValueFunc is null)
                throw new ArgumentNullException(nameof(getValueFunc));
            
            return ConfigureFor(cancellationToken => Task.FromResult(getValueFunc(cancellationToken)));
        }
        
        public static ICachedObjectConfigurationManager<T> ConfigureFor<T>(Func<CancellationToken, Task<T>> getValueFunc)
        {
            if (getValueFunc is null)
                throw new ArgumentNullException(nameof(getValueFunc));
            
            return new CachedObjectConfigurationManager<T>(getValueFunc);
        }
    }
}