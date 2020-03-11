﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerSyncCanxBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, CancellationToken, TValue> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;

        internal CachedFunctionConfigurationManagerSyncCanxBase(
            Func<TParams, CancellationToken, TValue> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, CancellationToken, TValue> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _cacheKeySelector);

            return Get;
            
            TValue Get(TParams parameters, CancellationToken cancellationToken)
            {
                var valueTask = cachedFunction.Get(parameters, cancellationToken);

                return valueTask.IsCompleted
                    ? valueTask.Result
                    : valueTask.AsTask().GetAwaiter().GetResult();
            }
        }
        
        private Func<TParams, CancellationToken, ValueTask<TValue>> ConvertFunction()
        {
            return (keys, cancellationToken) => new ValueTask<TValue>(_originalFunction(keys, cancellationToken));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerSyncCanxBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>>,
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, CancellationToken, TValue> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_1Param(
            Func<TParam, CancellationToken, TValue> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue> OnResult(
            Action<SuccessfulRequestEvent<TParam, TValue>> onSuccess = null,
            Action<ExceptionEvent<TParam>> onException = null)
        {
            OnSuccess(r => onSuccess(new SuccessfulRequestEvent<TParam, TValue>(r)));
            OnException(r => onException(new ExceptionEvent<TParam>(r)));
            return this;
        }
        
        public Func<TParam, CancellationToken, TValue> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue>>,
            ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_1Param(
            Func<TParam, CancellationToken, TValue> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_1Param<TParam, TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_1Param<TParam, TKey>> onException = null)
        {
            OnSuccess(r => onSuccess(new SuccessfulRequestEvent_1Param<TParam, TKey, TValue>(r)));
            OnException(r => onException(new ExceptionEvent_1Param<TParam, TKey>(r)));
            return this;
        }
        
        public Func<TParam, CancellationToken, TValue> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_2Params<TParam1, TParam2, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_2Params(
            Func<TParam1, TParam2, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_2Params<TParam1, TParam2, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, cancellationToken) => cachedFunction((p1, p2), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_3Params(
            Func<TParam1, TParam2, TParam3, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TParam3, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, cancellationToken) => cachedFunction((p1, p2, p3), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, cancellationToken) => cachedFunction((p1, p2, p3, p4), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }
        
        public CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6, p7), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6, p7, p8), cancellationToken);
        }
    }
}