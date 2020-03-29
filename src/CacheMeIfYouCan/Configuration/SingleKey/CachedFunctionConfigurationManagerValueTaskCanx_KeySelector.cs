using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_2Params_KeySelector<TParam1, TParam2, TValue>
    {
        private readonly Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_2Params_KeySelector(Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_3Params_KeySelector(Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_4Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_5Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_6Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_7Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_8Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
}