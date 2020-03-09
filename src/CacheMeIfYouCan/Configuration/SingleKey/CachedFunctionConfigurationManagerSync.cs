﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerSyncBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
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
                var task = cachedFunction.Get(parameters, CancellationToken.None);

                return task.IsCompleted
                    ? task.Result
                    : task.GetAwaiter().GetResult();
            }
        }

        private Func<TParams, CancellationToken, ValueTask<TValue>> ConvertFunction()
        {
            return (keys, _) => new ValueTask<TValue>(_originalFunction(keys));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerSyncBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue>>,
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_1Param(
            Func<TParam, TValue> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue> OnResult(
            Action<SuccessfulRequestEvent<TParam, TValue>> onSuccess = null,
            Action<ExceptionEvent<TParam>> onException = null)
        {
            OnSuccess(r => onSuccess(new SuccessfulRequestEvent<TParam, TValue>(r)));
            OnException(r => onException(new ExceptionEvent<TParam>(r)));
            return this;
        }
        
        public Func<TParam, TValue> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>>,
            ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerSync_1Param(
            Func<TParam, TValue> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_1Param<TParam, TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_1Param<TParam, TKey>> onException = null)
        {
            OnSuccess(r => onSuccess(new SuccessfulRequestEvent_1Param<TParam, TKey, TValue>(r)));
            OnException(r => onException(new ExceptionEvent_1Param<TParam, TKey>(r)));
            return this;
        }
        
        public Func<TParam, TValue> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_2Params(
            Func<TParam1, TParam2, TValue> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2),
                t => cacheKeySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2) => cachedFunction((p1, p2));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_3Params(
            Func<TParam1, TParam2, TParam3, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TParam3, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3) => cachedFunction((p1, p2, p3));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }

        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4) => cachedFunction((p1, p2, p3, p4));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }
        
        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5) => cachedFunction((p1, p2, p3, p4, p5));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }

        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6) => cachedFunction((p1, p2, p3, p4, p5, p6));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }

        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7) => cachedFunction((p1, p2, p3, p4, p5, p6, p7));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8))
        { }

        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, p8) => cachedFunction((p1, p2, p3, p4, p5, p6, p7, p8));
        }
    }
}