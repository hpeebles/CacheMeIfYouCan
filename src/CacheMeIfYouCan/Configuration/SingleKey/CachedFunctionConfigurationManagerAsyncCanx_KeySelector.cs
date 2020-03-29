using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_2Params_KeySelector<TParam1, TParam2, TValue>
    {
        private readonly Func<TParam1, TParam2, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_2Params_KeySelector(Func<TParam1, TParam2, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_3Params_KeySelector(Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_4Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_5Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_6Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_7Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_8Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }

        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
}