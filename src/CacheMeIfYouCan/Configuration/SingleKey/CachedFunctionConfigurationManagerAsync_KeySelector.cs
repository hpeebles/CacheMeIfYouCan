using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerAsync_2Params_KeySelector<TParam1, TParam2, TValue>
    {
        private readonly Func<TParam1, TParam2, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_2Params_KeySelector(Func<TParam1, TParam2, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_2Params<TParam1, TParam2, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_2Params<TParam1, TParam2, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_3Params_KeySelector(Func<TParam1, TParam2, TParam3, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TParam3, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_4Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_5Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_6Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_7Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync_8Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TValue>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithCacheKeySelector<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
    }
}