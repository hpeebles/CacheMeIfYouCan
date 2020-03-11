﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerValueTaskCanxBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanxBase(Func<TParams, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        private protected Func<TParams, TKeys, CancellationToken, ValueTask<TResponse>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async ValueTask<TResponse> Get(TParams parameters, TKeys request, CancellationToken cancellationToken)
            {
                var valueTask = cachedFunction.GetMany(parameters, request, cancellationToken);

                var results = valueTask.IsCompleted
                    ? valueTask.Result
                    : await valueTask.ConfigureAwait(false);

                return results switch
                {
                    null => default,
                    TResponse typedResponse => typedResponse,
                    _ => responseConverter(results)
                };
            }
        }

        private Func<TParams, IReadOnlyCollection<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            async ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>> Get(
                TParams parameters,
                IReadOnlyCollection<TKey> keys,
                CancellationToken cancellationToken)
            {
                if (!(keys is TKeys typedRequest))
                    typedRequest = requestConverter(keys);

                var valueTask = _originalFunction(parameters, typedRequest, cancellationToken);

                return valueTask.IsCompleted
                    ? valueTask.Result
                    : await valueTask.ConfigureAwait(false);
            }
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx<TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<Unit, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx<TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx(Func<TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((_, keys, cancellationToken) => originalFunction(keys, cancellationToken))
        { }

        public CachedFunctionConfigurationManagerValueTaskCanx<TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent<TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent<TKey>> onException = null)
        {
            OnSuccess(r => onSuccess(new SuccessfulRequestEvent<TKey, TValue>(r)));
            OnException(r => onException(new ExceptionEvent<TKey>(r)));
            return this;
        }
        
        public Func<TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (keys, cancellationToken) => cachedFunction(null, keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<TParam, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_2Params(Func<TParam, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base(originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerValueTaskCanx<TParam, TKeys, TResponse, TParam, TKey, TValue> UseFirstParamAsOuterCacheKey()
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx<TParam, TKeys, TResponse, TParam, TKey, TValue>(_originalFunction, p => p);
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx<TParam, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx<TParam, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_1ExtraParam<TParam, TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_1ExtraParam<TParam, TKey>> onException = null)
        {
            OnSuccess(r => onSuccess(new SuccessfulRequestEvent_1ExtraParam<TParam, TKey, TValue>(r)));
            OnException(r => onException(new ExceptionEvent_1ExtraParam<TParam, TKey>(r)));
            return this;
        }
        
        public Func<TParam, TKeys, CancellationToken, ValueTask<TResponse>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_3Params(Func<TParam1, TParam2, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }
        
        public Func<TParam1, TParam2, TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, keys, cancellationToken) => cachedFunction((p1, p2), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_4Params(Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_5Params(Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, keys, cancellationToken) => 
                cachedFunction((p1, p2, p3, p4), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_6Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_7Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, ValueTask<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_8Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, ValueTask<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> OnResult(
            Action<SuccessfulRequestEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent_MultiParam<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey>> onException = null)
        {
            OnSuccess(onSuccess);
            OnException(onException);
            return this;
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6, p7), keys, cancellationToken);
        }
    }
}