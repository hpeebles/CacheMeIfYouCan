﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Caching;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Caches
{
    public class MemoryCache<TK, TV> : ILocalCache<TK, TV>, ICachedItemCounter, IDisposable
    {
        private readonly MemoryCache _cache;
        
        internal MemoryCache(FunctionInfo functionInfo, int maxSizeMB = 100)
        {
            FunctionInfo = functionInfo;

            var config = new NameValueCollection
            {
                { "cacheMemoryLimitMegabytes", maxSizeMB.ToString() }
            };
            
            _cache = new MemoryCache(Guid.NewGuid().ToString(), config);
        }
        
        public string CacheType { get; } = "memory";

        public FunctionInfo FunctionInfo { get; }

        public long Count => _cache.GetCount();

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            var results = new List<GetFromCacheResult<TK, TV>>();

            foreach (var key in keys)
            {
                var valueObj = _cache.Get(key.AsString);

                if (valueObj is ValueWithExpiry<TV> value)
                {
                    results.Add(new GetFromCacheResult<TK, TV>(
                        key,
                        value,
                        value.Expiry - DateTimeOffset.UtcNow,
                        CacheType));
                }
            }

            return results;
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var expiry = DateTime.UtcNow + timeToLive;
            
            foreach (var kv in values)
                _cache.Set(kv.Key.AsString, new ValueWithExpiry<TV>(kv.Value, expiry), expiry);
        }

        public void Remove(Key<TK> key)
        {
            _cache.Remove(key.AsString);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}