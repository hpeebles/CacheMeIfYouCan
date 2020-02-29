﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerValueTaskCanxBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ICachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, CancellationToken, ValueTask<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;
        
        internal CachedFunctionConfigurationManagerValueTaskCanxBase(
            Func<TParams, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, CancellationToken, ValueTask<TValue>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(_originalFunction, _cacheKeySelector);

            return Get;
            
            ValueTask<TValue> Get(TParams parameters, CancellationToken cancellationToken)
            {
                return cachedFunction.Get(parameters, cancellationToken);
            }
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerValueTaskCanxBase<TParam, TParam, TValue, ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TParam, TValue>>,
        ICachedFunctionConfigurationManagerValueTaskCanx_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_1Param(
            Func<TParam, CancellationToken, ValueTask<TValue>> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public Func<TParam, CancellationToken, ValueTask<TValue>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_1Param(
            Func<TParam, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public Func<TParam, CancellationToken, ValueTask<TValue>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_2Params(
            Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_3Params(
            Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
}