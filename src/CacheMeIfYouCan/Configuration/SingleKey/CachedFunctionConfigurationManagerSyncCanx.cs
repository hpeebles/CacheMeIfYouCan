﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerSyncCanx<TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, CachedFunctionConfigurationManagerSyncCanx<TKey, TValue>>
    {
        private readonly Func<TKey, CancellationToken, TValue> _originalFunction;

        public CachedFunctionConfigurationManagerSyncCanx(Func<TKey, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }
        
        public Func<TKey, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            return Get;

            TValue Get(TKey key, CancellationToken cancellationToken)
            {
                return Task.Run(() => cachedFunction.Get(key, cancellationToken)).GetAwaiter().GetResult();
            }
        }
        
        private Func<TKey, CancellationToken, Task<TValue>> ConvertFunction()
        {
            return (keys, cancellationToken) => Task.FromResult(_originalFunction(keys, cancellationToken));
        }
    }
}