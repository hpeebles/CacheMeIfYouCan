using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK, Task<TV>> func, string cacheName = null)
        {
            return new FunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK, TV> func, string cacheName = null)
        {
            return new FunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult(func(key)), cacheName);
        }
    }
}