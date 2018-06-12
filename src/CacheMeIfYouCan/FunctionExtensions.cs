using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        public static FunctionCacheConfigurationManager<T> Cached<T>(this Func<string, Task<T>> func, string cacheName = null)
        {
            return new FunctionCacheConfigurationManager<T>(func, cacheName);
        }
        
        public static FunctionCacheConfigurationManager<T> Cached<T>(this Func<string, T> func, string cacheName = null)
        {
            return new FunctionCacheConfigurationManager<T>(key => Task.FromResult(func(key)), cacheName);
        }
    }
}