using System;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public sealed class CachedFunctionConfigurationManagerSync_2Params_KeySelector<TParam1, TParam2, TValue>
    {
        private readonly Func<TParam1, TParam2, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_2Params_KeySelector(Func<TParam1, TParam2, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_3Params_KeySelector(Func<TParam1, TParam2, TParam3, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_4Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_5Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_6Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_7Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_8Params_KeySelector(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>(
                _originalFunction,
                cacheKeySelector);
        }
        
        internal Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> OriginalFunction => _originalFunction;
    }
}