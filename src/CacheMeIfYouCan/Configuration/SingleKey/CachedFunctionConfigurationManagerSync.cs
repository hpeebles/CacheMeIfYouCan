using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.SingleKey;
using CacheMeIfYouCan.Internal;

public abstract class CachedFunctionConfigurationManagerSyncBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ICachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, TValue> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;
        
        internal CachedFunctionConfigurationManagerSyncBase(
            Func<TParams, TValue> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, TValue> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _cacheKeySelector);

            return Get;

            TValue Get(TParams parameters)
            {
                var valueTask = cachedFunction.Get(parameters, CancellationToken.None);

                return valueTask.IsCompleted
                    ? valueTask.Result
                    : valueTask.AsTask().GetAwaiter().GetResult();
            }
        }

        private Func<TParams, CancellationToken, ValueTask<TValue>> ConvertFunction()
        {
            return (keys, _) => new ValueTask<TValue>(_originalFunction(keys));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerSyncBase<TParam, TParam, TValue, ICachedFunctionConfigurationManagerSync_1Param<TParam, TParam, TValue>>,
        ICachedFunctionConfigurationManagerSync_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_1Param(
            Func<TParam, TValue> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ICachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> WithCacheKeySelector<TKey>(Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public Func<TParam, TValue> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerSync_1Param(
            Func<TParam, TValue> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public Func<TParam, TValue> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_2Params(
            Func<TParam1, TParam2, TValue> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_3Params(
            Func<TParam1, TParam2, TParam3, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                TupleHelper.ConvertFuncToTupleInput(originalFunction),
                TupleHelper.ConvertFuncToTupleInput(cacheKeySelector))
        { }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return TupleHelper.ConvertFuncFromTupleInput(cachedFunction);
        }
    }