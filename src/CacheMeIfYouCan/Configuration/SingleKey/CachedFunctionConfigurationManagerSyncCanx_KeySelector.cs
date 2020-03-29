using System;
using System.Threading;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerSyncCanx_2Params_KeySelector<TParam1, TParam2, TValue>
    {
        private readonly Func<TParam1, TParam2, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_2Params_KeySelector(Func<TParam1, TParam2, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_2Params<TParam1, TParam2, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_2Params<TParam1, TParam2, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_3Params_KeySelector(Func<TParam1, TParam2, TParam3, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_4Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_5Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_6Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_7Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_8Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> OriginalFunction => _originalFunction;
    }
}