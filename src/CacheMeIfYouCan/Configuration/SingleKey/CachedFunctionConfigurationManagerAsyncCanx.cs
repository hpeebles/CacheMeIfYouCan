﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerAsyncCanxBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ICachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, CancellationToken, Task<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;

        internal CachedFunctionConfigurationManagerAsyncCanxBase(
            Func<TParams, CancellationToken, Task<TValue>> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, CancellationToken, Task<TValue>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(_originalFunction, _cacheKeySelector);

            return (key, cancellationToken) => cachedFunction.Get(key, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerAsyncCanxBase<TParam, TParam, TValue, ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TParam, TValue>>,
        ICachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_1Param(
            Func<TParam, CancellationToken, Task<TValue>> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> WithCacheKeySelector<TKey>(Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public Func<TParam, CancellationToken, Task<TValue>> Build()
        {
            return BuildInternal();
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_1Param(
            Func<TParam, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public Func<TParam, CancellationToken, Task<TValue>> Build()
        {
            return BuildInternal();
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_2Params(
            Func<TParam1, TParam2, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_3Params(
            Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
}