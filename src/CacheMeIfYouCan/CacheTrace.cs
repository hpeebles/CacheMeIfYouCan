using System.Collections.Generic;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan
{
    public class CacheTrace
    {
        public FunctionCacheGetResult Result { get; internal set; }
        public List<FunctionCacheFetchResult> Fetches { get; } = new List<FunctionCacheFetchResult>();
        public List<CacheGetResult> CacheGetResults { get; } = new List<CacheGetResult>();
        public List<CacheSetResult> CacheSetResults { get; } = new List<CacheSetResult>();
        public List<CacheRemoveResult> CacheRemoveResults { get; } = new List<CacheRemoveResult>();
        public FunctionCacheException GetResultException { get; internal set; }
        public List<FunctionCacheException> FetchExceptions { get; } = new List<FunctionCacheException>();
        public List<CacheException> CacheExceptions { get; } = new List<CacheException>();
    }
}